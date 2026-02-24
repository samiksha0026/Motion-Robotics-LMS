using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.DTOs;
using MotionRobotics.LMS.API.Models;
using StudentModel = MotionRobotics.LMS.API.Models.Student;

namespace MotionRobotics.LMS.API.Controllers
{
    /// <summary>
    /// Student API Controller - Handles student experiments, exams, and certificates.
    /// Students can only access their assigned robotics level.
    /// Experiments must be completed sequentially with teacher approval.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get current student's assigned robotics level based on school level mapping.
        /// </summary>
        private async Task<RoboticsLevel?> GetStudentRoboticsLevelAsync(StudentModel student)
        {
            // Get current academic year
            var currentYear = await _context.AcademicYears
                .FirstOrDefaultAsync(ay => ay.IsCurrent);

            if (currentYear == null) return null;

            // Get the level mapping for this student's school + class + academic year
            var mapping = await _context.SchoolLevelMappings
                .Include(m => m.RoboticsLevel)
                .FirstOrDefaultAsync(m =>
                    m.SchoolId == student.SchoolId &&
                    m.ClassId == student.ClassId &&
                    m.AcademicYearId == currentYear.Id);

            return mapping?.RoboticsLevel;
        }

        /// <summary>
        /// Get current academic year
        /// </summary>
        private async Task<AcademicYear?> GetCurrentAcademicYearAsync()
        {
            return await _context.AcademicYears.FirstOrDefaultAsync(ay => ay.IsCurrent);
        }

        /// <summary>
        /// Submit experiment completion - requires teacher approval to unlock next.
        /// </summary>
        [HttpPost("complete-experiment/{experimentId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CompleteExperiment(int experimentId, [FromBody] ExperimentCompleteDto? dto)
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("User not authenticated");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Email == email);

            if (student == null)
                return NotFound("Student profile not found");

            var currentYear = await GetCurrentAcademicYearAsync();
            if (currentYear == null)
                return BadRequest("No active academic year found");

            var roboticsLevel = await GetStudentRoboticsLevelAsync(student);
            if (roboticsLevel == null)
                return BadRequest("No robotics level assigned to your class. Contact your school admin.");

            // Verify the experiment belongs to student's assigned level
            var experiment = await _context.Experiments
                .FirstOrDefaultAsync(e => e.Id == experimentId && e.RoboticsLevelId == roboticsLevel.Id);

            if (experiment == null)
                return NotFound("Experiment not found or not accessible for your level");

            // Check if previous experiments are completed and approved (sequential unlock)
            var previousExperiments = await _context.Experiments
                .Where(e => e.RoboticsLevelId == roboticsLevel.Id && e.SequenceOrder < experiment.SequenceOrder)
                .Select(e => e.Id)
                .ToListAsync();

            if (previousExperiments.Any())
            {
                var allPreviousApproved = await _context.StudentProgress
                    .Where(p => p.StudentId == student.Id &&
                               previousExperiments.Contains(p.ExperimentId) &&
                               p.AcademicYearId == currentYear.Id)
                    .AllAsync(p => p.Completed && p.IsApprovedByTeacher);

                if (!allPreviousApproved)
                    return BadRequest("Complete and get approval for previous experiments first");
            }

            // Check if already completed
            var progress = await _context.StudentProgress
                .FirstOrDefaultAsync(p =>
                    p.StudentId == student.Id &&
                    p.ExperimentId == experimentId &&
                    p.AcademicYearId == currentYear.Id);

            if (progress == null)
            {
                progress = new StudentProgress
                {
                    StudentId = student.Id,
                    StudentEmail = email,
                    ExperimentId = experimentId,
                    AcademicYearId = currentYear.Id,
                    Completed = true,
                    CompletedAt = DateTime.UtcNow,
                    SubmissionNotes = dto?.Notes,
                    SubmissionImageUrl = dto?.ImageUrl,
                    IsApprovedByTeacher = false, // Requires teacher approval
                    CreatedAt = DateTime.UtcNow
                };
                _context.StudentProgress.Add(progress);
            }
            else if (progress.Completed)
            {
                return BadRequest("Experiment already submitted. Waiting for teacher approval.");
            }
            else
            {
                progress.Completed = true;
                progress.CompletedAt = DateTime.UtcNow;
                progress.SubmissionNotes = dto?.Notes;
                progress.SubmissionImageUrl = dto?.ImageUrl;
                progress.UpdatedAt = DateTime.UtcNow;
                _context.StudentProgress.Update(progress);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Experiment submitted! Waiting for teacher approval.",
                experimentId,
                status = "pending_approval"
            });
        }

        /// <summary>
        /// Get student dashboard - shows assigned level, experiments, progress, exam status.
        /// </summary>
        [HttpGet("dashboard")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetDashboard()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("User not authenticated");

            var student = await _context.Students
                .Include(s => s.School)
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.Email == email);

            if (student == null)
                return NotFound("Student profile not found");

            var currentYear = await GetCurrentAcademicYearAsync();
            if (currentYear == null)
                return BadRequest("No active academic year found");

            var roboticsLevel = await GetStudentRoboticsLevelAsync(student);

            // If no level assigned yet
            if (roboticsLevel == null)
            {
                return Ok(new
                {
                    student = new
                    {
                        student.FullName,
                        student.Email,
                        school = student.School?.SchoolName,
                        className = student.Class?.ClassName
                    },
                    levelAssigned = false,
                    message = "No robotics level assigned to your class yet. Contact your school admin."
                });
            }

            // Get all experiments for this level
            var experiments = await _context.Experiments
                .Where(e => e.RoboticsLevelId == roboticsLevel.Id && e.IsActive)
                .OrderBy(e => e.SequenceOrder)
                .ToListAsync();

            // Get student's progress for this level
            var progressList = await _context.StudentProgress
                .Where(p => p.StudentId == student.Id && p.AcademicYearId == currentYear.Id)
                .ToListAsync();

            var experimentStatuses = experiments.Select(e =>
            {
                var prog = progressList.FirstOrDefault(p => p.ExperimentId == e.Id);
                string status = "locked";

                if (prog != null)
                {
                    if (prog.IsApprovedByTeacher)
                        status = "approved";
                    else if (prog.Completed)
                        status = "pending_approval";
                }
                else
                {
                    // Check if this is the next experiment to unlock
                    var previousExp = experiments.FirstOrDefault(x => x.SequenceOrder == e.SequenceOrder - 1);
                    if (previousExp == null || progressList.Any(p => p.ExperimentId == previousExp.Id && p.IsApprovedByTeacher))
                    {
                        status = "available";
                    }
                }

                return new
                {
                    e.Id,
                    e.Title,
                    e.Description,
                    e.SequenceOrder,
                    e.EstimatedMinutes,
                    status,
                    completedAt = prog?.CompletedAt,
                    approvedAt = prog?.ApprovedAt,
                    teacherRemarks = prog?.TeacherRemarks
                };
            }).ToList();

            var completedCount = progressList.Count(p => p.IsApprovedByTeacher);
            var totalCount = experiments.Count;
            var progressPercent = totalCount == 0 ? 0 : (int)Math.Round((double)completedCount * 100 / totalCount);

            // Check if exam is unlocked (all experiments approved)
            var allApproved = totalCount > 0 && completedCount == totalCount;

            // Get exam info
            var exam = await _context.Exams
                .FirstOrDefaultAsync(ex => ex.RoboticsLevelId == roboticsLevel.Id && ex.IsActive);

            // Check if already passed exam
            var examResult = await _context.ExamResults
                .FirstOrDefaultAsync(er =>
                    er.StudentId == student.Id &&
                    er.AcademicYearId == currentYear.Id &&
                    er.Exam!.RoboticsLevelId == roboticsLevel.Id);

            // Get certificate if exists
            var certificate = await _context.Certificates
                .FirstOrDefaultAsync(c =>
                    c.StudentId == student.Id &&
                    c.RoboticsLevelId == roboticsLevel.Id &&
                    c.AcademicYearId == currentYear.Id);

            return Ok(new
            {
                student = new
                {
                    student.FullName,
                    student.Email,
                    school = student.School?.SchoolName,
                    schoolLogo = student.School?.LogoUrl,
                    className = student.Class?.ClassName
                },
                levelAssigned = true,
                roboticsLevel = new
                {
                    roboticsLevel.Id,
                    roboticsLevel.LevelNumber,
                    roboticsLevel.Name,
                    roboticsLevel.Description
                },
                academicYear = currentYear.YearName,
                progress = new
                {
                    percent = progressPercent,
                    completedExperiments = completedCount,
                    totalExperiments = totalCount
                },
                experiments = experimentStatuses,
                exam = exam == null ? null : new
                {
                    exam.Id,
                    exam.Title,
                    exam.DurationMinutes,
                    exam.TotalQuestions,
                    exam.PassingPercentage,
                    isUnlocked = allApproved,
                    status = examResult != null ? (examResult.IsPassed ? "passed" : "failed") : (allApproved ? "available" : "locked"),
                    result = examResult == null ? null : new
                    {
                        examResult.ScoreObtained,
                        examResult.TotalMarks,
                        examResult.Percentage,
                        examResult.IsPassed,
                        examResult.AttemptedAt
                    }
                },
                certificate = certificate == null ? null : new
                {
                    certificate.CertificateNumber,
                    certificate.Title,
                    certificate.IssuedDate,
                    certificate.CertificateFileUrl
                }
            });
        }

        /// <summary>
        /// Get list of experiments for student's assigned level.
        /// </summary>
        [HttpGet("experiments")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetExperiments()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("User not authenticated");

            var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == email);
            if (student == null)
                return NotFound("Student profile not found");

            var roboticsLevel = await GetStudentRoboticsLevelAsync(student);
            if (roboticsLevel == null)
                return BadRequest("No robotics level assigned to your class");

            var currentYear = await GetCurrentAcademicYearAsync();

            var experiments = await _context.Experiments
                .Where(e => e.RoboticsLevelId == roboticsLevel.Id && e.IsActive)
                .OrderBy(e => e.SequenceOrder)
                .ToListAsync();

            var progressList = await _context.StudentProgress
                .Where(p => p.StudentId == student.Id && p.AcademicYearId == currentYear!.Id)
                .ToListAsync();

            var result = experiments.Select(e =>
            {
                var prog = progressList.FirstOrDefault(p => p.ExperimentId == e.Id);
                string status = "locked";

                if (prog != null)
                {
                    status = prog.IsApprovedByTeacher ? "approved" : (prog.Completed ? "pending_approval" : "locked");
                }
                else
                {
                    var previousExp = experiments.FirstOrDefault(x => x.SequenceOrder == e.SequenceOrder - 1);
                    if (previousExp == null || progressList.Any(p => p.ExperimentId == previousExp.Id && p.IsApprovedByTeacher))
                    {
                        status = "available";
                    }
                }

                return new
                {
                    e.Id,
                    e.Title,
                    e.Description,
                    e.Objective,
                    e.Components,
                    e.Procedure,
                    e.WiringDiagram,
                    e.CircuitDiagram,
                    e.CodeSnippet,
                    e.DemoVideoUrl,
                    e.SafetyNotes,
                    e.SequenceOrder,
                    e.EstimatedMinutes,
                    status
                };
            });

            return Ok(new
            {
                level = new { roboticsLevel.LevelNumber, roboticsLevel.Name },
                experiments = result
            });
        }

        /// <summary>
        /// Submit exam - auto-generates certificate if passed.
        /// </summary>
        [HttpPost("submit-exam/{examId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SubmitExam(int examId, [FromBody] ExamSubmissionDto submission)
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("User not authenticated");

            var student = await _context.Students
                .Include(s => s.School)
                .FirstOrDefaultAsync(s => s.Email == email);

            if (student == null)
                return NotFound("Student profile not found");

            var currentYear = await GetCurrentAcademicYearAsync();
            if (currentYear == null)
                return BadRequest("No active academic year found");

            var roboticsLevel = await GetStudentRoboticsLevelAsync(student);
            if (roboticsLevel == null)
                return BadRequest("No robotics level assigned");

            // Verify exam belongs to student's level
            var exam = await _context.Exams
                .FirstOrDefaultAsync(e => e.Id == examId && e.RoboticsLevelId == roboticsLevel.Id);

            if (exam == null)
                return NotFound("Exam not found or not accessible");

            // Check if all experiments are approved
            var totalExperiments = await _context.Experiments
                .CountAsync(e => e.RoboticsLevelId == roboticsLevel.Id && e.IsActive);

            var approvedExperiments = await _context.StudentProgress
                .CountAsync(p => p.StudentId == student.Id &&
                                p.AcademicYearId == currentYear.Id &&
                                p.IsApprovedByTeacher &&
                                p.Experiment!.RoboticsLevelId == roboticsLevel.Id);

            if (totalExperiments == 0 || approvedExperiments < totalExperiments)
                return BadRequest("Complete all experiments before taking the exam");

            // Check if already attempted
            var existingResult = await _context.ExamResults
                .FirstOrDefaultAsync(er => er.StudentId == student.Id && er.ExamId == examId && er.AcademicYearId == currentYear.Id);

            if (existingResult != null)
                return BadRequest("You have already attempted this exam");

            // Calculate result
            var percentage = exam.TotalMarks > 0 ? (submission.Score / exam.TotalMarks) * 100 : 0;
            var passed = percentage >= exam.PassingPercentage;

            var examResult = new ExamResult
            {
                StudentId = student.Id,
                ExamId = examId,
                AcademicYearId = currentYear.Id,
                ScoreObtained = submission.Score,
                TotalMarks = exam.TotalMarks,
                Percentage = percentage,
                IsPassed = passed,
                AnswersJson = submission.AnswersJson,
                TimeTakenSeconds = submission.TimeTakenSeconds,
                AttemptedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.ExamResults.Add(examResult);

            Certificate? certificate = null;

            // Generate certificate if passed
            if (passed)
            {
                certificate = new Certificate
                {
                    CertificateNumber = $"MR-{currentYear.YearName.Replace("-", "")}-{student.SchoolId}-{student.Id}-{roboticsLevel.LevelNumber}",
                    StudentId = student.Id,
                    StudentEmail = email,
                    StudentName = student.FullName,
                    SchoolId = student.SchoolId,
                    SchoolName = student.School?.SchoolName ?? "",
                    SchoolLogoUrl = student.School?.LogoUrl,
                    RoboticsLevelId = roboticsLevel.Id,
                    LevelName = roboticsLevel.Name,
                    LevelNumber = roboticsLevel.LevelNumber,
                    AcademicYearId = currentYear.Id,
                    AcademicYearName = currentYear.YearName,
                    ExamScore = submission.Score,
                    PassingScore = exam.TotalMarks * exam.PassingPercentage / 100,
                    Title = $"Certificate of Completion - {roboticsLevel.Name}",
                    IssuedDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Certificates.Add(certificate);
                await _context.SaveChangesAsync();

                examResult.CertificateId = certificate.Id;
                _context.ExamResults.Update(examResult);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = passed ? "Congratulations! You passed the exam. Certificate issued." : "Exam completed but not passed.",
                result = new
                {
                    examResult.ScoreObtained,
                    examResult.TotalMarks,
                    examResult.Percentage,
                    examResult.IsPassed
                },
                certificate = certificate == null ? null : new
                {
                    certificate.CertificateNumber,
                    certificate.Title,
                    certificate.IssuedDate
                }
            });
        }
    }
}
