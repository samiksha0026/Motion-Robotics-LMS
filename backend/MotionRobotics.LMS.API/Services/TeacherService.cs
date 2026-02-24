using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.DTOs.Teacher;
using MotionRobotics.LMS.API.Models;

namespace MotionRobotics.LMS.API.Services
{
    public interface ITeacherService
    {
        Task<TeacherDashboardDto?> GetDashboardAsync(int teacherId);
        Task<List<AssignedClassDto>> GetAssignedClassesAsync(int teacherId);
        Task<ClassDetailDto?> GetClassDetailAsync(int teacherId, int classId);
        Task<List<ClassStudentDto>> GetClassStudentsAsync(int teacherId, int classId);
        Task<StudentDetailedProgressDto?> GetStudentProgressAsync(int teacherId, int studentId);
        Task<List<PendingApprovalDto>> GetPendingApprovalsAsync(int teacherId, int? classId = null);
        Task<(bool Success, string Message)> ApproveProgressAsync(int teacherId, int progressId, ApprovalRequestDto request);
        Task<(bool Success, string Message, int Processed)> BulkApproveProgressAsync(int teacherId, BulkApprovalRequestDto request);
        Task<int?> GetTeacherIdByUserIdAsync(string userId);
        Task<bool> IsTeacherAssignedToClass(int teacherId, int classId);
        Task<bool> IsTeacherAssignedToStudent(int teacherId, int studentId);
    }

    public class TeacherService : ITeacherService
    {
        private readonly ApplicationDbContext _context;

        public TeacherService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int?> GetTeacherIdByUserIdAsync(string userId)
        {
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            return teacher?.Id;
        }

        public async Task<bool> IsTeacherAssignedToClass(int teacherId, int classId)
        {
            return await _context.TeacherClasses
                .AnyAsync(tc => tc.TeacherId == teacherId && tc.ClassId == classId);
        }

        public async Task<bool> IsTeacherAssignedToStudent(int teacherId, int studentId)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null) return false;

            return await _context.TeacherClasses
                .AnyAsync(tc => tc.TeacherId == teacherId && tc.ClassId == student.ClassId);
        }

        /// <summary>
        /// Gets the robotics level for a class through SchoolLevelMapping
        /// </summary>
        private async Task<(RoboticsLevel? Level, int? MappingId)> GetClassRoboticsLevelAsync(int classId, int schoolId)
        {
            // Get current academic year
            var currentYear = await _context.AcademicYears
                .FirstOrDefaultAsync(y => y.IsCurrent);

            if (currentYear == null) return (null, null);

            var mapping = await _context.SchoolLevelMappings
                .Include(m => m.RoboticsLevel)
                    .ThenInclude(rl => rl!.Experiments)
                .FirstOrDefaultAsync(m => m.ClassId == classId &&
                                          m.SchoolId == schoolId &&
                                          m.AcademicYearId == currentYear.Id);

            return (mapping?.RoboticsLevel, mapping?.Id);
        }

        public async Task<TeacherDashboardDto?> GetDashboardAsync(int teacherId)
        {
            var teacher = await _context.Teachers
                .Include(t => t.School)
                .FirstOrDefaultAsync(t => t.Id == teacherId);

            if (teacher == null) return null;

            var assignedClasses = await GetAssignedClassesAsync(teacherId);
            var pendingApprovals = await GetPendingApprovalsAsync(teacherId);

            // Get current academic year
            var currentYear = await _context.AcademicYears
                .Where(y => y.IsCurrent)
                .Select(y => y.YearName)
                .FirstOrDefaultAsync() ?? "N/A";

            return new TeacherDashboardDto
            {
                TeacherId = teacher.Id,
                FullName = teacher.FullName,
                Email = teacher.Email,
                SchoolName = teacher.School?.SchoolName ?? "N/A",
                TotalClasses = assignedClasses.Count,
                TotalStudents = assignedClasses.Sum(c => c.StudentCount),
                PendingApprovals = pendingApprovals.Count,
                CurrentAcademicYear = currentYear,
                AssignedClasses = assignedClasses,
                RecentPendingApprovals = pendingApprovals.Take(10).ToList()
            };
        }

        public async Task<List<AssignedClassDto>> GetAssignedClassesAsync(int teacherId)
        {
            var teacher = await _context.Teachers.FindAsync(teacherId);
            if (teacher == null) return new List<AssignedClassDto>();

            var classIds = await _context.TeacherClasses
                .Where(tc => tc.TeacherId == teacherId)
                .Select(tc => tc.ClassId)
                .ToListAsync();

            var classes = await _context.Classes
                .Where(c => classIds.Contains(c.Id))
                .Include(c => c.Students)
                    .ThenInclude(s => s.StudentProgresses)
                .ToListAsync();

            var result = new List<AssignedClassDto>();

            foreach (var c in classes)
            {
                var (level, _) = await GetClassRoboticsLevelAsync(c.Id, teacher.SchoolId);
                var totalExperiments = level?.Experiments?.Count ?? 0;

                result.Add(new AssignedClassDto
                {
                    ClassId = c.Id,
                    ClassName = c.ClassName,
                    StudentCount = c.Students.Count(s => s.IsActive),
                    RoboticsLevelId = level?.Id ?? 0,
                    LevelNumber = level?.LevelNumber ?? 0,
                    LevelName = level?.Name ?? "Not Assigned",
                    TotalExperiments = totalExperiments,
                    ExperimentsCompleted = c.Students
                        .SelectMany(s => s.StudentProgresses)
                        .Count(sp => sp.IsApprovedByTeacher),
                    PendingApprovals = c.Students
                        .SelectMany(s => s.StudentProgresses)
                        .Count(sp => sp.Completed && !sp.IsApprovedByTeacher)
                });
            }

            return result;
        }

        public async Task<ClassDetailDto?> GetClassDetailAsync(int teacherId, int classId)
        {
            if (!await IsTeacherAssignedToClass(teacherId, classId))
                return null;

            var teacher = await _context.Teachers.FindAsync(teacherId);
            if (teacher == null) return null;

            var classEntity = await _context.Classes
                .Include(c => c.Students)
                    .ThenInclude(s => s.StudentProgresses)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classEntity == null) return null;

            var (level, _) = await GetClassRoboticsLevelAsync(classId, teacher.SchoolId);
            var students = classEntity.Students.Where(s => s.IsActive).ToList();
            var experiments = level?.Experiments?.OrderBy(e => e.SequenceOrder).ToList() ?? new List<Experiment>();

            return new ClassDetailDto
            {
                ClassId = classEntity.Id,
                ClassName = classEntity.ClassName,
                RoboticsLevelId = level?.Id ?? 0,
                LevelNumber = level?.LevelNumber ?? 0,
                LevelName = level?.Name ?? "Not Assigned",
                LevelDescription = level?.Description ?? "",
                TotalExperiments = experiments.Count,
                Students = students.Select(s => new ClassStudentDto
                {
                    StudentId = s.Id,
                    FullName = s.FullName,
                    RollNumber = s.RollNo,
                    Email = s.Email,
                    IsActive = s.IsActive,
                    TotalExperiments = experiments.Count,
                    CompletedExperiments = s.StudentProgresses.Count(sp => sp.Completed),
                    ApprovedExperiments = s.StudentProgresses.Count(sp => sp.IsApprovedByTeacher),
                    PendingApproval = s.StudentProgresses.Count(sp => sp.Completed && !sp.IsApprovedByTeacher),
                    ProgressPercentage = experiments.Count > 0
                        ? Math.Round((decimal)s.StudentProgresses.Count(sp => sp.IsApprovedByTeacher) / experiments.Count * 100, 1)
                        : 0
                }).OrderBy(s => s.RollNumber).ToList(),
                Experiments = experiments.Select(e => new ExperimentProgressDto
                {
                    ExperimentId = e.Id,
                    SequenceOrder = e.SequenceOrder,
                    Title = e.Title,
                    EstimatedMinutes = e.EstimatedMinutes,
                    TotalStudents = students.Count,
                    CompletedCount = students.SelectMany(s => s.StudentProgresses)
                        .Count(sp => sp.ExperimentId == e.Id && sp.Completed),
                    ApprovedCount = students.SelectMany(s => s.StudentProgresses)
                        .Count(sp => sp.ExperimentId == e.Id && sp.IsApprovedByTeacher),
                    PendingCount = students.SelectMany(s => s.StudentProgresses)
                        .Count(sp => sp.ExperimentId == e.Id && sp.Completed && !sp.IsApprovedByTeacher)
                }).ToList()
            };
        }

        public async Task<List<ClassStudentDto>> GetClassStudentsAsync(int teacherId, int classId)
        {
            if (!await IsTeacherAssignedToClass(teacherId, classId))
                return new List<ClassStudentDto>();

            var teacher = await _context.Teachers.FindAsync(teacherId);
            if (teacher == null) return new List<ClassStudentDto>();

            var classEntity = await _context.Classes
                .Include(c => c.Students)
                    .ThenInclude(s => s.StudentProgresses)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classEntity == null) return new List<ClassStudentDto>();

            var (level, _) = await GetClassRoboticsLevelAsync(classId, teacher.SchoolId);
            var totalExperiments = level?.Experiments?.Count ?? 0;

            return classEntity.Students
                .Where(s => s.IsActive)
                .Select(s => new ClassStudentDto
                {
                    StudentId = s.Id,
                    FullName = s.FullName,
                    RollNumber = s.RollNo,
                    Email = s.Email,
                    IsActive = s.IsActive,
                    TotalExperiments = totalExperiments,
                    CompletedExperiments = s.StudentProgresses.Count(sp => sp.Completed),
                    ApprovedExperiments = s.StudentProgresses.Count(sp => sp.IsApprovedByTeacher),
                    PendingApproval = s.StudentProgresses.Count(sp => sp.Completed && !sp.IsApprovedByTeacher),
                    ProgressPercentage = totalExperiments > 0
                        ? Math.Round((decimal)s.StudentProgresses.Count(sp => sp.IsApprovedByTeacher) / totalExperiments * 100, 1)
                        : 0
                })
                .OrderBy(s => s.RollNumber)
                .ToList();
        }

        public async Task<StudentDetailedProgressDto?> GetStudentProgressAsync(int teacherId, int studentId)
        {
            if (!await IsTeacherAssignedToStudent(teacherId, studentId))
                return null;

            var teacher = await _context.Teachers.FindAsync(teacherId);
            if (teacher == null) return null;

            var student = await _context.Students
                .Include(s => s.Class)
                .Include(s => s.StudentProgresses)
                    .ThenInclude(sp => sp.ApprovedByTeacher)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null || student.Class == null) return null;

            var (level, _) = await GetClassRoboticsLevelAsync(student.ClassId, teacher.SchoolId);
            var experiments = level?.Experiments?
                .OrderBy(e => e.SequenceOrder).ToList() ?? new List<Experiment>();

            var progressList = student.StudentProgresses.ToList();
            var approvedCount = progressList.Count(sp => sp.IsApprovedByTeacher);

            // Determine status for each experiment
            var experimentStatuses = new List<ExperimentStatusDto>();
            foreach (var exp in experiments)
            {
                var progress = progressList.FirstOrDefault(sp => sp.ExperimentId == exp.Id);
                var prevExperiment = experiments.FirstOrDefault(e => e.SequenceOrder == exp.SequenceOrder - 1);
                var prevProgress = prevExperiment != null
                    ? progressList.FirstOrDefault(sp => sp.ExperimentId == prevExperiment.Id)
                    : null;

                string status;
                if (progress?.IsApprovedByTeacher == true)
                    status = "approved";
                else if (progress?.Completed == true)
                    status = "completed";
                else if (exp.SequenceOrder == 1 || prevProgress?.IsApprovedByTeacher == true)
                    status = "available";
                else
                    status = "locked";

                experimentStatuses.Add(new ExperimentStatusDto
                {
                    ExperimentId = exp.Id,
                    SequenceOrder = exp.SequenceOrder,
                    Title = exp.Title,
                    Description = exp.Description,
                    EstimatedMinutes = exp.EstimatedMinutes,
                    Status = status,
                    CompletedAt = progress?.CompletedAt,
                    ApprovedAt = progress?.ApprovedAt,
                    SubmissionNotes = progress?.SubmissionNotes,
                    SubmissionImageUrl = progress?.SubmissionImageUrl,
                    TeacherRemarks = progress?.TeacherRemarks
                });
            }

            return new StudentDetailedProgressDto
            {
                StudentId = student.Id,
                StudentName = student.FullName,
                RollNumber = student.RollNo,
                Email = student.Email,
                ClassId = student.ClassId,
                ClassName = student.Class.ClassName,
                RoboticsLevelId = level?.Id ?? 0,
                LevelName = level?.Name ?? "Not Assigned",
                LevelNumber = level?.LevelNumber ?? 0,
                TotalExperiments = experiments.Count,
                CompletedExperiments = progressList.Count(sp => sp.Completed),
                ApprovedExperiments = approvedCount,
                ProgressPercentage = experiments.Count > 0
                    ? Math.Round((decimal)approvedCount / experiments.Count * 100, 1)
                    : 0,
                Experiments = experimentStatuses
            };
        }

        public async Task<List<PendingApprovalDto>> GetPendingApprovalsAsync(int teacherId, int? classId = null)
        {
            var classIds = await _context.TeacherClasses
                .Where(tc => tc.TeacherId == teacherId)
                .Select(tc => tc.ClassId)
                .ToListAsync();

            if (classId.HasValue && !classIds.Contains(classId.Value))
                return new List<PendingApprovalDto>();

            var query = _context.StudentProgress
                .Include(sp => sp.Student)
                    .ThenInclude(s => s.Class)
                .Include(sp => sp.Experiment)
                .Where(sp => sp.Completed && !sp.IsApprovedByTeacher && classIds.Contains(sp.Student.ClassId));

            if (classId.HasValue)
                query = query.Where(sp => sp.Student.ClassId == classId.Value);

            var pendingProgress = await query
                .OrderByDescending(sp => sp.CompletedAt)
                .ToListAsync();

            return pendingProgress.Select(sp => new PendingApprovalDto
            {
                ProgressId = sp.Id,
                StudentId = sp.StudentId,
                StudentName = sp.Student.FullName,
                RollNumber = sp.Student.RollNo,
                ClassId = sp.Student.ClassId,
                ClassName = sp.Student.Class?.ClassName ?? "N/A",
                ExperimentId = sp.ExperimentId,
                ExperimentSequence = sp.Experiment?.SequenceOrder ?? 0,
                ExperimentTitle = sp.Experiment?.Title ?? "N/A",
                CompletedAt = sp.CompletedAt ?? DateTime.UtcNow,
                SubmissionNotes = sp.SubmissionNotes,
                SubmissionImageUrl = sp.SubmissionImageUrl
            }).ToList();
        }

        public async Task<(bool Success, string Message)> ApproveProgressAsync(int teacherId, int progressId, ApprovalRequestDto request)
        {
            var progress = await _context.StudentProgress
                .Include(sp => sp.Student)
                .FirstOrDefaultAsync(sp => sp.Id == progressId);

            if (progress == null)
                return (false, "Progress entry not found");

            if (!await IsTeacherAssignedToStudent(teacherId, progress.StudentId))
                return (false, "Not authorized to approve this student's progress");

            if (!progress.Completed)
                return (false, "Student has not completed this experiment yet");

            if (progress.IsApprovedByTeacher)
                return (false, "This progress has already been approved");

            progress.IsApprovedByTeacher = request.Approve;
            progress.ApprovedByTeacherId = teacherId;
            progress.ApprovedAt = DateTime.UtcNow;
            progress.TeacherRemarks = request.TeacherRemarks;

            await _context.SaveChangesAsync();

            return (true, request.Approve ? "Progress approved successfully" : "Progress rejected");
        }

        public async Task<(bool Success, string Message, int Processed)> BulkApproveProgressAsync(int teacherId, BulkApprovalRequestDto request)
        {
            if (!request.ProgressIds.Any())
                return (false, "No progress IDs provided", 0);

            var progressList = await _context.StudentProgress
                .Include(sp => sp.Student)
                .Where(sp => request.ProgressIds.Contains(sp.Id))
                .ToListAsync();

            int processed = 0;
            foreach (var progress in progressList)
            {
                if (!await IsTeacherAssignedToStudent(teacherId, progress.StudentId))
                    continue;

                if (!progress.Completed || progress.IsApprovedByTeacher)
                    continue;

                progress.IsApprovedByTeacher = request.Approve;
                progress.ApprovedByTeacherId = teacherId;
                progress.ApprovedAt = DateTime.UtcNow;
                progress.TeacherRemarks = request.TeacherRemarks;
                processed++;
            }

            await _context.SaveChangesAsync();

            return (true, $"Processed {processed} out of {request.ProgressIds.Count} items", processed);
        }
    }
}
