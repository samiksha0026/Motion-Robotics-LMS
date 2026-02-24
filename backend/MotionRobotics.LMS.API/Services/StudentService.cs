using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.DTOs.Student;
using MotionRobotics.LMS.API.Models;

namespace MotionRobotics.LMS.API.Services
{
    public interface IStudentService
    {
        Task<StudentDashboardDto?> GetDashboardAsync(int studentId);
        Task<StudentProfileDto?> GetProfileAsync(int studentId);
        Task<StudentExperimentsListDto?> GetExperimentsAsync(int studentId);
        Task<StudentExperimentDto?> GetExperimentDetailAsync(int studentId, int experimentId);
        Task<(bool Success, string Message, ExperimentSubmissionResponseDto? Data)> SubmitExperimentAsync(int studentId, int experimentId, ExperimentSubmissionDto submission);
        Task<List<StudentCertificateDto>> GetCertificatesAsync(int studentId);
        Task<StudentCertificateDto?> GetCertificateAsync(int studentId, int certificateId);
        Task<int?> GetStudentIdByUserIdAsync(string userId);
    }

    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _context;

        public StudentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int?> GetStudentIdByUserIdAsync(string userId)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            return student?.Id;
        }

        /// <summary>
        /// Gets the robotics level for a student through SchoolLevelMapping
        /// </summary>
        private async Task<(RoboticsLevel? Level, AcademicYear? Year)> GetStudentRoboticsLevelAsync(Models.Student student)
        {
            // Get current academic year
            var currentYear = await _context.AcademicYears
                .FirstOrDefaultAsync(y => y.IsCurrent);

            if (currentYear == null) return (null, null);

            var mapping = await _context.SchoolLevelMappings
                .Include(m => m.RoboticsLevel)
                    .ThenInclude(rl => rl!.Experiments)
                .FirstOrDefaultAsync(m => m.ClassId == student.ClassId &&
                                          m.SchoolId == student.SchoolId &&
                                          m.AcademicYearId == currentYear.Id);

            return (mapping?.RoboticsLevel, currentYear);
        }

        public async Task<StudentDashboardDto?> GetDashboardAsync(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.School)
                .Include(s => s.Class)
                .Include(s => s.StudentProgresses)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) return null;

            var (level, academicYear) = await GetStudentRoboticsLevelAsync(student);
            var experiments = level?.Experiments?.OrderBy(e => e.SequenceOrder).ToList() ?? new List<Experiment>();
            var progressList = student.StudentProgresses.ToList();

            // Calculate progress
            var completedCount = progressList.Count(sp => sp.Completed);
            var approvedCount = progressList.Count(sp => sp.IsApprovedByTeacher);
            var pendingCount = progressList.Count(sp => sp.Completed && !sp.IsApprovedByTeacher);

            // Find next available experiment
            ExperimentPreviewDto? nextExp = null;
            foreach (var exp in experiments)
            {
                var progress = progressList.FirstOrDefault(sp => sp.ExperimentId == exp.Id);
                if (progress == null || (!progress.Completed && !progress.IsApprovedByTeacher))
                {
                    // Check if previous is approved (or first experiment)
                    var prevExp = experiments.FirstOrDefault(e => e.SequenceOrder == exp.SequenceOrder - 1);
                    var prevProgress = prevExp != null ? progressList.FirstOrDefault(sp => sp.ExperimentId == prevExp.Id) : null;

                    if (exp.SequenceOrder == 1 || prevProgress?.IsApprovedByTeacher == true)
                    {
                        nextExp = new ExperimentPreviewDto
                        {
                            ExperimentId = exp.Id,
                            SequenceOrder = exp.SequenceOrder,
                            Title = exp.Title,
                            EstimatedMinutes = exp.EstimatedMinutes
                        };
                        break;
                    }
                }
            }

            // Get certificates
            var certificates = await _context.Certificates
                .Include(c => c.RoboticsLevel)
                .Include(c => c.AcademicYear)
                .Where(c => c.StudentId == studentId)
                .OrderByDescending(c => c.IssuedDate)
                .Take(5)
                .Select(c => new CertificateSummaryDto
                {
                    CertificateId = c.Id,
                    LevelName = c.RoboticsLevel != null ? c.RoboticsLevel.Name : "N/A",
                    AcademicYear = c.AcademicYear != null ? c.AcademicYear.YearName : "N/A",
                    IssuedDate = c.IssuedDate
                })
                .ToListAsync();

            return new StudentDashboardDto
            {
                StudentId = student.Id,
                FullName = student.FullName,
                Email = student.Email,
                RollNo = student.RollNo,
                SchoolName = student.School?.SchoolName ?? "N/A",
                ClassName = student.Class?.ClassName ?? "N/A",
                CurrentAcademicYear = academicYear?.YearName ?? "N/A",
                CurrentLevel = level != null ? new CurrentLevelDto
                {
                    LevelId = level.Id,
                    LevelNumber = level.LevelNumber,
                    LevelName = level.Name,
                    Description = level.Description,
                    TotalExperiments = experiments.Count
                } : null,
                Progress = new ProgressSummaryDto
                {
                    TotalExperiments = experiments.Count,
                    CompletedExperiments = completedCount,
                    ApprovedExperiments = approvedCount,
                    PendingApproval = pendingCount,
                    ProgressPercentage = experiments.Count > 0
                        ? Math.Round((decimal)approvedCount / experiments.Count * 100, 1)
                        : 0,
                    LevelCompleted = experiments.Count > 0 && approvedCount == experiments.Count
                },
                NextExperiment = nextExp,
                Certificates = certificates
            };
        }

        public async Task<StudentProfileDto?> GetProfileAsync(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.School)
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) return null;

            // Get level history from certificates
            var levelHistory = await _context.Certificates
                .Include(c => c.RoboticsLevel)
                .Include(c => c.AcademicYear)
                .Where(c => c.StudentId == studentId)
                .OrderBy(c => c.RoboticsLevel!.LevelNumber)
                .Select(c => new LevelProgressHistoryDto
                {
                    LevelNumber = c.RoboticsLevel != null ? c.RoboticsLevel.LevelNumber : 0,
                    LevelName = c.RoboticsLevel != null ? c.RoboticsLevel.Name : "N/A",
                    AcademicYear = c.AcademicYear != null ? c.AcademicYear.YearName : "N/A",
                    ExperimentsCompleted = c.RoboticsLevel != null ? c.RoboticsLevel.Experiments.Count : 0,
                    TotalExperiments = c.RoboticsLevel != null ? c.RoboticsLevel.Experiments.Count : 0,
                    HasCertificate = true,
                    CertificateDate = c.IssuedDate
                })
                .ToListAsync();

            return new StudentProfileDto
            {
                StudentId = student.Id,
                FullName = student.FullName,
                Email = student.Email,
                RollNo = student.RollNo,
                ParentName = student.ParentName,
                ParentPhone = student.ParentPhone,
                SchoolName = student.School?.SchoolName ?? "N/A",
                ClassName = student.Class?.ClassName ?? "N/A",
                IsActive = student.IsActive,
                JoinedAt = student.CreatedAt,
                LevelHistory = levelHistory
            };
        }

        public async Task<StudentExperimentsListDto?> GetExperimentsAsync(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.StudentProgresses)
                    .ThenInclude(sp => sp.ApprovedByTeacher)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) return null;

            var (level, _) = await GetStudentRoboticsLevelAsync(student);
            if (level == null) return new StudentExperimentsListDto
            {
                Level = new CurrentLevelDto(),
                Progress = new ProgressSummaryDto(),
                Experiments = new List<StudentExperimentDto>()
            };

            // Get ALL experiments for this level (for total count)
            var allExperiments = level.Experiments?.OrderBy(e => e.SequenceOrder).ToList() ?? new List<Experiment>();

            // Get unlocked experiments for this class
            var unlockedExperimentIds = await _context.ClassExperimentUnlocks
                .Where(u => u.ClassId == student.ClassId)
                .Select(u => u.ExperimentId)
                .ToListAsync();

            // Filter to only show unlocked experiments
            var experiments = allExperiments
                .Where(e => unlockedExperimentIds.Contains(e.Id))
                .OrderBy(e => e.SequenceOrder)
                .ToList();

            var progressList = student.StudentProgresses.ToList();

            var experimentDtos = new List<StudentExperimentDto>();

            foreach (var exp in experiments)
            {
                var progress = progressList.FirstOrDefault(sp => sp.ExperimentId == exp.Id);
                var prevExp = experiments.FirstOrDefault(e => e.SequenceOrder == exp.SequenceOrder - 1);
                var prevProgress = prevExp != null ? progressList.FirstOrDefault(sp => sp.ExperimentId == prevExp.Id) : null;

                string status;
                if (progress?.IsApprovedByTeacher == true)
                    status = "approved";
                else if (progress?.Completed == true)
                    status = "completed";
                else if (exp.SequenceOrder == 1 || prevProgress?.IsApprovedByTeacher == true)
                    status = "available";
                else
                    status = "locked";

                experimentDtos.Add(new StudentExperimentDto
                {
                    ExperimentId = exp.Id,
                    SequenceOrder = exp.SequenceOrder,
                    Title = exp.Title,
                    Description = exp.Description,
                    Objective = exp.Objective,
                    Components = exp.Components,
                    Procedure = exp.Procedure,
                    WiringDiagram = exp.WiringDiagram,
                    CircuitDiagram = exp.CircuitDiagram,
                    CodeSnippet = exp.CodeSnippet,
                    DemoVideoUrl = exp.DemoVideoUrl,
                    SafetyNotes = exp.SafetyNotes,
                    EstimatedMinutes = exp.EstimatedMinutes,
                    Status = status,
                    Progress = progress != null ? new StudentExperimentProgressDto
                    {
                        ProgressId = progress.Id,
                        Completed = progress.Completed,
                        CompletedAt = progress.CompletedAt,
                        SubmissionNotes = progress.SubmissionNotes,
                        SubmissionImageUrl = progress.SubmissionImageUrl,
                        IsApproved = progress.IsApprovedByTeacher,
                        ApprovedAt = progress.ApprovedAt,
                        TeacherRemarks = progress.TeacherRemarks,
                        ApprovedByTeacher = progress.ApprovedByTeacher?.FullName
                    } : null
                });
            }

            var completedCount = progressList.Count(sp => sp.Completed);
            var approvedCount = progressList.Count(sp => sp.IsApprovedByTeacher);

            return new StudentExperimentsListDto
            {
                Level = new CurrentLevelDto
                {
                    LevelId = level.Id,
                    LevelNumber = level.LevelNumber,
                    LevelName = level.Name,
                    Description = level.Description,
                    TotalExperiments = allExperiments.Count,
                    SyllabusUrl = level.SyllabusUrl
                },
                Progress = new ProgressSummaryDto
                {
                    TotalExperiments = allExperiments.Count,
                    CompletedExperiments = completedCount,
                    ApprovedExperiments = approvedCount,
                    PendingApproval = progressList.Count(sp => sp.Completed && !sp.IsApprovedByTeacher),
                    ProgressPercentage = allExperiments.Count > 0
                        ? Math.Round((decimal)approvedCount / allExperiments.Count * 100, 1)
                        : 0,
                    LevelCompleted = allExperiments.Count > 0 && approvedCount == allExperiments.Count
                },
                UnlockedCount = experiments.Count,
                Experiments = experimentDtos
            };
        }

        public async Task<StudentExperimentDto?> GetExperimentDetailAsync(int studentId, int experimentId)
        {
            var student = await _context.Students
                .Include(s => s.StudentProgresses)
                    .ThenInclude(sp => sp.ApprovedByTeacher)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) return null;

            var (level, _) = await GetStudentRoboticsLevelAsync(student);
            if (level == null) return null;

            var experiment = level.Experiments?.FirstOrDefault(e => e.Id == experimentId);
            if (experiment == null) return null;

            var experiments = level.Experiments?.OrderBy(e => e.SequenceOrder).ToList() ?? new List<Experiment>();
            var progressList = student.StudentProgresses.ToList();

            var progress = progressList.FirstOrDefault(sp => sp.ExperimentId == experimentId);
            var prevExp = experiments.FirstOrDefault(e => e.SequenceOrder == experiment.SequenceOrder - 1);
            var prevProgress = prevExp != null ? progressList.FirstOrDefault(sp => sp.ExperimentId == prevExp.Id) : null;

            string status;
            if (progress?.IsApprovedByTeacher == true)
                status = "approved";
            else if (progress?.Completed == true)
                status = "completed";
            else if (experiment.SequenceOrder == 1 || prevProgress?.IsApprovedByTeacher == true)
                status = "available";
            else
                status = "locked";

            return new StudentExperimentDto
            {
                ExperimentId = experiment.Id,
                SequenceOrder = experiment.SequenceOrder,
                Title = experiment.Title,
                Description = experiment.Description,
                Objective = experiment.Objective,
                Components = experiment.Components,
                Procedure = experiment.Procedure,
                WiringDiagram = experiment.WiringDiagram,
                CircuitDiagram = experiment.CircuitDiagram,
                CodeSnippet = experiment.CodeSnippet,
                DemoVideoUrl = experiment.DemoVideoUrl,
                SafetyNotes = experiment.SafetyNotes,
                EstimatedMinutes = experiment.EstimatedMinutes,
                Status = status,
                Progress = progress != null ? new StudentExperimentProgressDto
                {
                    ProgressId = progress.Id,
                    Completed = progress.Completed,
                    CompletedAt = progress.CompletedAt,
                    SubmissionNotes = progress.SubmissionNotes,
                    SubmissionImageUrl = progress.SubmissionImageUrl,
                    IsApproved = progress.IsApprovedByTeacher,
                    ApprovedAt = progress.ApprovedAt,
                    TeacherRemarks = progress.TeacherRemarks,
                    ApprovedByTeacher = progress.ApprovedByTeacher?.FullName
                } : null
            };
        }

        public async Task<(bool Success, string Message, ExperimentSubmissionResponseDto? Data)> SubmitExperimentAsync(
            int studentId, int experimentId, ExperimentSubmissionDto submission)
        {
            var student = await _context.Students
                .Include(s => s.StudentProgresses)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
                return (false, "Student not found", null);

            var (level, academicYear) = await GetStudentRoboticsLevelAsync(student);
            if (level == null)
                return (false, "No robotics level assigned for current academic year", null);

            var experiment = level.Experiments?.FirstOrDefault(e => e.Id == experimentId);
            if (experiment == null)
                return (false, "Experiment not found in your current level", null);

            var experiments = level.Experiments?.OrderBy(e => e.SequenceOrder).ToList() ?? new List<Experiment>();
            var progressList = student.StudentProgresses.ToList();

            // Check if already completed
            var existingProgress = progressList.FirstOrDefault(sp => sp.ExperimentId == experimentId);
            if (existingProgress?.Completed == true)
                return (false, "This experiment has already been submitted", null);

            // Check if previous experiment is approved (sequential unlock)
            if (experiment.SequenceOrder > 1)
            {
                var prevExp = experiments.FirstOrDefault(e => e.SequenceOrder == experiment.SequenceOrder - 1);
                var prevProgress = prevExp != null ? progressList.FirstOrDefault(sp => sp.ExperimentId == prevExp.Id) : null;

                if (prevProgress?.IsApprovedByTeacher != true)
                    return (false, "You must complete and get approval for the previous experiment first", null);
            }

            // Create or update progress
            StudentProgress progress;
            if (existingProgress != null)
            {
                progress = existingProgress;
            }
            else
            {
                progress = new StudentProgress
                {
                    StudentId = studentId,
                    ExperimentId = experimentId,
                    AcademicYearId = academicYear?.Id ?? 0
                };
                _context.StudentProgress.Add(progress);
            }

            progress.Completed = true;
            progress.CompletedAt = DateTime.UtcNow;
            progress.SubmissionNotes = submission.SubmissionNotes;
            progress.SubmissionImageUrl = submission.SubmissionImageUrl;

            await _context.SaveChangesAsync();

            // Find next experiment
            ExperimentPreviewDto? nextExp = null;
            var nextExperiment = experiments.FirstOrDefault(e => e.SequenceOrder == experiment.SequenceOrder + 1);
            if (nextExperiment != null)
            {
                nextExp = new ExperimentPreviewDto
                {
                    ExperimentId = nextExperiment.Id,
                    SequenceOrder = nextExperiment.SequenceOrder,
                    Title = nextExperiment.Title,
                    EstimatedMinutes = nextExperiment.EstimatedMinutes
                };
            }

            return (true, "Experiment submitted successfully! Waiting for teacher approval.", new ExperimentSubmissionResponseDto
            {
                ProgressId = progress.Id,
                ExperimentId = experimentId,
                ExperimentTitle = experiment.Title,
                CompletedAt = progress.CompletedAt ?? DateTime.UtcNow,
                Message = "Your submission is pending teacher approval. Once approved, the next experiment will unlock.",
                NextExperiment = nextExp
            });
        }

        public async Task<List<StudentCertificateDto>> GetCertificatesAsync(int studentId)
        {
            return await _context.Certificates
                .Include(c => c.Student)
                    .ThenInclude(s => s!.School)
                .Include(c => c.RoboticsLevel)
                .Include(c => c.AcademicYear)
                .Where(c => c.StudentId == studentId)
                .OrderByDescending(c => c.IssuedDate)
                .Select(c => new StudentCertificateDto
                {
                    CertificateId = c.Id,
                    CertificateNumber = c.CertificateNumber,
                    StudentName = c.Student != null ? c.Student.FullName : "N/A",
                    SchoolName = c.Student != null && c.Student.School != null ? c.Student.School.SchoolName : "N/A",
                    LevelNumber = c.RoboticsLevel != null ? c.RoboticsLevel.LevelNumber : 0,
                    LevelName = c.RoboticsLevel != null ? c.RoboticsLevel.Name : "N/A",
                    AcademicYear = c.AcademicYear != null ? c.AcademicYear.YearName : "N/A",
                    IssuedDate = c.IssuedDate,
                    CertificateFileUrl = c.CertificateFileUrl,
                    ExperimentsCompleted = c.RoboticsLevel != null ? c.RoboticsLevel.Experiments.Count : 0
                })
                .ToListAsync();
        }

        public async Task<StudentCertificateDto?> GetCertificateAsync(int studentId, int certificateId)
        {
            return await _context.Certificates
                .Include(c => c.Student)
                    .ThenInclude(s => s!.School)
                .Include(c => c.RoboticsLevel)
                .Include(c => c.AcademicYear)
                .Where(c => c.StudentId == studentId && c.Id == certificateId)
                .Select(c => new StudentCertificateDto
                {
                    CertificateId = c.Id,
                    CertificateNumber = c.CertificateNumber,
                    StudentName = c.Student != null ? c.Student.FullName : "N/A",
                    SchoolName = c.Student != null && c.Student.School != null ? c.Student.School.SchoolName : "N/A",
                    LevelNumber = c.RoboticsLevel != null ? c.RoboticsLevel.LevelNumber : 0,
                    LevelName = c.RoboticsLevel != null ? c.RoboticsLevel.Name : "N/A",
                    AcademicYear = c.AcademicYear != null ? c.AcademicYear.YearName : "N/A",
                    IssuedDate = c.IssuedDate,
                    CertificateFileUrl = c.CertificateFileUrl,
                    ExperimentsCompleted = c.RoboticsLevel != null ? c.RoboticsLevel.Experiments.Count : 0
                })
                .FirstOrDefaultAsync();
        }
    }
}
