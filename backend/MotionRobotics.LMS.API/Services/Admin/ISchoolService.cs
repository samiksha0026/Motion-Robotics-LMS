using Microsoft.AspNetCore.Http;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Services.Admin
{
    public interface ISchoolService
    {
        Task<SchoolResponseDto?> GetSchoolByIdAsync(int id);
        Task<List<SchoolResponseDto>> GetAllSchoolsAsync();
        Task<SchoolResponseDto> CreateSchoolAsync(SchoolCreateDto dto);
        Task<SchoolResponseDto> UpdateSchoolAsync(int id, SchoolCreateDto dto);
        Task<bool> DeleteSchoolAsync(int id);
        Task<string> UploadLogoAsync(int id, IFormFile file);
        Task<bool> ToggleSchoolStatusAsync(int id);
        Task<string> ResetSchoolAdminPasswordAsync(int id);
    }
}
