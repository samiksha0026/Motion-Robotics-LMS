using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.DTOs.Student;
using MotionRobotics.LMS.API.DTOs.Teacher;
using MotionRobotics.LMS.API.Helpers;
using MotionRobotics.LMS.API.Models;

namespace MotionRobotics.LMS.API.Services
{
    public interface IExamService
    {
        // Admin operations
        Task<ExamDetailDto> CreateExamAsync(ExamCreateDto dto);
        Task<ExamDetailDto?> GetExamByIdAsync(int examId);
        Task<ExamListDto> GetAllExamsAsync(int? roboticsLevelId = null, bool? isActive = null);
        Task<ExamDetailDto?> UpdateExamAsync(int examId, ExamUpdateDto dto);
        Task<bool> DeleteExamAsync(int examId);
        Task<ExamResultsListDto> GetExamResultsAsync(int examId, int? schoolId = null, int? classId = null);
        Task<ExamStatisticsDto?> GetExamStatisticsAsync(int examId);

        // Teacher operations
        Task<TeacherExamResultsListDto> GetTeacherExamResultsAsync(int teacherId, int? classId = null, int? examId = null);
        Task<TeacherExamOverviewDto?> GetTeacherExamOverviewAsync(int teacherId, int examId);
        Task<List<StudentExamEligibilityDto>> GetStudentsExamEligibilityAsync(int teacherId, int classId, int roboticsLevelId);

        // Student operations
        Task<ExamEligibilityDto> CheckExamEligibilityAsync(int studentId);
        Task<ExamQuestionsDto?> StartExamAsync(int studentId);
        Task<StudentExamResultDto> SubmitExamAsync(int studentId, ExamAnswerSubmitDto dto);
        Task<ExamHistoryDto> GetExamHistoryAsync(int studentId);
        Task<StudentExamResultDto?> GetExamResultAsync(int studentId, int resultId);

        // Helper
        Task<int?> GetStudentIdByUserIdAsync(string userId);
        Task<int?> GetTeacherIdByUserIdAsync(string userId);
    }

    public class ExamService : IExamService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ExamService> _logger;

        public ExamService(ApplicationDbContext context, ILogger<ExamService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Admin Operations

        public async Task<ExamDetailDto> CreateExamAsync(ExamCreateDto dto)
        {
            var roboticsLevel = await _context.RoboticsLevels.FindAsync(dto.RoboticsLevelId)
                ?? throw new ArgumentException("Invalid robotics level");

            // Check if exam already exists for this level
            var existingExam = await _context.Exams
                .FirstOrDefaultAsync(e => e.RoboticsLevelId == dto.RoboticsLevelId && e.IsActive);
            if (existingExam != null)
            {
                throw new InvalidOperationException($"An active exam already exists for level {roboticsLevel.Name}");
            }

            var questionsJson = ExamEvaluator.SerializeQuestions(dto.Questions);
            var totalMarks = dto.Questions.Sum(q => q.Marks);

            var exam = new Exam
            {
                RoboticsLevelId = dto.RoboticsLevelId,
                Title = dto.Title,
                Description = dto.Description,
                DurationMinutes = dto.DurationMinutes,
                TotalQuestions = dto.Questions.Count,
                TotalMarks = totalMarks,
                PassingPercentage = dto.PassingPercentage,
                QuestionsJson = questionsJson,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            return await GetExamByIdAsync(exam.Id) ?? throw new Exception("Failed to create exam");
        }

        public async Task<ExamDetailDto?> GetExamByIdAsync(int examId)
        {
            var exam = await _context.Exams
                .Include(e => e.RoboticsLevel)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null) return null;

            var questions = ExamEvaluator.ParseQuestionsJson(exam.QuestionsJson);

            return new ExamDetailDto
            {
                Id = exam.Id,
                RoboticsLevelId = exam.RoboticsLevelId,
                RoboticsLevelName = exam.RoboticsLevel?.Name ?? "",
                LevelNumber = exam.RoboticsLevel?.LevelNumber ?? 0,
                Title = exam.Title,
                Description = exam.Description,
                DurationMinutes = exam.DurationMinutes,
                TotalQuestions = exam.TotalQuestions,
                TotalMarks = exam.TotalMarks,
                PassingPercentage = exam.PassingPercentage,
                PassingMarks = Math.Ceiling(exam.TotalMarks * exam.PassingPercentage / 100),
                IsActive = exam.IsActive,
                CreatedAt = exam.CreatedAt,
                UpdatedAt = exam.UpdatedAt,
                Questions = questions
            };
        }

        public async Task<ExamListDto> GetAllExamsAsync(int? roboticsLevelId = null, bool? isActive = null)
        {
            var query = _context.Exams.Include(e => e.RoboticsLevel).AsQueryable();

            if (roboticsLevelId.HasValue)
                query = query.Where(e => e.RoboticsLevelId == roboticsLevelId.Value);

            if (isActive.HasValue)
                query = query.Where(e => e.IsActive == isActive.Value);

            var exams = await query
                .OrderBy(e => e.RoboticsLevel!.LevelNumber)
                .ToListAsync();

            return new ExamListDto
            {
                Exams = exams.Select(e => new ExamResponseDto
                {
                    Id = e.Id,
                    RoboticsLevelId = e.RoboticsLevelId,
                    RoboticsLevelName = e.RoboticsLevel?.Name ?? "",
                    LevelNumber = e.RoboticsLevel?.LevelNumber ?? 0,
                    Title = e.Title,
                    Description = e.Description,
                    DurationMinutes = e.DurationMinutes,
                    TotalQuestions = e.TotalQuestions,
                    TotalMarks = e.TotalMarks,
                    PassingPercentage = e.PassingPercentage,
                    PassingMarks = Math.Ceiling(e.TotalMarks * e.PassingPercentage / 100),
                    IsActive = e.IsActive,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                }).ToList(),
                TotalCount = exams.Count
            };
        }

        public async Task<ExamDetailDto?> UpdateExamAsync(int examId, ExamUpdateDto dto)
        {
            var exam = await _context.Exams.FindAsync(examId);
            if (exam == null) return null;

            if (dto.Title != null)
                exam.Title = dto.Title;

            if (dto.Description != null)
                exam.Description = dto.Description;

            if (dto.DurationMinutes.HasValue)
                exam.DurationMinutes = dto.DurationMinutes.Value;

            if (dto.PassingPercentage.HasValue)
                exam.PassingPercentage = dto.PassingPercentage.Value;

            if (dto.IsActive.HasValue)
                exam.IsActive = dto.IsActive.Value;

            if (dto.Questions != null && dto.Questions.Count > 0)
            {
                exam.QuestionsJson = ExamEvaluator.SerializeQuestions(dto.Questions);
                exam.TotalQuestions = dto.Questions.Count;
                exam.TotalMarks = dto.Questions.Sum(q => q.Marks);
            }

            exam.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetExamByIdAsync(examId);
        }

        public async Task<bool> DeleteExamAsync(int examId)
        {
            var exam = await _context.Exams.FindAsync(examId);
            if (exam == null) return false;

            // Check if exam has any results
            var hasResults = await _context.ExamResults.AnyAsync(r => r.ExamId == examId);
            if (hasResults)
            {
                // Soft delete - just deactivate
                exam.IsActive = false;
                exam.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.Exams.Remove(exam);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ExamResultsListDto> GetExamResultsAsync(int examId, int? schoolId = null, int? classId = null)
        {
            var query = _context.ExamResults
                .Include(r => r.Student)
                    .ThenInclude(s => s!.School)
                .Include(r => r.Student)
                    .ThenInclude(s => s!.Class)
                .Include(r => r.Exam)
                    .ThenInclude(e => e!.RoboticsLevel)
                .Where(r => r.ExamId == examId);

            if (schoolId.HasValue)
                query = query.Where(r => r.Student!.SchoolId == schoolId.Value);

            if (classId.HasValue)
                query = query.Where(r => r.Student!.ClassId == classId.Value);

            var results = await query
                .OrderByDescending(r => r.AttemptedAt)
                .ToListAsync();

            var resultDtos = results.Select(r => new ExamResultResponseDto
            {
                Id = r.Id,
                StudentId = r.StudentId,
                StudentName = r.Student?.FullName ?? "",
                StudentRollNo = r.Student?.RollNo ?? "",
                SchoolName = r.Student?.School?.SchoolName ?? "",
                ClassName = r.Student?.Class?.ClassName ?? "",
                ExamId = r.ExamId,
                ExamTitle = r.Exam?.Title ?? "",
                RoboticsLevelName = r.Exam?.RoboticsLevel?.Name ?? "",
                LevelNumber = r.Exam?.RoboticsLevel?.LevelNumber ?? 0,
                ScoreObtained = r.ScoreObtained,
                TotalMarks = r.TotalMarks,
                Percentage = r.Percentage,
                IsPassed = r.IsPassed,
                TimeTakenSeconds = r.TimeTakenSeconds,
                AttemptedAt = r.AttemptedAt,
                CertificateGenerated = r.CertificateId.HasValue,
                CertificateId = r.CertificateId
            }).ToList();

            return new ExamResultsListDto
            {
                Results = resultDtos,
                TotalCount = resultDtos.Count,
                PassedCount = resultDtos.Count(r => r.IsPassed),
                FailedCount = resultDtos.Count(r => !r.IsPassed),
                AveragePercentage = resultDtos.Count > 0 ? Math.Round(resultDtos.Average(r => r.Percentage), 2) : 0
            };
        }

        public async Task<ExamStatisticsDto?> GetExamStatisticsAsync(int examId)
        {
            var exam = await _context.Exams.FindAsync(examId);
            if (exam == null) return null;

            var results = await _context.ExamResults
                .Where(r => r.ExamId == examId)
                .ToListAsync();

            if (results.Count == 0)
            {
                return new ExamStatisticsDto
                {
                    ExamId = examId,
                    ExamTitle = exam.Title,
                    TotalAttempts = 0
                };
            }

            return new ExamStatisticsDto
            {
                ExamId = examId,
                ExamTitle = exam.Title,
                TotalAttempts = results.Count,
                PassedCount = results.Count(r => r.IsPassed),
                FailedCount = results.Count(r => !r.IsPassed),
                PassRate = Math.Round((decimal)results.Count(r => r.IsPassed) / results.Count * 100, 2),
                AverageScore = Math.Round(results.Average(r => r.ScoreObtained), 2),
                HighestScore = results.Max(r => r.ScoreObtained),
                LowestScore = results.Min(r => r.ScoreObtained),
                AverageTimeTakenMinutes = Math.Round(results.Average(r => r.TimeTakenSeconds) / 60.0, 2)
            };
        }

        #endregion

        #region Teacher Operations

        public async Task<TeacherExamResultsListDto> GetTeacherExamResultsAsync(int teacherId, int? classId = null, int? examId = null)
        {
            // Get classes assigned to this teacher
            var teacherClassIds = await _context.TeacherClasses
                .Where(tc => tc.TeacherId == teacherId)
                .Select(tc => tc.ClassId)
                .ToListAsync();

            if (teacherClassIds.Count == 0)
            {
                return new TeacherExamResultsListDto();
            }

            var query = _context.ExamResults
                .Include(r => r.Student)
                    .ThenInclude(s => s!.Class)
                .Include(r => r.Exam)
                    .ThenInclude(e => e!.RoboticsLevel)
                .Where(r => teacherClassIds.Contains(r.Student!.ClassId));

            if (classId.HasValue)
                query = query.Where(r => r.Student!.ClassId == classId.Value);

            if (examId.HasValue)
                query = query.Where(r => r.ExamId == examId.Value);

            var results = await query
                .OrderByDescending(r => r.AttemptedAt)
                .ToListAsync();

            var resultDtos = results.Select(r => new TeacherExamResultDto
            {
                Id = r.Id,
                StudentId = r.StudentId,
                StudentName = r.Student?.FullName ?? "",
                StudentRollNo = r.Student?.RollNo ?? "",
                ClassName = r.Student?.Class?.ClassName ?? "",
                ExamId = r.ExamId,
                ExamTitle = r.Exam?.Title ?? "",
                RoboticsLevelName = r.Exam?.RoboticsLevel?.Name ?? "",
                LevelNumber = r.Exam?.RoboticsLevel?.LevelNumber ?? 0,
                ScoreObtained = r.ScoreObtained,
                TotalMarks = r.TotalMarks,
                Percentage = r.Percentage,
                IsPassed = r.IsPassed,
                TimeTakenSeconds = r.TimeTakenSeconds,
                TimeTakenFormatted = ExamEvaluator.FormatTime(r.TimeTakenSeconds),
                AttemptedAt = r.AttemptedAt,
                CertificateGenerated = r.CertificateId.HasValue
            }).ToList();

            return new TeacherExamResultsListDto
            {
                Results = resultDtos,
                TotalCount = resultDtos.Count,
                PassedCount = resultDtos.Count(r => r.IsPassed),
                FailedCount = resultDtos.Count(r => !r.IsPassed),
                AveragePercentage = resultDtos.Count > 0 ? Math.Round(resultDtos.Average(r => r.Percentage), 2) : 0
            };
        }

        public async Task<TeacherExamOverviewDto?> GetTeacherExamOverviewAsync(int teacherId, int examId)
        {
            var exam = await _context.Exams
                .Include(e => e.RoboticsLevel)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null) return null;

            // Get classes assigned to this teacher
            var teacherClasses = await _context.TeacherClasses
                .Include(tc => tc.Class)
                .Where(tc => tc.TeacherId == teacherId)
                .ToListAsync();

            var classSummaries = new List<ClassExamSummaryDto>();

            foreach (var tc in teacherClasses)
            {
                var classId = tc.ClassId;
                var students = await _context.Students
                    .Where(s => s.ClassId == classId && s.IsActive)
                    .ToListAsync();

                var results = await _context.ExamResults
                    .Where(r => r.ExamId == examId && students.Select(s => s.Id).Contains(r.StudentId))
                    .ToListAsync();

                classSummaries.Add(new ClassExamSummaryDto
                {
                    ClassId = classId,
                    ClassName = tc.Class?.ClassName ?? "",
                    TotalStudents = students.Count,
                    StudentsAttempted = results.Select(r => r.StudentId).Distinct().Count(),
                    StudentsPassed = results.Count(r => r.IsPassed),
                    StudentsFailed = results.Count(r => !r.IsPassed),
                    StudentsPending = students.Count - results.Select(r => r.StudentId).Distinct().Count(),
                    PassRate = results.Count > 0 ? Math.Round((decimal)results.Count(r => r.IsPassed) / results.Count * 100, 2) : 0,
                    AverageScore = results.Count > 0 ? Math.Round(results.Average(r => r.ScoreObtained), 2) : 0
                });
            }

            return new TeacherExamOverviewDto
            {
                ExamId = exam.Id,
                ExamTitle = exam.Title,
                RoboticsLevelName = exam.RoboticsLevel?.Name ?? "",
                LevelNumber = exam.RoboticsLevel?.LevelNumber ?? 0,
                DurationMinutes = exam.DurationMinutes,
                TotalQuestions = exam.TotalQuestions,
                TotalMarks = exam.TotalMarks,
                PassingPercentage = exam.PassingPercentage,
                ClassSummaries = classSummaries
            };
        }

        public async Task<List<StudentExamEligibilityDto>> GetStudentsExamEligibilityAsync(int teacherId, int classId, int roboticsLevelId)
        {
            // Verify teacher has access to this class
            var hasAccess = await _context.TeacherClasses
                .AnyAsync(tc => tc.TeacherId == teacherId && tc.ClassId == classId);

            if (!hasAccess)
                return new List<StudentExamEligibilityDto>();

            var students = await _context.Students
                .Where(s => s.ClassId == classId && s.IsActive)
                .ToListAsync();

            var totalExperiments = await _context.Experiments
                .CountAsync(e => e.RoboticsLevelId == roboticsLevelId);

            var exam = await _context.Exams
                .FirstOrDefaultAsync(e => e.RoboticsLevelId == roboticsLevelId && e.IsActive);

            var eligibilityList = new List<StudentExamEligibilityDto>();

            foreach (var student in students)
            {
                var completedExperiments = await _context.StudentProgress
                    .CountAsync(p => p.StudentId == student.Id &&
                                    p.Experiment!.RoboticsLevelId == roboticsLevelId &&
                                    p.Completed &&
                                    p.IsApprovedByTeacher);

                ExamResult? examResult = null;
                if (exam != null)
                {
                    examResult = await _context.ExamResults
                        .OrderByDescending(r => r.AttemptedAt)
                        .FirstOrDefaultAsync(r => r.StudentId == student.Id && r.ExamId == exam.Id);
                }

                eligibilityList.Add(new StudentExamEligibilityDto
                {
                    StudentId = student.Id,
                    StudentName = student.FullName,
                    RollNo = student.RollNo,
                    IsEligible = completedExperiments >= totalExperiments && totalExperiments > 0,
                    TotalExperiments = totalExperiments,
                    CompletedExperiments = completedExperiments,
                    PendingExperiments = totalExperiments - completedExperiments,
                    HasAttemptedExam = examResult != null,
                    HasPassedExam = examResult?.IsPassed ?? false
                });
            }

            return eligibilityList.OrderBy(e => e.RollNo).ToList();
        }

        #endregion

        #region Student Operations

        public async Task<ExamEligibilityDto> CheckExamEligibilityAsync(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.School)
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                return new ExamEligibilityDto
                {
                    IsEligible = false,
                    Message = "Student not found"
                };
            }

            // Get student's current robotics level
            var roboticsLevelId = await GetStudentRoboticsLevelAsync(student);
            if (!roboticsLevelId.HasValue)
            {
                return new ExamEligibilityDto
                {
                    IsEligible = false,
                    Message = "No robotics level assigned for your class"
                };
            }

            // Get total experiments for this level
            var totalExperiments = await _context.Experiments
                .CountAsync(e => e.RoboticsLevelId == roboticsLevelId.Value);

            // Get completed and approved experiments
            var completedExperiments = await _context.StudentProgress
                .CountAsync(p => p.StudentId == studentId &&
                                p.Experiment!.RoboticsLevelId == roboticsLevelId.Value &&
                                p.Completed &&
                                p.IsApprovedByTeacher);

            // Check if exam exists for this level
            var exam = await _context.Exams
                .Include(e => e.RoboticsLevel)
                .FirstOrDefaultAsync(e => e.RoboticsLevelId == roboticsLevelId.Value && e.IsActive);

            if (exam == null)
            {
                return new ExamEligibilityDto
                {
                    IsEligible = false,
                    Message = "No exam available for your current level",
                    TotalExperiments = totalExperiments,
                    CompletedExperiments = completedExperiments,
                    PendingExperiments = totalExperiments - completedExperiments
                };
            }

            // Check if student has already passed
            var existingResult = await _context.ExamResults
                .OrderByDescending(r => r.AttemptedAt)
                .FirstOrDefaultAsync(r => r.StudentId == studentId && r.ExamId == exam.Id);

            if (existingResult?.IsPassed == true)
            {
                return new ExamEligibilityDto
                {
                    IsEligible = false,
                    Message = "You have already passed this exam",
                    TotalExperiments = totalExperiments,
                    CompletedExperiments = completedExperiments,
                    PendingExperiments = 0,
                    HasAttemptedExam = true,
                    HasPassedExam = true,
                    Exam = CreateExamPreview(exam)
                };
            }

            // Check if all experiments are completed
            var isEligible = completedExperiments >= totalExperiments && totalExperiments > 0;

            return new ExamEligibilityDto
            {
                IsEligible = isEligible,
                Message = isEligible
                    ? "You are eligible to take the exam"
                    : $"Complete all experiments to unlock the exam ({completedExperiments}/{totalExperiments} completed)",
                TotalExperiments = totalExperiments,
                CompletedExperiments = completedExperiments,
                PendingExperiments = totalExperiments - completedExperiments,
                HasAttemptedExam = existingResult != null,
                HasPassedExam = false,
                Exam = isEligible ? CreateExamPreview(exam) : null
            };
        }

        public async Task<ExamQuestionsDto?> StartExamAsync(int studentId)
        {
            var eligibility = await CheckExamEligibilityAsync(studentId);
            if (!eligibility.IsEligible || eligibility.Exam == null)
            {
                return null;
            }

            var exam = await _context.Exams
                .FirstOrDefaultAsync(e => e.Id == eligibility.Exam.ExamId);

            if (exam == null) return null;

            var questions = ExamEvaluator.ParseQuestionsJson(exam.QuestionsJson);

            var startTime = DateTime.UtcNow;

            return new ExamQuestionsDto
            {
                ExamId = exam.Id,
                Title = exam.Title,
                DurationMinutes = exam.DurationMinutes,
                TotalQuestions = exam.TotalQuestions,
                TotalMarks = exam.TotalMarks,
                StartedAt = startTime,
                MustEndBy = startTime.AddMinutes(exam.DurationMinutes),
                Questions = questions.Select(q => new StudentQuestionDto
                {
                    QuestionNumber = q.QuestionNumber,
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType,
                    Options = q.Options,
                    Marks = q.Marks
                }).ToList()
            };
        }

        public async Task<StudentExamResultDto> SubmitExamAsync(int studentId, ExamAnswerSubmitDto dto)
        {
            var student = await _context.Students.FindAsync(studentId)
                ?? throw new ArgumentException("Student not found");

            var exam = await _context.Exams
                .Include(e => e.RoboticsLevel)
                .FirstOrDefaultAsync(e => e.Id == dto.ExamId)
                ?? throw new ArgumentException("Exam not found");

            var questions = ExamEvaluator.ParseQuestionsJson(exam.QuestionsJson);

            // Evaluate answers
            var evaluation = ExamEvaluator.Evaluate(questions, dto.Answers);
            var isPassed = evaluation.Percentage >= exam.PassingPercentage;

            // Create exam result
            var result = new ExamResult
            {
                StudentId = studentId,
                ExamId = exam.Id,
                AcademicYearId = student.CurrentAcademicYearId ?? 0,
                ScoreObtained = evaluation.ScoreObtained,
                TotalMarks = evaluation.TotalMarks,
                Percentage = evaluation.Percentage,
                IsPassed = isPassed,
                AnswersJson = ExamEvaluator.SerializeAnswers(dto.Answers),
                TimeTakenSeconds = dto.TimeTakenSeconds,
                AttemptedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.ExamResults.Add(result);

            // Generate certificate if passed
            if (isPassed)
            {
                var certificate = await GenerateCertificateAsync(student, exam, result);
                result.CertificateId = certificate.Id;
            }

            await _context.SaveChangesAsync();

            return new StudentExamResultDto
            {
                ResultId = result.Id,
                ExamId = exam.Id,
                ExamTitle = exam.Title,
                RoboticsLevelName = exam.RoboticsLevel?.Name ?? "",
                LevelNumber = exam.RoboticsLevel?.LevelNumber ?? 0,
                ScoreObtained = evaluation.ScoreObtained,
                TotalMarks = evaluation.TotalMarks,
                Percentage = evaluation.Percentage,
                IsPassed = isPassed,
                ResultMessage = isPassed
                    ? "Congratulations! You have passed the exam and earned a certificate."
                    : "You did not pass. Review the material and try again.",
                TimeTakenSeconds = dto.TimeTakenSeconds,
                TimeTakenFormatted = ExamEvaluator.FormatTime(dto.TimeTakenSeconds),
                AttemptedAt = result.AttemptedAt,
                CertificateGenerated = isPassed,
                CertificateId = result.CertificateId,
                QuestionResults = evaluation.QuestionResults
            };
        }

        public async Task<ExamHistoryDto> GetExamHistoryAsync(int studentId)
        {
            var results = await _context.ExamResults
                .Include(r => r.Exam)
                    .ThenInclude(e => e!.RoboticsLevel)
                .Where(r => r.StudentId == studentId)
                .OrderByDescending(r => r.AttemptedAt)
                .ToListAsync();

            return new ExamHistoryDto
            {
                Results = results.Select(r => new StudentExamResultDto
                {
                    ResultId = r.Id,
                    ExamId = r.ExamId,
                    ExamTitle = r.Exam?.Title ?? "",
                    RoboticsLevelName = r.Exam?.RoboticsLevel?.Name ?? "",
                    LevelNumber = r.Exam?.RoboticsLevel?.LevelNumber ?? 0,
                    ScoreObtained = r.ScoreObtained,
                    TotalMarks = r.TotalMarks,
                    Percentage = r.Percentage,
                    IsPassed = r.IsPassed,
                    ResultMessage = r.IsPassed ? "Passed" : "Failed",
                    TimeTakenSeconds = r.TimeTakenSeconds,
                    TimeTakenFormatted = ExamEvaluator.FormatTime(r.TimeTakenSeconds),
                    AttemptedAt = r.AttemptedAt,
                    CertificateGenerated = r.CertificateId.HasValue,
                    CertificateId = r.CertificateId
                }).ToList(),
                TotalAttempts = results.Count,
                PassedCount = results.Count(r => r.IsPassed)
            };
        }

        public async Task<StudentExamResultDto?> GetExamResultAsync(int studentId, int resultId)
        {
            var result = await _context.ExamResults
                .Include(r => r.Exam)
                    .ThenInclude(e => e!.RoboticsLevel)
                .FirstOrDefaultAsync(r => r.Id == resultId && r.StudentId == studentId);

            if (result == null) return null;

            var questions = ExamEvaluator.ParseQuestionsJson(result.Exam?.QuestionsJson);
            var answers = string.IsNullOrEmpty(result.AnswersJson)
                ? new List<AnswerDto>()
                : System.Text.Json.JsonSerializer.Deserialize<List<AnswerDto>>(result.AnswersJson,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new List<AnswerDto>();

            var questionResults = questions.Select(q =>
            {
                var answer = answers.FirstOrDefault(a => a.QuestionNumber == q.QuestionNumber);
                var studentAnswer = answer?.Answer ?? "";
                var isCorrect = studentAnswer.Equals(q.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
                return new QuestionResultDto
                {
                    QuestionNumber = q.QuestionNumber,
                    QuestionText = q.QuestionText,
                    YourAnswer = studentAnswer,
                    CorrectAnswer = q.CorrectAnswer,
                    IsCorrect = isCorrect,
                    MarksObtained = isCorrect ? q.Marks : 0,
                    TotalMarks = q.Marks,
                    Explanation = q.Explanation
                };
            }).ToList();

            return new StudentExamResultDto
            {
                ResultId = result.Id,
                ExamId = result.ExamId,
                ExamTitle = result.Exam?.Title ?? "",
                RoboticsLevelName = result.Exam?.RoboticsLevel?.Name ?? "",
                LevelNumber = result.Exam?.RoboticsLevel?.LevelNumber ?? 0,
                ScoreObtained = result.ScoreObtained,
                TotalMarks = result.TotalMarks,
                Percentage = result.Percentage,
                IsPassed = result.IsPassed,
                ResultMessage = result.IsPassed ? "Passed" : "Failed",
                TimeTakenSeconds = result.TimeTakenSeconds,
                TimeTakenFormatted = ExamEvaluator.FormatTime(result.TimeTakenSeconds),
                AttemptedAt = result.AttemptedAt,
                CertificateGenerated = result.CertificateId.HasValue,
                CertificateId = result.CertificateId,
                QuestionResults = questionResults
            };
        }

        #endregion

        #region Helper Methods

        public async Task<int?> GetStudentIdByUserIdAsync(string userId)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            return student?.Id;
        }

        public async Task<int?> GetTeacherIdByUserIdAsync(string userId)
        {
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            return teacher?.Id;
        }

        private async Task<int?> GetStudentRoboticsLevelAsync(Models.Student student)
        {
            if (!student.CurrentAcademicYearId.HasValue) return null;
            var mapping = await _context.SchoolLevelMappings
                .FirstOrDefaultAsync(m => m.SchoolId == student.SchoolId &&
                                        m.ClassId == student.ClassId &&
                                        m.AcademicYearId == student.CurrentAcademicYearId.Value);
            return mapping?.RoboticsLevelId;
        }

        private ExamPreviewDto CreateExamPreview(Exam exam)
        {
            return new ExamPreviewDto
            {
                ExamId = exam.Id,
                Title = exam.Title,
                Description = exam.Description,
                DurationMinutes = exam.DurationMinutes,
                TotalQuestions = exam.TotalQuestions,
                TotalMarks = exam.TotalMarks,
                PassingPercentage = exam.PassingPercentage,
                PassingMarks = Math.Ceiling(exam.TotalMarks * exam.PassingPercentage / 100),
                RoboticsLevelName = exam.RoboticsLevel?.Name ?? "",
                LevelNumber = exam.RoboticsLevel?.LevelNumber ?? 0
            };
        }

        private async Task<Certificate> GenerateCertificateAsync(Models.Student student, Exam exam, ExamResult result)
        {
            var school = await _context.Schools.FindAsync(student.SchoolId);
            var roboticsLevel = exam.RoboticsLevel ?? await _context.RoboticsLevels.FindAsync(exam.RoboticsLevelId);

            var certificateNumber = $"MR-{DateTime.UtcNow:yyyyMMdd}-{student.Id}-{exam.RoboticsLevelId}";

            var certificate = new Certificate
            {
                CertificateNumber = certificateNumber,
                StudentId = student.Id,
                StudentName = student.FullName,
                SchoolId = student.SchoolId,
                RoboticsLevelId = exam.RoboticsLevelId,
                LevelName = roboticsLevel?.Name ?? "",
                LevelNumber = roboticsLevel?.LevelNumber ?? 0,
                AcademicYearId = student.CurrentAcademicYearId ?? 0,
                ExamScore = result.ScoreObtained,
                PassingScore = Math.Ceiling(exam.TotalMarks * exam.PassingPercentage / 100),
                Title = $"Certificate of Completion - {roboticsLevel?.Name}",
                IssuedDate = DateTime.UtcNow
            };

            _context.Certificates.Add(certificate);
            await _context.SaveChangesAsync();

            return certificate;
        }

        #endregion
    }
}
