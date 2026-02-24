using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Services.Admin
{
    public interface IReportService
    {
        Task<ComprehensiveStudentReportDto> GetStudentComprehensiveReportAsync(int studentId);
        Task<ComprehensiveSchoolReportDto> GetSchoolComprehensiveReportAsync(int schoolId);
        Task<PeriodReportDto> GetPeriodReportAsync(DateTime startDate, DateTime endDate, int? schoolId = null);
        Task<List<TopPerformerDto>> GetTopPerformersAsync(int? schoolId = null, int limit = 10);
    }

    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ComprehensiveStudentReportDto> GetStudentComprehensiveReportAsync(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.School)
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
                throw new KeyNotFoundException("Student not found");

            // Get attendance data
            var attendances = await _context.Attendances
                .Where(a => a.StudentId == studentId)
                .ToListAsync();

            var presentDays = attendances.Count(a => a.IsPresent);
            var totalDays = attendances.Count;
            var attendancePercentage = totalDays > 0 ? Math.Round((double)presentDays / totalDays * 100, 2) : 0;

            // Get progress data
            var progress = await _context.StudentProgress
                .Include(p => p.Experiment)
                    .ThenInclude(e => e!.RoboticsLevel)
                .Where(p => p.StudentId == studentId)
                .ToListAsync();

            var totalExperiments = await _context.Experiments.CountAsync();
            var completedExperiments = progress.Count(p => p.Completed);
            var approvedExperiments = progress.Count(p => p.IsApprovedByTeacher);

            // Get level-wise progress
            var levels = await _context.RoboticsLevels.OrderBy(l => l.LevelNumber).ToListAsync();
            var levelProgress = new List<LevelProgressDto>();
            foreach (var level in levels)
            {
                var levelExperiments = await _context.Experiments.CountAsync(e => e.RoboticsLevelId == level.Id);
                var levelCompleted = progress.Count(p => p.Experiment?.RoboticsLevelId == level.Id && p.Completed);

                levelProgress.Add(new LevelProgressDto
                {
                    LevelNumber = level.LevelNumber,
                    LevelName = level.Name,
                    TotalExperiments = levelExperiments,
                    CompletedExperiments = levelCompleted,
                    IsCompleted = levelExperiments > 0 && levelCompleted >= levelExperiments
                });
            }

            // Get exam data
            var examResults = await _context.ExamResults
                .Include(r => r.Exam)
                    .ThenInclude(e => e!.RoboticsLevel)
                .Where(r => r.StudentId == studentId)
                .OrderByDescending(r => r.AttemptedAt)
                .ToListAsync();

            var examsPassed = examResults.Count(r => r.IsPassed);
            var avgScore = examResults.Any() ? Math.Round((double)examResults.Average(r => r.ScoreObtained), 2) : 0;
            var highestScore = examResults.Any() ? (double)examResults.Max(r => r.ScoreObtained) : 0;

            // Get certificates
            var certificates = await _context.Certificates
                .Where(c => c.StudentId == studentId)
                .OrderByDescending(c => c.IssuedDate)
                .ToListAsync();

            // Get current level from level mapping
            var currentLevel = "Not Assigned";
            if (student.SchoolId > 0 && student.ClassId > 0 && student.CurrentAcademicYearId.HasValue)
            {
                var levelMapping = await _context.SchoolLevelMappings
                    .Include(m => m.RoboticsLevel)
                    .FirstOrDefaultAsync(m => m.SchoolId == student.SchoolId &&
                                             m.ClassId == student.ClassId &&
                                             m.AcademicYearId == student.CurrentAcademicYearId.Value);
                if (levelMapping?.RoboticsLevel != null)
                    currentLevel = levelMapping.RoboticsLevel.Name;
            }

            return new ComprehensiveStudentReportDto
            {
                StudentInfo = new StudentInfoDto
                {
                    StudentId = student.Id,
                    FullName = student.FullName,
                    Email = student.Email,
                    RollNumber = student.RollNo,
                    SchoolName = student.School?.SchoolName ?? "Unknown",
                    ClassName = student.Class?.ClassName ?? "Not Assigned",
                    CurrentLevel = currentLevel,
                    EnrollmentDate = student.CreatedAt,
                    IsActive = student.IsActive
                },
                AttendanceReport = new AttendanceReportSectionDto
                {
                    TotalDays = totalDays,
                    PresentDays = presentDays,
                    AbsentDays = totalDays - presentDays,
                    AttendancePercentage = attendancePercentage,
                    AttendanceGrade = GetAttendanceGrade(attendancePercentage)
                },
                ProgressReport = new ProgressReportSectionDto
                {
                    TotalExperiments = totalExperiments,
                    CompletedExperiments = completedExperiments,
                    ApprovedExperiments = approvedExperiments,
                    CompletionPercentage = totalExperiments > 0 ? Math.Round((double)completedExperiments / totalExperiments * 100, 2) : 0,
                    ProgressStatus = GetProgressStatus(completedExperiments, totalExperiments),
                    LevelProgress = levelProgress
                },
                ExamReport = new ExamReportSectionDto
                {
                    TotalExamsTaken = examResults.Count,
                    ExamsPassed = examsPassed,
                    ExamsFailed = examResults.Count - examsPassed,
                    AverageScore = avgScore,
                    HighestScore = highestScore,
                    ExamResults = examResults.Select(r => new ExamResultSummaryDto
                    {
                        ExamId = r.ExamId,
                        ExamTitle = r.Exam?.Title ?? "Unknown",
                        LevelName = r.Exam?.RoboticsLevel?.Name ?? "Unknown",
                        Score = (double)r.ScoreObtained,
                        PassingScore = (double)(r.Exam?.PassingPercentage ?? 0),
                        Passed = r.IsPassed,
                        CompletedAt = r.AttemptedAt
                    }).ToList()
                },
                CertificateReport = new CertificateReportSectionDto
                {
                    TotalCertificates = certificates.Count,
                    Certificates = certificates.Select(c => new CertificateSummaryDto
                    {
                        CertificateId = c.Id,
                        CertificateNumber = c.CertificateNumber,
                        LevelName = c.LevelName,
                        Title = c.Title,
                        IssuedDate = c.IssuedDate
                    }).ToList()
                }
            };
        }

        public async Task<ComprehensiveSchoolReportDto> GetSchoolComprehensiveReportAsync(int schoolId)
        {
            var school = await _context.Schools.FirstOrDefaultAsync(s => s.Id == schoolId);
            if (school == null)
                throw new KeyNotFoundException("School not found");

            var students = await _context.Students
                .Where(s => s.SchoolId == schoolId)
                .ToListAsync();

            var teachers = await _context.Teachers
                .Where(t => t.SchoolId == schoolId)
                .ToListAsync();

            var classes = await _context.Classes
                .Where(c => c.SchoolId == schoolId)
                .ToListAsync();

            var certificates = await _context.Certificates
                .Where(c => c.SchoolId == schoolId)
                .CountAsync();

            // Calculate attendance
            var attendances = await _context.Attendances
                .Where(a => students.Select(s => s.Id).Contains(a.StudentId))
                .ToListAsync();
            var avgAttendance = attendances.Any()
                ? Math.Round((double)attendances.Count(a => a.IsPresent) / attendances.Count * 100, 2)
                : 0;

            // Calculate average exam score
            var examResults = await _context.ExamResults
                .Where(r => students.Select(s => s.Id).Contains(r.StudentId))
                .ToListAsync();
            var avgExamScore = examResults.Any() ? Math.Round((double)examResults.Average(r => r.ScoreObtained), 2) : 0;

            // Get teacher-class assignments
            var teacherClasses = await _context.TeacherClasses
                .Include(tc => tc.Teacher)
                .Where(tc => classes.Select(c => c.Id).Contains(tc.ClassId))
                .ToListAsync();

            // Class summaries
            var classSummaries = new List<ClassSummaryReportDto>();
            foreach (var cls in classes)
            {
                var classStudents = students.Where(s => s.ClassId == cls.Id).ToList();
                var classAttendances = attendances.Where(a => classStudents.Select(s => s.Id).Contains(a.StudentId)).ToList();
                var classAvgAttendance = classAttendances.Any()
                    ? Math.Round((double)classAttendances.Count(a => a.IsPresent) / classAttendances.Count * 100, 2)
                    : 0;

                // Calculate progress
                var classProgress = await _context.StudentProgress
                    .Where(p => classStudents.Select(s => s.Id).Contains(p.StudentId) && p.Completed)
                    .CountAsync();
                var totalExperiments = await _context.Experiments.CountAsync();
                var avgProgress = classStudents.Any() && totalExperiments > 0
                    ? Math.Round((double)classProgress / (classStudents.Count * totalExperiments) * 100, 2)
                    : 0;

                // Get assigned teacher
                var assignedTeacher = teacherClasses.FirstOrDefault(tc => tc.ClassId == cls.Id)?.Teacher?.FullName ?? "Not Assigned";

                classSummaries.Add(new ClassSummaryReportDto
                {
                    ClassId = cls.Id,
                    ClassName = cls.ClassName,
                    LevelName = "N/A",  // Level determined by school mapping
                    TeacherName = assignedTeacher,
                    StudentCount = classStudents.Count,
                    AverageAttendance = classAvgAttendance,
                    AverageProgress = avgProgress
                });
            }

            // Get top performers
            var topPerformers = await GetSchoolTopPerformersAsync(schoolId);

            return new ComprehensiveSchoolReportDto
            {
                SchoolInfo = new SchoolInfoDto
                {
                    SchoolId = school.Id,
                    SchoolName = school.SchoolName,
                    SchoolCode = school.SchoolCode,
                    Address = school.Address ?? string.Empty,
                    ContactEmail = school.ContactEmail ?? string.Empty,
                    ContactPhone = school.ContactPhone ?? string.Empty,
                    IsActive = school.IsActive
                },
                Statistics = new SchoolStatisticsDto
                {
                    TotalStudents = students.Count,
                    ActiveStudents = students.Count(s => s.IsActive),
                    TotalTeachers = teachers.Count,
                    TotalClasses = classes.Count,
                    TotalCertificatesIssued = certificates,
                    AverageAttendance = avgAttendance,
                    AverageExamScore = avgExamScore
                },
                ClassReports = classSummaries.OrderBy(c => c.ClassName).ToList(),
                TopPerformers = topPerformers
            };
        }

        public async Task<PeriodReportDto> GetPeriodReportAsync(DateTime startDate, DateTime endDate, int? schoolId = null)
        {
            var studentsQuery = _context.Students.AsQueryable();
            if (schoolId.HasValue)
                studentsQuery = studentsQuery.Where(s => s.SchoolId == schoolId.Value);

            var totalStudents = await studentsQuery.CountAsync();
            var newEnrollments = await studentsQuery
                .Where(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate)
                .CountAsync();

            var studentIds = await studentsQuery.Select(s => s.Id).ToListAsync();

            // Certificates in period
            var certificatesQuery = _context.Certificates.AsQueryable();
            if (schoolId.HasValue)
                certificatesQuery = certificatesQuery.Where(c => c.SchoolId == schoolId.Value);
            var certificatesIssued = await certificatesQuery
                .Where(c => c.IssuedDate >= startDate && c.IssuedDate <= endDate)
                .CountAsync();

            // Exams in period - count exam results taken
            var examsConducted = await _context.ExamResults
                .Where(r => studentIds.Contains(r.StudentId) &&
                           r.AttemptedAt >= startDate && r.AttemptedAt <= endDate)
                .Select(r => r.ExamId)
                .Distinct()
                .CountAsync();

            // Attendance in period
            var attendances = await _context.Attendances
                .Where(a => studentIds.Contains(a.StudentId) &&
                           a.AttendanceDate >= startDate && a.AttendanceDate <= endDate)
                .ToListAsync();
            var avgAttendance = attendances.Any()
                ? Math.Round((double)attendances.Count(a => a.IsPresent) / attendances.Count * 100, 2)
                : 0;

            // Exam scores in period
            var examResults = await _context.ExamResults
                .Where(r => studentIds.Contains(r.StudentId) &&
                           r.AttemptedAt >= startDate && r.AttemptedAt <= endDate)
                .ToListAsync();
            var avgExamScore = examResults.Any() ? Math.Round((double)examResults.Average(r => r.ScoreObtained), 2) : 0;

            // Level-wise data
            var levels = await _context.RoboticsLevels.OrderBy(l => l.LevelNumber).ToListAsync();
            var levelWiseData = new List<LevelWiseReportDto>();
            foreach (var level in levels)
            {
                // Count students with level mapping for this level
                var levelStudentIds = await _context.SchoolLevelMappings
                    .Where(m => m.RoboticsLevelId == level.Id && (!schoolId.HasValue || m.SchoolId == schoolId.Value))
                    .Select(m => m.ClassId)
                    .Distinct()
                    .ToListAsync();

                var levelStudents = await _context.Students
                    .Where(s => levelStudentIds.Contains(s.ClassId) && (!schoolId.HasValue || s.SchoolId == schoolId.Value))
                    .CountAsync();

                var levelCertificates = await _context.Certificates
                    .Where(c => (!schoolId.HasValue || c.SchoolId == schoolId.Value) &&
                               c.RoboticsLevelId == level.Id &&
                               c.IssuedDate >= startDate && c.IssuedDate <= endDate)
                    .CountAsync();

                var levelExamResults = await _context.ExamResults
                    .Include(r => r.Exam)
                    .Where(r => studentIds.Contains(r.StudentId) &&
                               r.Exam != null && r.Exam.RoboticsLevelId == level.Id &&
                               r.AttemptedAt >= startDate && r.AttemptedAt <= endDate)
                    .ToListAsync();

                levelWiseData.Add(new LevelWiseReportDto
                {
                    LevelId = level.Id,
                    LevelName = level.Name,
                    StudentsInLevel = levelStudents,
                    StudentsCompleted = levelCertificates,
                    AverageScore = levelExamResults.Any() ? Math.Round((double)levelExamResults.Average(r => r.ScoreObtained), 2) : 0
                });
            }

            var days = (endDate - startDate).Days;
            string period = days <= 7 ? "Weekly" : days <= 31 ? "Monthly" : "Custom";

            return new PeriodReportDto
            {
                StartDate = startDate,
                EndDate = endDate,
                Period = period,
                TotalStudentsEnrolled = totalStudents,
                NewEnrollments = newEnrollments,
                CertificatesIssued = certificatesIssued,
                ExamsConducted = examsConducted,
                AverageAttendance = avgAttendance,
                AverageExamScore = avgExamScore,
                LevelWiseData = levelWiseData
            };
        }

        public async Task<List<TopPerformerDto>> GetTopPerformersAsync(int? schoolId = null, int limit = 10)
        {
            var performers = new List<TopPerformerDto>();

            var studentsQuery = _context.Students
                .Include(s => s.Class)
                .Where(s => s.IsActive);
            if (schoolId.HasValue)
                studentsQuery = studentsQuery.Where(s => s.SchoolId == schoolId.Value);

            var students = await studentsQuery.ToListAsync();
            var studentIds = students.Select(s => s.Id).ToList();

            // Top by attendance
            var attendanceGroups = await _context.Attendances
                .Where(a => studentIds.Contains(a.StudentId))
                .GroupBy(a => a.StudentId)
                .Select(g => new
                {
                    StudentId = g.Key,
                    Percentage = g.Count() > 0 ? (double)g.Count(a => a.IsPresent) / g.Count() * 100 : 0
                })
                .OrderByDescending(a => a.Percentage)
                .Take(limit)
                .ToListAsync();

            foreach (var att in attendanceGroups)
            {
                var student = students.First(s => s.Id == att.StudentId);
                performers.Add(new TopPerformerDto
                {
                    StudentId = student.Id,
                    StudentName = student.FullName,
                    ClassName = student.Class?.ClassName ?? "Unknown",
                    Score = Math.Round(att.Percentage, 2),
                    Category = "Attendance"
                });
            }

            // Top by exam scores
            var examGroups = await _context.ExamResults
                .Where(r => studentIds.Contains(r.StudentId))
                .GroupBy(r => r.StudentId)
                .Select(g => new
                {
                    StudentId = g.Key,
                    AvgScore = g.Average(r => r.ScoreObtained)
                })
                .OrderByDescending(e => e.AvgScore)
                .Take(limit)
                .ToListAsync();

            foreach (var exam in examGroups)
            {
                var student = students.First(s => s.Id == exam.StudentId);
                performers.Add(new TopPerformerDto
                {
                    StudentId = student.Id,
                    StudentName = student.FullName,
                    ClassName = student.Class?.ClassName ?? "Unknown",
                    Score = Math.Round((double)exam.AvgScore, 2),
                    Category = "Exam"
                });
            }

            // Top by progress
            var totalExperiments = await _context.Experiments.CountAsync();
            if (totalExperiments > 0)
            {
                var progressGroups = await _context.StudentProgress
                    .Where(p => studentIds.Contains(p.StudentId) && p.Completed)
                    .GroupBy(p => p.StudentId)
                    .Select(g => new
                    {
                        StudentId = g.Key,
                        CompletedCount = g.Count()
                    })
                    .OrderByDescending(p => p.CompletedCount)
                    .Take(limit)
                    .ToListAsync();

                foreach (var prog in progressGroups)
                {
                    var student = students.First(s => s.Id == prog.StudentId);
                    performers.Add(new TopPerformerDto
                    {
                        StudentId = student.Id,
                        StudentName = student.FullName,
                        ClassName = student.Class?.ClassName ?? "Unknown",
                        Score = Math.Round((double)prog.CompletedCount / totalExperiments * 100, 2),
                        Category = "Progress"
                    });
                }
            }

            return performers;
        }

        private async Task<List<TopPerformerDto>> GetSchoolTopPerformersAsync(int schoolId)
        {
            return await GetTopPerformersAsync(schoolId, 5);
        }

        private static string GetAttendanceGrade(double percentage)
        {
            return percentage switch
            {
                >= 95 => "Excellent",
                >= 85 => "Very Good",
                >= 75 => "Good",
                >= 60 => "Satisfactory",
                >= 50 => "Needs Improvement",
                _ => "Poor"
            };
        }

        private static string GetProgressStatus(int completed, int total)
        {
            if (total == 0) return "Not Started";
            var percentage = (double)completed / total * 100;
            return percentage switch
            {
                >= 100 => "Completed",
                >= 75 => "Advanced",
                >= 50 => "Intermediate",
                >= 25 => "Beginner",
                > 0 => "Just Started",
                _ => "Not Started"
            };
        }
    }
}
