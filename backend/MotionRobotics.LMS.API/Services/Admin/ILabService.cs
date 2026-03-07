using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Services.Admin;

public interface ILabService
{
    Task<LabInfoDto?> GetLabInfoAsync(int schoolId);
    Task<LabInfoDto> UpdateLabInfoAsync(int schoolId, UpdateLabInfoDto dto);
    Task<LabPhotoDto> UploadPhotoAsync(int schoolId, IFormFile file, string? caption);
    Task<bool> DeletePhotoAsync(int photoId, int schoolId);
    Task<List<SchoolLabSummaryDto>> GetAllSchoolsLabSummaryAsync();
}
