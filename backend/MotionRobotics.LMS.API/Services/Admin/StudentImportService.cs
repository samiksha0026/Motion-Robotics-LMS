using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Models;
using System.Text.RegularExpressions;

namespace MotionRobotics.LMS.API.Services.Admin
{
    /// <summary>
    /// Handles Excel-based bulk student imports.
    ///
    /// Strategy for 1 000 rows:
    ///   1. Pre-load all reference data (classes, existing emails, roll numbers) in one pass.
    ///   2. Validate every row in memory — zero extra DB round-trips per row.
    ///   3. Create Identity users sequentially (UserManager has no bulk API).
    ///   4. Batch-insert all Student records with a single SaveChangesAsync inside a transaction.
    /// </summary>
    public class StudentImportService : IStudentImportService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public StudentImportService(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ─────────────────────────────────────────────────────────────────────────
        // IMPORT
        // ─────────────────────────────────────────────────────────────────────────

        public async Task<StudentImportResultDto> ImportFromExcelAsync(IFormFile file, int schoolId)
        {
            var result = new StudentImportResultDto();

            // ── 0. Basic file validation ──────────────────────────────────────────
            if (file is null || file.Length == 0)
                throw new InvalidOperationException("No file uploaded.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".xlsx")
                throw new InvalidOperationException("Only .xlsx files are supported. Please download the template.");

            // ── 1. Pre-load reference data (one DB pass each) ─────────────────────
            var school = await _context.Schools.FindAsync(schoolId)
                ?? throw new KeyNotFoundException($"School with id {schoolId} not found.");

            var classes = await _context.Classes
                .Where(c => c.SchoolId == schoolId)
                .ToListAsync();

            var currentYear = await _context.AcademicYears
                .FirstOrDefaultAsync(a => a.IsCurrent);

            // HashSets for O(1) duplicate detection
            var existingEmails = (await _userManager.Users
                .Select(u => u.Email!.ToLower())
                .ToListAsync())
                .ToHashSet();

            var existingRollNos = (await _context.Students
                .Where(s => s.SchoolId == schoolId)
                .Select(s => s.RollNo.ToLower())
                .ToListAsync())
                .ToHashSet();

            // ── 2. Parse Excel ────────────────────────────────────────────────────
            var rawRows = ParseExcel(file);

            // ── 3. Validate every row in memory ──────────────────────────────────
            var validRows = new List<(IdentityUser User, Student Student)>();
            var batchEmails = new HashSet<string>(); // within-batch duplicate guard
            var batchRollNos = new HashSet<string>(); // within-batch duplicate guard

            for (int i = 0; i < rawRows.Count; i++)
            {
                int excelRow = i + 2; // row 1 = header → data starts at row 2
                var raw = rawRows[i];
                var rowErrors = new List<string>();

                // Required fields
                if (string.IsNullOrWhiteSpace(raw.FullName)) rowErrors.Add("FullName is required");
                if (string.IsNullOrWhiteSpace(raw.Email)) rowErrors.Add("Email is required");
                if (string.IsNullOrWhiteSpace(raw.RollNo)) rowErrors.Add("RollNo is required");

                // Grade must be a positive integer
                if (!int.TryParse(raw.Grade, out int grade) || grade <= 0)
                    rowErrors.Add("Grade must be a positive number (e.g. 5, 6, 7)");

                // Division must be exactly one letter
                if (string.IsNullOrWhiteSpace(raw.Division)
                    || raw.Division.Trim().Length != 1
                    || !char.IsLetter(raw.Division.Trim()[0]))
                    rowErrors.Add("Division must be a single letter (A–Z)");

                if (rowErrors.Count > 0)
                {
                    AddError(result, excelRow, string.Join("; ", rowErrors));
                    continue;
                }

                var emailNorm = raw.Email.Trim().ToLower();
                var rollNorm = raw.RollNo.Trim().ToLower();
                var divisionUpper = raw.Division.Trim().ToUpper();

                // Duplicate email
                if (existingEmails.Contains(emailNorm) || batchEmails.Contains(emailNorm))
                {
                    AddError(result, excelRow, $"Email '{raw.Email.Trim()}' already exists");
                    continue;
                }

                // Duplicate roll number within this school
                if (existingRollNos.Contains(rollNorm) || batchRollNos.Contains(rollNorm))
                {
                    AddError(result, excelRow, $"RollNo '{raw.RollNo.Trim()}' already exists in this school");
                    continue;
                }

                // Class matching: "Class 6-A", "Class 6A", "Grade 6A", "6-A", etc.
                var matchedClass = FindClass(classes, grade, divisionUpper);
                if (matchedClass is null)
                {
                    AddError(result, excelRow,
                        $"No class found for Grade {grade} Division {divisionUpper} in this school. " +
                        $"Create the class first (e.g. 'Class {grade}-{divisionUpper}').");
                    continue;
                }

                // Build objects (not saved yet)
                var identityUser = new IdentityUser
                {
                    UserName = raw.Email.Trim(),
                    Email = raw.Email.Trim(),
                    EmailConfirmed = true
                };

                var student = new Student
                {
                    Email = raw.Email.Trim(),
                    FullName = raw.FullName.Trim(),
                    RollNo = raw.RollNo.Trim(),
                    ParentPhone = string.IsNullOrWhiteSpace(raw.ParentPhone)
                                              ? null
                                              : raw.ParentPhone.Trim(),
                    ClassId = matchedClass.Id,
                    SchoolId = schoolId,
                    CurrentAcademicYearId = currentYear?.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                validRows.Add((identityUser, student));
                batchEmails.Add(emailNorm);
                batchRollNos.Add(rollNorm);
            }

            // ── 4. Bulk create in a transaction ──────────────────────────────────
            if (validRows.Count > 0)
            {
                await using var tx = await _context.Database.BeginTransactionAsync();
                try
                {
                    foreach (var (user, student) in validRows)
                    {
                        // Default password: Student@123
                        var createResult = await _userManager.CreateAsync(user, "Student@123");
                        if (!createResult.Succeeded)
                        {
                            var msg = string.Join(", ", createResult.Errors.Select(e => e.Description));
                            result.Errors.Add(new StudentImportErrorDto
                            {
                                Row = -1, // row unknown at this point; email identifies it
                                Error = $"Failed to create login for '{student.Email}': {msg}"
                            });
                            result.FailedCount++;
                            continue;
                        }

                        await _userManager.AddToRoleAsync(user, "Student");

                        student.UserId = user.Id;
                        _context.Students.Add(student);
                        result.SuccessCount++;
                    }

                    // Single SaveChangesAsync for all Student records
                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            }

            return result;
        }

        // ─────────────────────────────────────────────────────────────────────────
        // TEMPLATE GENERATOR
        // ─────────────────────────────────────────────────────────────────────────

        public byte[] GenerateTemplate()
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Students");

            // ── Headers ─────────────────────────────────────────────────────────
            string[] headers = { "FullName", "Email", "RollNo", "Grade", "Division", "ParentPhone" };

            for (int col = 1; col <= headers.Length; col++)
            {
                var cell = sheet.Cell(1, col);
                cell.Value = headers[col - 1];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F4E79");
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // ── Sample rows ──────────────────────────────────────────────────────
            object[][] samples =
            {
                new object[] { "Ahmed Ali",   "ahmed@school.com",  "101", "5", "A", "9876543210" },
                new object[] { "Sara Khan",   "sara@school.com",   "102", "5", "B", "9876543211" },
                new object[] { "Ravi Kumar",  "ravi@school.com",   "201", "6", "A", "9876543212" },
            };

            for (int r = 0; r < samples.Length; r++)
            {
                for (int c = 0; c < samples[r].Length; c++)
                    sheet.Cell(r + 2, c + 1).Value = XLCellValue.FromObject(samples[r][c]);

                // Light-blue alternating row
                if (r % 2 == 1)
                    sheet.Row(r + 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#DEEAF1");
            }

            // ── Notes row ────────────────────────────────────────────────────────
            int notesRow = samples.Length + 3;
            sheet.Cell(notesRow, 1).Value =
                "NOTES: Grade must be a number (5, 6, 7…). " +
                "Division must be a single letter (A, B, C…). " +
                "Default password for all imported students: Student@123";
            sheet.Cell(notesRow, 1).Style.Font.Italic = true;
            sheet.Cell(notesRow, 1).Style.Font.FontColor = XLColor.Gray;
            sheet.Range(notesRow, 1, notesRow, headers.Length).Merge();

            sheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        // ─────────────────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Reads all data rows from the first worksheet, returning raw string values.
        /// Row 1 is assumed to be the header; data starts at row 2.
        /// </summary>
        private static List<ExcelRow> ParseExcel(IFormFile file)
        {
            var rows = new List<ExcelRow>();

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var sheet = workbook.Worksheets.First();
            int lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;

            for (int r = 2; r <= lastRow; r++)
            {
                var row = sheet.Row(r);
                if (row.IsEmpty()) continue;

                rows.Add(new ExcelRow
                {
                    FullName = row.Cell(1).GetString(),
                    Email = row.Cell(2).GetString(),
                    RollNo = row.Cell(3).GetString(),
                    Grade = row.Cell(4).GetString(),
                    Division = row.Cell(5).GetString(),
                    ParentPhone = row.Cell(6).GetString()
                });
            }

            return rows;
        }

        /// <summary>
        /// Finds a class whose ClassName contains both the grade number and the
        /// division letter in the expected order, regardless of formatting.
        ///
        /// Matches: "Class 6-A", "Class 6A", "Grade 6A", "6A", "6-A", "Std 6 A"
        /// Does NOT match "Class 12-A" for Grade=1, Division=A (word-boundary guards).
        /// </summary>
        private static Class? FindClass(List<Class> classes, int grade, string division)
        {
            // Pattern: word-boundary · grade digits · optional non-digit chars · division letter · word-boundary
            var pattern = $@"\b{Regex.Escape(grade.ToString())}[^0-9]*{Regex.Escape(division)}\b";

            return classes.FirstOrDefault(c =>
                Regex.IsMatch(c.ClassName, pattern, RegexOptions.IgnoreCase));
        }

        private static void AddError(StudentImportResultDto result, int row, string error)
        {
            result.Errors.Add(new StudentImportErrorDto { Row = row, Error = error });
            result.FailedCount++;
        }

        /// <summary>Internal model for a parsed Excel row.</summary>
        private sealed class ExcelRow
        {
            public string FullName { get; init; } = string.Empty;
            public string Email { get; init; } = string.Empty;
            public string RollNo { get; init; } = string.Empty;
            public string Grade { get; init; } = string.Empty;
            public string Division { get; init; } = string.Empty;
            public string ParentPhone { get; init; } = string.Empty;
        }
    }
}
