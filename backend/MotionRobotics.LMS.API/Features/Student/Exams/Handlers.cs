using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Student;
using MotionRobotics.LMS.API.Services;

namespace MotionRobotics.LMS.API.Features.Student.Exams;

public class CheckExamEligibilityHandler : IQueryHandler<CheckExamEligibilityQuery, ExamEligibilityDto>
{
    private readonly IExamService _s;
    public CheckExamEligibilityHandler(IExamService s) => _s = s;
    public async Task<ExamEligibilityDto> Handle(CheckExamEligibilityQuery r, CancellationToken ct)
    {
        var studentId = await _s.GetStudentIdByUserIdAsync(r.UserId)
            ?? throw new KeyNotFoundException("Student profile not found");
        return await _s.CheckExamEligibilityAsync(studentId);
    }
}

public class StartExamHandler : IQueryHandler<StartExamQuery, ExamQuestionsDto?>
{
    private readonly IExamService _s;
    public StartExamHandler(IExamService s) => _s = s;
    public async Task<ExamQuestionsDto?> Handle(StartExamQuery r, CancellationToken ct)
    {
        var studentId = await _s.GetStudentIdByUserIdAsync(r.UserId)
            ?? throw new KeyNotFoundException("Student profile not found");
        return await _s.StartExamAsync(studentId);
    }
}

public class SubmitExamHandler : ICommandHandler<SubmitExamCommand, StudentExamResultDto>
{
    private readonly IExamService _s;
    public SubmitExamHandler(IExamService s) => _s = s;
    public async Task<StudentExamResultDto> Handle(SubmitExamCommand r, CancellationToken ct)
    {
        var studentId = await _s.GetStudentIdByUserIdAsync(r.UserId)
            ?? throw new KeyNotFoundException("Student profile not found");
        return await _s.SubmitExamAsync(studentId, r.Data);
    }
}

public class GetExamHistoryHandler : IQueryHandler<GetExamHistoryQuery, ExamHistoryDto>
{
    private readonly IExamService _s;
    public GetExamHistoryHandler(IExamService s) => _s = s;
    public async Task<ExamHistoryDto> Handle(GetExamHistoryQuery r, CancellationToken ct)
    {
        var studentId = await _s.GetStudentIdByUserIdAsync(r.UserId)
            ?? throw new KeyNotFoundException("Student profile not found");
        return await _s.GetExamHistoryAsync(studentId);
    }
}

public class GetExamResultHandler : IQueryHandler<GetExamResultQuery, StudentExamResultDto?>
{
    private readonly IExamService _s;
    public GetExamResultHandler(IExamService s) => _s = s;
    public async Task<StudentExamResultDto?> Handle(GetExamResultQuery r, CancellationToken ct)
    {
        var studentId = await _s.GetStudentIdByUserIdAsync(r.UserId)
            ?? throw new KeyNotFoundException("Student profile not found");
        return await _s.GetExamResultAsync(studentId, r.ResultId);
    }
}
