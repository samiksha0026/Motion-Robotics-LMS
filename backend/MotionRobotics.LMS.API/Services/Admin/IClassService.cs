using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Services.Admin
{
    public interface IClassService
    {
        Task<ClassResponseDto?> GetClassByIdAsync(int id);
        Task<List<ClassResponseDto>> GetAllClassesAsync();
        Task<List<ClassResponseDto>> GetClassesBySchoolAsync(int schoolId);
        Task<ClassResponseDto> CreateClassAsync(ClassCreateDto dto);
        Task<ClassResponseDto> UpdateClassAsync(int id, ClassCreateDto dto);
        Task<bool> DeleteClassAsync(int id);
    }
}
