using MediatR;
using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.RoboticsLevels;

public class GetAllRoboticsLevelsHandler : IQueryHandler<GetAllRoboticsLevelsQuery, List<RoboticsLevelDto>>
{
    private readonly IRoboticsLevelService _s;
    public GetAllRoboticsLevelsHandler(IRoboticsLevelService s) => _s = s;
    public Task<List<RoboticsLevelDto>> Handle(GetAllRoboticsLevelsQuery r, CancellationToken ct) => _s.GetAllLevelsAsync();
}

public class GetRoboticsLevelByIdHandler : IQueryHandler<GetRoboticsLevelByIdQuery, RoboticsLevelDetailDto?>
{
    private readonly IRoboticsLevelService _s;
    public GetRoboticsLevelByIdHandler(IRoboticsLevelService s) => _s = s;
    public Task<RoboticsLevelDetailDto?> Handle(GetRoboticsLevelByIdQuery r, CancellationToken ct) => _s.GetLevelWithExperimentsAsync(r.Id);
}

public class UpdateSyllabusUrlHandler : ICommandHandler<UpdateSyllabusUrlCommand, bool>
{
    private readonly IRoboticsLevelService _s;
    public UpdateSyllabusUrlHandler(IRoboticsLevelService s) => _s = s;
    public Task<bool> Handle(UpdateSyllabusUrlCommand r, CancellationToken ct) => _s.UpdateSyllabusUrlAsync(r.Id, r.SyllabusUrl);
}

public class SeedSyllabusUrlsHandler : ICommandHandler<SeedSyllabusUrlsCommand>
{
    private readonly IRoboticsLevelService _s;
    public SeedSyllabusUrlsHandler(IRoboticsLevelService s) => _s = s;
    public Task Handle(SeedSyllabusUrlsCommand r, CancellationToken ct) => _s.SeedSyllabusUrlsAsync();
}

public class SeedSampleExperimentsHandler : ICommandHandler<SeedSampleExperimentsCommand>
{
    private readonly IRoboticsLevelService _s;
    public SeedSampleExperimentsHandler(IRoboticsLevelService s) => _s = s;
    public Task Handle(SeedSampleExperimentsCommand r, CancellationToken ct) => _s.SeedSampleExperimentsAsync();
}
