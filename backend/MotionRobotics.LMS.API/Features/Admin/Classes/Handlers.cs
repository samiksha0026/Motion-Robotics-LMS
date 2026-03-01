using MediatR;
using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Classes;

public class GetAllClassesHandler : IQueryHandler<GetAllClassesQuery, List<ClassResponseDto>>
{
    private readonly IClassService _s;
    public GetAllClassesHandler(IClassService s) => _s = s;
    public Task<List<ClassResponseDto>> Handle(GetAllClassesQuery r, CancellationToken ct) => _s.GetAllClassesAsync();
}

public class GetClassByIdHandler : IQueryHandler<GetClassByIdQuery, ClassResponseDto?>
{
    private readonly IClassService _s;
    public GetClassByIdHandler(IClassService s) => _s = s;
    public Task<ClassResponseDto?> Handle(GetClassByIdQuery r, CancellationToken ct) => _s.GetClassByIdAsync(r.Id);
}

public class GetClassesBySchoolHandler : IQueryHandler<GetClassesBySchoolQuery, List<ClassResponseDto>>
{
    private readonly IClassService _s;
    public GetClassesBySchoolHandler(IClassService s) => _s = s;
    public Task<List<ClassResponseDto>> Handle(GetClassesBySchoolQuery r, CancellationToken ct) => _s.GetClassesBySchoolAsync(r.SchoolId);
}

public class CreateClassHandler : ICommandHandler<CreateClassCommand, ClassResponseDto>
{
    private readonly IClassService _s;
    public CreateClassHandler(IClassService s) => _s = s;
    public Task<ClassResponseDto> Handle(CreateClassCommand r, CancellationToken ct) => _s.CreateClassAsync(r.Data);
}

public class UpdateClassHandler : ICommandHandler<UpdateClassCommand, ClassResponseDto>
{
    private readonly IClassService _s;
    public UpdateClassHandler(IClassService s) => _s = s;
    public Task<ClassResponseDto> Handle(UpdateClassCommand r, CancellationToken ct) => _s.UpdateClassAsync(r.Id, r.Data);
}

public class DeleteClassHandler : ICommandHandler<DeleteClassCommand, bool>
{
    private readonly IClassService _s;
    public DeleteClassHandler(IClassService s) => _s = s;
    public Task<bool> Handle(DeleteClassCommand r, CancellationToken ct) => _s.DeleteClassAsync(r.Id);
}
