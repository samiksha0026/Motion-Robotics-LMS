using MediatR;
using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Teachers;

public class GetAllTeachersHandler : IQueryHandler<GetAllTeachersQuery, List<TeacherResponseDto>>
{
    private readonly IAdminTeacherService _s;
    public GetAllTeachersHandler(IAdminTeacherService s) => _s = s;
    public Task<List<TeacherResponseDto>> Handle(GetAllTeachersQuery r, CancellationToken ct) => _s.GetAllTeachersAsync();
}

public class GetTeacherByIdHandler : IQueryHandler<GetTeacherByIdQuery, TeacherResponseDto?>
{
    private readonly IAdminTeacherService _s;
    public GetTeacherByIdHandler(IAdminTeacherService s) => _s = s;
    public Task<TeacherResponseDto?> Handle(GetTeacherByIdQuery r, CancellationToken ct) => _s.GetTeacherByIdAsync(r.Id);
}

public class GetTeachersBySchoolHandler : IQueryHandler<GetTeachersBySchoolQuery, List<TeacherResponseDto>>
{
    private readonly IAdminTeacherService _s;
    public GetTeachersBySchoolHandler(IAdminTeacherService s) => _s = s;
    public Task<List<TeacherResponseDto>> Handle(GetTeachersBySchoolQuery r, CancellationToken ct) => _s.GetTeachersBySchoolAsync(r.SchoolId);
}

public class CreateTeacherHandler : ICommandHandler<CreateTeacherCommand, TeacherResponseDto>
{
    private readonly IAdminTeacherService _s;
    public CreateTeacherHandler(IAdminTeacherService s) => _s = s;
    public Task<TeacherResponseDto> Handle(CreateTeacherCommand r, CancellationToken ct) => _s.CreateTeacherAsync(r.Data);
}

public class UpdateTeacherHandler : ICommandHandler<UpdateTeacherCommand, TeacherResponseDto>
{
    private readonly IAdminTeacherService _s;
    public UpdateTeacherHandler(IAdminTeacherService s) => _s = s;
    public Task<TeacherResponseDto> Handle(UpdateTeacherCommand r, CancellationToken ct) => _s.UpdateTeacherAsync(r.Id, r.Data);
}

public class DeleteTeacherHandler : ICommandHandler<DeleteTeacherCommand, bool>
{
    private readonly IAdminTeacherService _s;
    public DeleteTeacherHandler(IAdminTeacherService s) => _s = s;
    public Task<bool> Handle(DeleteTeacherCommand r, CancellationToken ct) => _s.DeleteTeacherAsync(r.Id);
}

public class AssignClassToTeacherHandler : ICommandHandler<AssignClassToTeacherCommand>
{
    private readonly IAdminTeacherService _s;
    public AssignClassToTeacherHandler(IAdminTeacherService s) => _s = s;
    public Task Handle(AssignClassToTeacherCommand r, CancellationToken ct) => _s.AssignClassToTeacherAsync(r.TeacherId, r.ClassId);
}

public class RemoveClassFromTeacherHandler : ICommandHandler<RemoveClassFromTeacherCommand>
{
    private readonly IAdminTeacherService _s;
    public RemoveClassFromTeacherHandler(IAdminTeacherService s) => _s = s;
    public Task Handle(RemoveClassFromTeacherCommand r, CancellationToken ct) => _s.RemoveClassFromTeacherAsync(r.TeacherId, r.ClassId);
}
