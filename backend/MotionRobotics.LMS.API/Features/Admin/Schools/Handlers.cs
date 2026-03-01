using MediatR;
using Microsoft.AspNetCore.Http;
using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Schools;

public class GetAllSchoolsHandler : IQueryHandler<GetAllSchoolsQuery, List<SchoolResponseDto>>
{
    private readonly ISchoolService _service;
    public GetAllSchoolsHandler(ISchoolService service) => _service = service;
    public Task<List<SchoolResponseDto>> Handle(GetAllSchoolsQuery request, CancellationToken ct)
        => _service.GetAllSchoolsAsync();
}

public class GetSchoolByIdHandler : IQueryHandler<GetSchoolByIdQuery, SchoolResponseDto?>
{
    private readonly ISchoolService _service;
    public GetSchoolByIdHandler(ISchoolService service) => _service = service;
    public Task<SchoolResponseDto?> Handle(GetSchoolByIdQuery request, CancellationToken ct)
        => _service.GetSchoolByIdAsync(request.Id);
}

public class CreateSchoolHandler : ICommandHandler<CreateSchoolCommand, SchoolResponseDto>
{
    private readonly ISchoolService _service;
    public CreateSchoolHandler(ISchoolService service) => _service = service;
    public Task<SchoolResponseDto> Handle(CreateSchoolCommand request, CancellationToken ct)
        => _service.CreateSchoolAsync(request.Data);
}

public class UpdateSchoolHandler : ICommandHandler<UpdateSchoolCommand, SchoolResponseDto>
{
    private readonly ISchoolService _service;
    public UpdateSchoolHandler(ISchoolService service) => _service = service;
    public Task<SchoolResponseDto> Handle(UpdateSchoolCommand request, CancellationToken ct)
        => _service.UpdateSchoolAsync(request.Id, request.Data);
}

public class DeleteSchoolHandler : ICommandHandler<DeleteSchoolCommand, bool>
{
    private readonly ISchoolService _service;
    public DeleteSchoolHandler(ISchoolService service) => _service = service;
    public Task<bool> Handle(DeleteSchoolCommand request, CancellationToken ct)
        => _service.DeleteSchoolAsync(request.Id);
}

public class UploadSchoolLogoHandler : ICommandHandler<UploadSchoolLogoCommand, string>
{
    private readonly ISchoolService _service;
    public UploadSchoolLogoHandler(ISchoolService service) => _service = service;
    public Task<string> Handle(UploadSchoolLogoCommand request, CancellationToken ct)
        => _service.UploadLogoAsync(request.Id, request.File);
}

public class ToggleSchoolStatusHandler : ICommandHandler<ToggleSchoolStatusCommand, bool>
{
    private readonly ISchoolService _service;
    public ToggleSchoolStatusHandler(ISchoolService service) => _service = service;
    public Task<bool> Handle(ToggleSchoolStatusCommand request, CancellationToken ct)
        => _service.ToggleSchoolStatusAsync(request.Id);
}

public class ResetSchoolAdminPasswordHandler : ICommandHandler<ResetSchoolAdminPasswordCommand, string>
{
    private readonly ISchoolService _service;
    public ResetSchoolAdminPasswordHandler(ISchoolService service) => _service = service;
    public Task<string> Handle(ResetSchoolAdminPasswordCommand request, CancellationToken ct)
        => _service.ResetSchoolAdminPasswordAsync(request.Id);
}
