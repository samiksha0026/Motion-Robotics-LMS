using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Lab;

public class GetLabInfoHandler : IQueryHandler<GetLabInfoQuery, LabInfoDto?>
{
    private readonly ILabService _s;
    public GetLabInfoHandler(ILabService s) => _s = s;
    public Task<LabInfoDto?> Handle(GetLabInfoQuery r, CancellationToken ct) => _s.GetLabInfoAsync(r.SchoolId);
}

public class GetAllSchoolsLabSummaryHandler : IQueryHandler<GetAllSchoolsLabSummaryQuery, List<SchoolLabSummaryDto>>
{
    private readonly ILabService _s;
    public GetAllSchoolsLabSummaryHandler(ILabService s) => _s = s;
    public Task<List<SchoolLabSummaryDto>> Handle(GetAllSchoolsLabSummaryQuery r, CancellationToken ct)
        => _s.GetAllSchoolsLabSummaryAsync();
}

public class UpdateLabInfoHandler : ICommandHandler<UpdateLabInfoCommand, LabInfoDto>
{
    private readonly ILabService _s;
    public UpdateLabInfoHandler(ILabService s) => _s = s;
    public Task<LabInfoDto> Handle(UpdateLabInfoCommand r, CancellationToken ct)
        => _s.UpdateLabInfoAsync(r.SchoolId, r.Data);
}

public class UploadLabPhotoHandler : ICommandHandler<UploadLabPhotoCommand, LabPhotoDto>
{
    private readonly ILabService _s;
    public UploadLabPhotoHandler(ILabService s) => _s = s;
    public Task<LabPhotoDto> Handle(UploadLabPhotoCommand r, CancellationToken ct)
        => _s.UploadPhotoAsync(r.SchoolId, r.File, r.Caption);
}

public class DeleteLabPhotoHandler : ICommandHandler<DeleteLabPhotoCommand, bool>
{
    private readonly ILabService _s;
    public DeleteLabPhotoHandler(ILabService s) => _s = s;
    public Task<bool> Handle(DeleteLabPhotoCommand r, CancellationToken ct)
        => _s.DeletePhotoAsync(r.PhotoId, r.SchoolId);
}
