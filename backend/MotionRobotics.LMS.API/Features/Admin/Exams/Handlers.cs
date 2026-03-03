using MediatR;
using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services;

namespace MotionRobotics.LMS.API.Features.Admin.Exams;

public class GetAllExamsHandler : IQueryHandler<GetAllExamsQuery, ExamListDto>
{
    private readonly IExamService _s;
    public GetAllExamsHandler(IExamService s) => _s = s;
    public Task<ExamListDto> Handle(GetAllExamsQuery r, CancellationToken ct) => _s.GetAllExamsAsync(r.RoboticsLevelId, r.IsActive);
}

public class GetExamByIdHandler : IQueryHandler<GetExamByIdQuery, ExamDetailDto?>
{
    private readonly IExamService _s;
    public GetExamByIdHandler(IExamService s) => _s = s;
    public Task<ExamDetailDto?> Handle(GetExamByIdQuery r, CancellationToken ct) => _s.GetExamByIdAsync(r.ExamId);
}

public class GetExamResultsHandler : IQueryHandler<GetExamResultsQuery, ExamResultsListDto>
{
    private readonly IExamService _s;
    public GetExamResultsHandler(IExamService s) => _s = s;
    public Task<ExamResultsListDto> Handle(GetExamResultsQuery r, CancellationToken ct) => _s.GetExamResultsAsync(r.ExamId, r.SchoolId, r.ClassId);
}

public class GetExamStatisticsHandler : IQueryHandler<GetExamStatisticsQuery, ExamStatisticsDto?>
{
    private readonly IExamService _s;
    public GetExamStatisticsHandler(IExamService s) => _s = s;
    public Task<ExamStatisticsDto?> Handle(GetExamStatisticsQuery r, CancellationToken ct) => _s.GetExamStatisticsAsync(r.ExamId, r.SchoolId);
}

public class CreateExamHandler : ICommandHandler<CreateExamCommand, ExamDetailDto>
{
    private readonly IExamService _s;
    public CreateExamHandler(IExamService s) => _s = s;
    public Task<ExamDetailDto> Handle(CreateExamCommand r, CancellationToken ct) => _s.CreateExamAsync(r.Data);
}

public class UpdateExamHandler : ICommandHandler<UpdateExamCommand, ExamDetailDto?>
{
    private readonly IExamService _s;
    public UpdateExamHandler(IExamService s) => _s = s;
    public Task<ExamDetailDto?> Handle(UpdateExamCommand r, CancellationToken ct) => _s.UpdateExamAsync(r.ExamId, r.Data);
}

public class DeleteExamHandler : ICommandHandler<DeleteExamCommand, bool>
{
    private readonly IExamService _s;
    public DeleteExamHandler(IExamService s) => _s = s;
    public Task<bool> Handle(DeleteExamCommand r, CancellationToken ct) => _s.DeleteExamAsync(r.ExamId);
}
