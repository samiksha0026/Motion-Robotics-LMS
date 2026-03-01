using MediatR;
using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.LevelMappings;

public class GetAllMappingsHandler : IQueryHandler<GetAllMappingsQuery, List<LevelMappingDto>>
{
    private readonly ILevelMappingService _s;
    public GetAllMappingsHandler(ILevelMappingService s) => _s = s;
    public Task<List<LevelMappingDto>> Handle(GetAllMappingsQuery r, CancellationToken ct) => _s.GetAllMappingsAsync(r.SchoolId, r.AcademicYearId);
}

public class GetMappingByIdHandler : IQueryHandler<GetMappingByIdQuery, LevelMappingDto?>
{
    private readonly ILevelMappingService _s;
    public GetMappingByIdHandler(ILevelMappingService s) => _s = s;
    public Task<LevelMappingDto?> Handle(GetMappingByIdQuery r, CancellationToken ct) => _s.GetMappingByIdAsync(r.Id);
}

public class GetSchoolLevelAssignmentsHandler : IQueryHandler<GetSchoolLevelAssignmentsQuery, SchoolLevelAssignmentsDto?>
{
    private readonly ILevelMappingService _s;
    public GetSchoolLevelAssignmentsHandler(ILevelMappingService s) => _s = s;
    public Task<SchoolLevelAssignmentsDto?> Handle(GetSchoolLevelAssignmentsQuery r, CancellationToken ct) => _s.GetSchoolLevelAssignmentsAsync(r.SchoolId, r.AcademicYearId);
}

public class CreateMappingHandler : ICommandHandler<CreateMappingCommand, LevelMappingDto>
{
    private readonly ILevelMappingService _s;
    public CreateMappingHandler(ILevelMappingService s) => _s = s;
    public Task<LevelMappingDto> Handle(CreateMappingCommand r, CancellationToken ct) => _s.CreateMappingAsync(r.Data);
}

public class CreateBulkMappingsHandler : ICommandHandler<CreateBulkMappingsCommand, List<LevelMappingDto>>
{
    private readonly ILevelMappingService _s;
    public CreateBulkMappingsHandler(ILevelMappingService s) => _s = s;
    public Task<List<LevelMappingDto>> Handle(CreateBulkMappingsCommand r, CancellationToken ct) => _s.CreateBulkMappingsAsync(r.Data);
}

public class UpdateMappingHandler : ICommandHandler<UpdateMappingCommand, LevelMappingDto>
{
    private readonly ILevelMappingService _s;
    public UpdateMappingHandler(ILevelMappingService s) => _s = s;
    public Task<LevelMappingDto> Handle(UpdateMappingCommand r, CancellationToken ct) => _s.UpdateMappingAsync(r.Id, r.Data);
}

public class DeleteMappingHandler : ICommandHandler<DeleteMappingCommand, bool>
{
    private readonly ILevelMappingService _s;
    public DeleteMappingHandler(ILevelMappingService s) => _s = s;
    public Task<bool> Handle(DeleteMappingCommand r, CancellationToken ct) => _s.DeleteMappingAsync(r.Id);
}

public class DeleteSchoolMappingsHandler : ICommandHandler<DeleteSchoolMappingsCommand, bool>
{
    private readonly ILevelMappingService _s;
    public DeleteSchoolMappingsHandler(ILevelMappingService s) => _s = s;
    public Task<bool> Handle(DeleteSchoolMappingsCommand r, CancellationToken ct) => _s.DeleteSchoolMappingsAsync(r.SchoolId, r.AcademicYearId);
}
