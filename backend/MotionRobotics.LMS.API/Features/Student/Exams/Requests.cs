using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Student;

namespace MotionRobotics.LMS.API.Features.Student.Exams;

public record CheckExamEligibilityQuery(string UserId) : IQuery<ExamEligibilityDto>;
public record StartExamQuery(string UserId) : IQuery<ExamQuestionsDto?>;
public record SubmitExamCommand(string UserId, ExamAnswerSubmitDto Data) : ICommand<StudentExamResultDto>;
public record GetExamHistoryQuery(string UserId) : IQuery<ExamHistoryDto>;
public record GetExamResultQuery(string UserId, int ResultId) : IQuery<StudentExamResultDto?>;
