using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Services.Admin
{
    public interface IAdminStudentService
    {
        Task<StudentResponseDto> CreateStudentAsync(StudentCreateDto dto);
        Task<StudentResponseDto?> GetStudentByIdAsync(int id);
        Task<List<StudentResponseDto>> GetStudentsBySchoolAsync(int schoolId);
        Task<List<StudentResponseDto>> GetStudentsByClassAsync(int classId);
        Task<List<StudentResponseDto>> GetAllStudentsAsync();
        Task<bool> DeleteStudentAsync(int id);
        Task<StudentResponseDto> UpdateStudentAsync(int id, StudentUpdateDto dto);
    }
}
