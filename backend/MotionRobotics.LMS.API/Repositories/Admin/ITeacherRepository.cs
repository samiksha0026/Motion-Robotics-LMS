using MotionRobotics.LMS.API.Models;

namespace MotionRobotics.LMS.API.Repositories.Admin
{
    public interface ITeacherRepository
    {
        Task<Teacher?> GetByIdAsync(int id);
        Task<List<Teacher>> GetBySchoolIdAsync(int schoolId);
        Task<List<Teacher>> GetAllAsync();
        Task<Teacher> CreateAsync(Teacher teacher);
        Task<Teacher> UpdateAsync(Teacher teacher);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsByEmailAsync(string email);
        Task AssignClassToTeacherAsync(int teacherId, int classId);
        Task RemoveClassFromTeacherAsync(int teacherId, int classId);
    }
}
