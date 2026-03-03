using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Helpers;
using MotionRobotics.LMS.API.Models;

namespace MotionRobotics.LMS.API.Services
{
    public interface ICertificateService
    {
        // Certificate management
        Task<CertificateListDto> GetAllCertificatesAsync(int? schoolId = null, int? roboticsLevelId = null, int? academicYearId = null);
        Task<CertificateDetailDto?> GetCertificateByIdAsync(int certificateId);
        Task<CertificateDetailDto?> GetCertificateByNumberAsync(string certificateNumber);
        Task<CertificateVerificationDto> VerifyCertificateAsync(string certificateNumber);
        Task<string> GenerateCertificateHtmlAsync(int certificateId);
        Task<bool> RegenerateCertificateAsync(int certificateId, string? customTitle = null);

        // Progress tracking
        Task<StudentProgressOverviewDto?> GetStudentProgressAsync(int studentId);
        Task<List<StudentProgressOverviewDto>> GetSchoolProgressAsync(int schoolId, int? classId = null, int? roboticsLevelId = null);
        Task<ProgressStatisticsDto> GetProgressStatisticsAsync(int? schoolId = null);
        Task<List<LevelProgressStatsDto>> GetLevelWiseStatisticsAsync(int? schoolId = null);

        // Teacher progress view
        Task<List<StudentProgressOverviewDto>> GetTeacherStudentsProgressAsync(int teacherId, int? classId = null);

        // Helper
        Task<int?> GetTeacherIdByUserIdAsync(string userId);
    }

    public class CertificateService : ICertificateService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CertificateService> _logger;

        public CertificateService(ApplicationDbContext context, ILogger<CertificateService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Certificate Management

        public async Task<CertificateListDto> GetAllCertificatesAsync(int? schoolId = null, int? roboticsLevelId = null, int? academicYearId = null)
        {
            var query = _context.Certificates
                .Include(c => c.Student)
                    .ThenInclude(s => s!.Class)
                .Include(c => c.School)
                .Include(c => c.RoboticsLevel)
                .Include(c => c.AcademicYear)
                .AsQueryable();

            if (schoolId.HasValue)
                query = query.Where(c => c.SchoolId == schoolId.Value);

            if (roboticsLevelId.HasValue)
                query = query.Where(c => c.RoboticsLevelId == roboticsLevelId.Value);

            if (academicYearId.HasValue)
                query = query.Where(c => c.AcademicYearId == academicYearId.Value);

            var certificates = await query
                .OrderByDescending(c => c.IssuedDate)
                .ToListAsync();

            return new CertificateListDto
            {
                Certificates = certificates.Select(c => MapToCertificateDetailDto(c)).ToList(),
                TotalCount = certificates.Count
            };
        }

        public async Task<CertificateDetailDto?> GetCertificateByIdAsync(int certificateId)
        {
            var certificate = await _context.Certificates
                .Include(c => c.Student)
                    .ThenInclude(s => s!.Class)
                .Include(c => c.School)
                .Include(c => c.RoboticsLevel)
                .Include(c => c.AcademicYear)
                .FirstOrDefaultAsync(c => c.Id == certificateId);

            return certificate == null ? null : MapToCertificateDetailDto(certificate);
        }

        public async Task<CertificateDetailDto?> GetCertificateByNumberAsync(string certificateNumber)
        {
            var certificate = await _context.Certificates
                .Include(c => c.Student)
                    .ThenInclude(s => s!.Class)
                .Include(c => c.School)
                .Include(c => c.RoboticsLevel)
                .Include(c => c.AcademicYear)
                .FirstOrDefaultAsync(c => c.CertificateNumber == certificateNumber);

            return certificate == null ? null : MapToCertificateDetailDto(certificate);
        }

        public async Task<CertificateVerificationDto> VerifyCertificateAsync(string certificateNumber)
        {
            var certificate = await _context.Certificates
                .Include(c => c.School)
                .Include(c => c.RoboticsLevel)
                .Include(c => c.AcademicYear)
                .FirstOrDefaultAsync(c => c.CertificateNumber == certificateNumber);

            if (certificate == null)
            {
                return new CertificateVerificationDto
                {
                    IsValid = false,
                    Message = "Certificate not found. Please check the certificate number."
                };
            }

            return new CertificateVerificationDto
            {
                IsValid = true,
                Message = "Certificate verified successfully.",
                Certificate = new CertificatePublicDto
                {
                    CertificateNumber = certificate.CertificateNumber,
                    StudentName = certificate.StudentName,
                    SchoolName = certificate.SchoolName,
                    LevelName = certificate.LevelName,
                    LevelNumber = certificate.LevelNumber,
                    AcademicYearName = certificate.AcademicYear?.YearName ?? certificate.AcademicYearName,
                    IssuedDate = certificate.IssuedDate,
                    Title = certificate.Title
                }
            };
        }

        public async Task<string> GenerateCertificateHtmlAsync(int certificateId)
        {
            var certificate = await _context.Certificates
                .Include(c => c.School)
                .Include(c => c.RoboticsLevel)
                .Include(c => c.AcademicYear)
                .FirstOrDefaultAsync(c => c.Id == certificateId);

            if (certificate == null)
                return string.Empty;

            // Update denormalized fields if needed
            certificate.SchoolName = certificate.School?.SchoolName ?? certificate.SchoolName;
            certificate.SchoolLogoUrl = certificate.School?.LogoUrl ?? certificate.SchoolLogoUrl;
            certificate.AcademicYearName = certificate.AcademicYear?.YearName ?? certificate.AcademicYearName;

            return CertificateGenerator.GenerateCertificateHtml(certificate);
        }

        public async Task<bool> RegenerateCertificateAsync(int certificateId, string? customTitle = null)
        {
            var certificate = await _context.Certificates.FindAsync(certificateId);
            if (certificate == null) return false;

            if (!string.IsNullOrEmpty(customTitle))
                certificate.Title = customTitle;

            // Regenerate certificate number if needed
            certificate.CertificateNumber = CertificateGenerator.GenerateCertificateNumber(
                certificate.StudentId,
                certificate.RoboticsLevelId,
                certificate.AcademicYearId);

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Progress Tracking

        public async Task<StudentProgressOverviewDto?> GetStudentProgressAsync(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.School)
                .Include(s => s.Class)
                .Include(s => s.CurrentAcademicYear)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) return null;

            // Get student's robotics level
            var roboticsLevelId = await GetStudentRoboticsLevelAsync(student);
            if (!roboticsLevelId.HasValue)
            {
                return new StudentProgressOverviewDto
                {
                    StudentId = student.Id,
                    StudentName = student.FullName,
                    StudentRollNo = student.RollNo,
                    SchoolId = student.SchoolId,
                    SchoolName = student.School?.SchoolName ?? "",
                    ClassName = student.Class?.ClassName ?? "",
                    AcademicYearName = student.CurrentAcademicYear?.YearName ?? "",
                    RoboticsLevelName = "Not Assigned",
                    LevelNumber = 0,
                    TotalExperiments = 0
                };
            }

            var roboticsLevel = await _context.RoboticsLevels.FindAsync(roboticsLevelId.Value);

            // Get experiments for this level
            var experiments = await _context.Experiments
                .Where(e => e.RoboticsLevelId == roboticsLevelId.Value)
                .OrderBy(e => e.SequenceOrder)
                .ToListAsync();

            // Get student's progress
            var progress = await _context.StudentProgress
                .Include(p => p.ApprovedByTeacher)
                .Where(p => p.StudentId == studentId &&
                           p.Experiment!.RoboticsLevelId == roboticsLevelId.Value)
                .ToListAsync();

            // Check if student has passed exam
            var exam = await _context.Exams
                .FirstOrDefaultAsync(e => e.RoboticsLevelId == roboticsLevelId.Value && e.IsActive);

            var hasPassedExam = exam != null && await _context.ExamResults
                .AnyAsync(r => r.StudentId == studentId && r.ExamId == exam.Id && r.IsPassed);

            // Check if certificate exists
            var hasCertificate = await _context.Certificates
                .AnyAsync(c => c.StudentId == studentId && c.RoboticsLevelId == roboticsLevelId.Value);

            var completedCount = progress.Count(p => p.Completed);
            var approvedCount = progress.Count(p => p.IsApprovedByTeacher);
            var pendingCount = progress.Count(p => p.Completed && !p.IsApprovedByTeacher);

            var experimentDetails = experiments.Select(e =>
            {
                var p = progress.FirstOrDefault(pr => pr.ExperimentId == e.Id);
                return new ExperimentProgressDetailDto
                {
                    ExperimentId = e.Id,
                    ExperimentTitle = e.Title,
                    SequenceOrder = e.SequenceOrder,
                    IsCompleted = p?.Completed ?? false,
                    CompletedAt = p?.CompletedAt,
                    IsApproved = p?.IsApprovedByTeacher ?? false,
                    ApprovedAt = p?.ApprovedAt,
                    ApprovedByTeacher = p?.ApprovedByTeacher?.FullName,
                    TeacherRemarks = p?.TeacherRemarks,
                    Status = CertificateGenerator.GetProgressStatus(p?.Completed ?? false, p?.IsApprovedByTeacher ?? false)
                };
            }).ToList();

            return new StudentProgressOverviewDto
            {
                StudentId = student.Id,
                StudentName = student.FullName,
                StudentRollNo = student.RollNo,
                SchoolId = student.SchoolId,
                SchoolName = student.School?.SchoolName ?? "",
                ClassName = student.Class?.ClassName ?? "",
                RoboticsLevelName = roboticsLevel?.Name ?? "",
                LevelNumber = roboticsLevel?.LevelNumber ?? 0,
                AcademicYearName = student.CurrentAcademicYear?.YearName ?? "",
                TotalExperiments = experiments.Count,
                CompletedExperiments = completedCount,
                ApprovedExperiments = approvedCount,
                PendingApproval = pendingCount,
                CompletionPercentage = experiments.Count > 0 ? Math.Round((decimal)approvedCount / experiments.Count * 100, 2) : 0,
                IsEligibleForExam = approvedCount >= experiments.Count && experiments.Count > 0,
                HasPassedExam = hasPassedExam,
                HasCertificate = hasCertificate,
                Experiments = experimentDetails
            };
        }

        public async Task<List<StudentProgressOverviewDto>> GetSchoolProgressAsync(int schoolId, int? classId = null, int? roboticsLevelId = null)
        {
            var query = _context.Students
                .Where(s => s.SchoolId == schoolId && s.IsActive);

            if (classId.HasValue)
                query = query.Where(s => s.ClassId == classId.Value);

            var students = await query.ToListAsync();
            var progressList = new List<StudentProgressOverviewDto>();

            foreach (var student in students)
            {
                var progress = await GetStudentProgressAsync(student.Id);
                if (progress != null)
                {
                    if (!roboticsLevelId.HasValue || progress.LevelNumber == roboticsLevelId.Value)
                    {
                        progressList.Add(progress);
                    }
                }
            }

            return progressList.OrderBy(p => p.ClassName).ThenBy(p => p.StudentRollNo).ToList();
        }

        public async Task<ProgressStatisticsDto> GetProgressStatisticsAsync(int? schoolId = null)
        {
            var query = _context.Students.Where(s => s.IsActive);

            if (schoolId.HasValue)
                query = query.Where(s => s.SchoolId == schoolId.Value);

            var students = await query.ToListAsync();
            var totalStudents = students.Count;

            var studentsWithProgress = await _context.StudentProgress
                .Where(p => students.Select(s => s.Id).Contains(p.StudentId))
                .Select(p => p.StudentId)
                .Distinct()
                .CountAsync();

            var certificatesIssued = await _context.Certificates
                .Where(c => students.Select(s => s.Id).Contains(c.StudentId))
                .CountAsync();

            var school = schoolId.HasValue
                ? await _context.Schools.FindAsync(schoolId.Value)
                : null;

            var levelStats = await GetLevelWiseStatisticsAsync(schoolId);

            return new ProgressStatisticsDto
            {
                SchoolId = schoolId ?? 0,
                SchoolName = school?.SchoolName ?? "All Schools",
                TotalStudents = totalStudents,
                ActiveStudents = totalStudents,
                StudentsWithProgress = studentsWithProgress,
                StudentsCompletedLevel = certificatesIssued,
                CertificatesIssued = certificatesIssued,
                AverageCompletionRate = levelStats.Count > 0 ? Math.Round(levelStats.Average(l => l.CompletionRate), 2) : 0,
                LevelStats = levelStats
            };
        }

        public async Task<List<LevelProgressStatsDto>> GetLevelWiseStatisticsAsync(int? schoolId = null)
        {
            var levels = await _context.RoboticsLevels
                .Where(l => l.IsActive)
                .OrderBy(l => l.LevelNumber)
                .ToListAsync();

            var stats = new List<LevelProgressStatsDto>();

            foreach (var level in levels)
            {
                // Get mappings for this level
                var mappingQuery = _context.SchoolLevelMappings
                    .Where(m => m.RoboticsLevelId == level.Id);

                if (schoolId.HasValue)
                    mappingQuery = mappingQuery.Where(m => m.SchoolId == schoolId.Value);

                var mappings = await mappingQuery.ToListAsync();

                // Get students in these mappings
                var studentCount = 0;
                foreach (var mapping in mappings)
                {
                    studentCount += await _context.Students
                        .CountAsync(s => s.SchoolId == mapping.SchoolId &&
                                        s.ClassId == mapping.ClassId &&
                                        s.CurrentAcademicYearId == mapping.AcademicYearId &&
                                        s.IsActive);
                }

                // Get certificates for this level
                var certificateQuery = _context.Certificates
                    .Where(c => c.RoboticsLevelId == level.Id);

                if (schoolId.HasValue)
                    certificateQuery = certificateQuery.Where(c => c.SchoolId == schoolId.Value);

                var certificateCount = await certificateQuery.CountAsync();

                stats.Add(new LevelProgressStatsDto
                {
                    RoboticsLevelId = level.Id,
                    LevelName = level.Name,
                    LevelNumber = level.LevelNumber,
                    StudentsEnrolled = studentCount,
                    StudentsCompleted = certificateCount,
                    CertificatesIssued = certificateCount,
                    CompletionRate = studentCount > 0 ? Math.Round((decimal)certificateCount / studentCount * 100, 2) : 0
                });
            }

            return stats;
        }

        public async Task<List<StudentProgressOverviewDto>> GetTeacherStudentsProgressAsync(int teacherId, int? classId = null)
        {
            // Get classes assigned to teacher
            var teacherClassIds = await _context.TeacherClasses
                .Where(tc => tc.TeacherId == teacherId)
                .Select(tc => tc.ClassId)
                .ToListAsync();

            if (classId.HasValue && !teacherClassIds.Contains(classId.Value))
                return new List<StudentProgressOverviewDto>();

            var classesToQuery = classId.HasValue
                ? new List<int> { classId.Value }
                : teacherClassIds;

            var students = await _context.Students
                .Where(s => classesToQuery.Contains(s.ClassId) && s.IsActive)
                .ToListAsync();

            var progressList = new List<StudentProgressOverviewDto>();

            foreach (var student in students)
            {
                var progress = await GetStudentProgressAsync(student.Id);
                if (progress != null)
                    progressList.Add(progress);
            }

            return progressList.OrderBy(p => p.ClassName).ThenBy(p => p.StudentRollNo).ToList();
        }

        #endregion

        #region Helper Methods

        public async Task<int?> GetTeacherIdByUserIdAsync(string userId)
        {
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            return teacher?.Id;
        }

        private async Task<int?> GetStudentRoboticsLevelAsync(Student student)
        {
            if (!student.CurrentAcademicYearId.HasValue) return null;
            var mapping = await _context.SchoolLevelMappings
                .FirstOrDefaultAsync(m => m.SchoolId == student.SchoolId &&
                                        m.ClassId == student.ClassId &&
                                        m.AcademicYearId == student.CurrentAcademicYearId.Value);
            return mapping?.RoboticsLevelId;
        }

        private static CertificateDetailDto MapToCertificateDetailDto(Certificate c)
        {
            var totalMarks = c.PassingScore > 0 ? c.PassingScore * 100 / 40 : 100; // Assuming 40% pass
            return new CertificateDetailDto
            {
                Id = c.Id,
                CertificateNumber = c.CertificateNumber,
                StudentId = c.StudentId,
                StudentName = c.StudentName,
                StudentEmail = c.StudentEmail,
                StudentRollNo = c.Student?.RollNo ?? "",
                SchoolId = c.SchoolId,
                SchoolName = c.SchoolName,
                SchoolLogoUrl = c.SchoolLogoUrl,
                ClassName = c.Student?.Class?.ClassName ?? "",
                RoboticsLevelId = c.RoboticsLevelId,
                LevelName = c.LevelName,
                LevelNumber = c.LevelNumber,
                AcademicYearName = c.AcademicYear?.YearName ?? c.AcademicYearName,
                ExamScore = c.ExamScore,
                PassingScore = c.PassingScore,
                Percentage = totalMarks > 0 ? Math.Round(c.ExamScore / totalMarks * 100, 2) : 0,
                Title = c.Title,
                CertificateFileUrl = c.CertificateFileUrl,
                IssuedDate = c.IssuedDate
            };
        }

        #endregion
    }
}
