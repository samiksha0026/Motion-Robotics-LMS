using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Services.Admin
{
    public interface IAdminTeacherService
    {
        Task<TeacherResponseDto> CreateTeacherAsync(TeacherCreateDto dto);
        Task<TeacherResponseDto?> GetTeacherByIdAsync(int id);
        Task<List<TeacherResponseDto>> GetTeachersBySchoolAsync(int schoolId);
        Task<List<TeacherResponseDto>> GetAllTeachersAsync();
        Task<TeacherResponseDto> UpdateTeacherAsync(int id, TeacherCreateDto dto);
        Task<bool> DeleteTeacherAsync(int id);
        Task AssignClassToTeacherAsync(int teacherId, int classId);
        Task RemoveClassFromTeacherAsync(int teacherId, int classId);
    }
}
