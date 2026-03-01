using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Student;
using MotionRobotics.LMS.API.Services;

namespace MotionRobotics.LMS.API.Features.Student.Experiments;

public class GetStudentExperimentsHandler : IQueryHandler<GetStudentExperimentsQuery, StudentExperimentsListDto?>
{
    private readonly IStudentService _s;
    public GetStudentExperimentsHandler(IStudentService s) => _s = s;
    public async Task<StudentExperimentsListDto?> Handle(GetStudentExperimentsQuery r, CancellationToken ct)
    {
        var studentId = await _s.GetStudentIdByUserIdAsync(r.UserId);
        return studentId.HasValue ? await _s.GetExperimentsAsync(studentId.Value) : null;
    }
}

public class GetStudentExperimentDetailHandler : IQueryHandler<GetStudentExperimentDetailQuery, StudentExperimentDto?>
{
    private readonly IStudentService _s;
    public GetStudentExperimentDetailHandler(IStudentService s) => _s = s;
    public async Task<StudentExperimentDto?> Handle(GetStudentExperimentDetailQuery r, CancellationToken ct)
    {
        var studentId = await _s.GetStudentIdByUserIdAsync(r.UserId);
        return studentId.HasValue ? await _s.GetExperimentDetailAsync(studentId.Value, r.ExperimentId) : null;
    }
}

public class SubmitStudentExperimentHandler : ICommandHandler<SubmitStudentExperimentCommand, (bool Success, string Message, ExperimentSubmissionResponseDto? Result)>
{
    private readonly IStudentService _s;
    public SubmitStudentExperimentHandler(IStudentService s) => _s = s;
    public async Task<(bool Success, string Message, ExperimentSubmissionResponseDto? Result)> Handle(SubmitStudentExperimentCommand r, CancellationToken ct)
    {
        var studentId = await _s.GetStudentIdByUserIdAsync(r.UserId);
        if (!studentId.HasValue) return (false, "Student profile not found", null);
        return await _s.SubmitExperimentAsync(studentId.Value, r.ExperimentId, r.Data);
    }
}
