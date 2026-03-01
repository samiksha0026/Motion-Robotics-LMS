using MediatR;
using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.AcademicYears;

public class GetAllAcademicYearsHandler : IQueryHandler<GetAllAcademicYearsQuery, List<AcademicYearDto>>
{
    private readonly IAcademicYearService _s;
    public GetAllAcademicYearsHandler(IAcademicYearService s) => _s = s;
    public Task<List<AcademicYearDto>> Handle(GetAllAcademicYearsQuery r, CancellationToken ct) => _s.GetAllAcademicYearsAsync();
}

public class GetCurrentAcademicYearHandler : IQueryHandler<GetCurrentAcademicYearQuery, AcademicYearDto?>
{
    private readonly IAcademicYearService _s;
    public GetCurrentAcademicYearHandler(IAcademicYearService s) => _s = s;
    public Task<AcademicYearDto?> Handle(GetCurrentAcademicYearQuery r, CancellationToken ct) => _s.GetCurrentAcademicYearAsync();
}

public class GetAcademicYearByIdHandler : IQueryHandler<GetAcademicYearByIdQuery, AcademicYearDto?>
{
    private readonly IAcademicYearService _s;
    public GetAcademicYearByIdHandler(IAcademicYearService s) => _s = s;
    public Task<AcademicYearDto?> Handle(GetAcademicYearByIdQuery r, CancellationToken ct) => _s.GetAcademicYearByIdAsync(r.Id);
}

public class CreateAcademicYearHandler : ICommandHandler<CreateAcademicYearCommand, AcademicYearDto>
{
    private readonly IAcademicYearService _s;
    public CreateAcademicYearHandler(IAcademicYearService s) => _s = s;
    public Task<AcademicYearDto> Handle(CreateAcademicYearCommand r, CancellationToken ct) => _s.CreateAcademicYearAsync(r.Data);
}

public class UpdateAcademicYearHandler : ICommandHandler<UpdateAcademicYearCommand, AcademicYearDto>
{
    private readonly IAcademicYearService _s;
    public UpdateAcademicYearHandler(IAcademicYearService s) => _s = s;
    public Task<AcademicYearDto> Handle(UpdateAcademicYearCommand r, CancellationToken ct) => _s.UpdateAcademicYearAsync(r.Id, r.Data);
}

public class SetCurrentAcademicYearHandler : ICommandHandler<SetCurrentAcademicYearCommand, bool>
{
    private readonly IAcademicYearService _s;
    public SetCurrentAcademicYearHandler(IAcademicYearService s) => _s = s;
    public Task<bool> Handle(SetCurrentAcademicYearCommand r, CancellationToken ct) => _s.SetCurrentAcademicYearAsync(r.Id);
}

public class DeleteAcademicYearHandler : ICommandHandler<DeleteAcademicYearCommand, bool>
{
    private readonly IAcademicYearService _s;
    public DeleteAcademicYearHandler(IAcademicYearService s) => _s = s;
    public Task<bool> Handle(DeleteAcademicYearCommand r, CancellationToken ct) => _s.DeleteAcademicYearAsync(r.Id);
}
