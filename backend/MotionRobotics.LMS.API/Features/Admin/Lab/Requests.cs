using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Lab;

public record GetLabInfoQuery(int SchoolId) : IQuery<LabInfoDto?>;
public record GetAllSchoolsLabSummaryQuery : IQuery<List<SchoolLabSummaryDto>>;
public record UpdateLabInfoCommand(int SchoolId, UpdateLabInfoDto Data) : ICommand<LabInfoDto>;
public record UploadLabPhotoCommand(int SchoolId, IFormFile File, string? Caption) : ICommand<LabPhotoDto>;
public record DeleteLabPhotoCommand(int PhotoId, int SchoolId) : ICommand<bool>;
