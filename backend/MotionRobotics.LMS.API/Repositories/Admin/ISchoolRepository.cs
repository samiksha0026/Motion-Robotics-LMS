using MotionRobotics.LMS.API.Models;

namespace MotionRobotics.LMS.API.Repositories.Admin
{
    public interface ISchoolRepository
    {
        Task<School?> GetByIdAsync(int id);
        Task<List<School>> GetAllAsync();
        Task<School> CreateAsync(School school);
        Task<School> UpdateAsync(School school);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsByNameAsync(string schoolName);
        Task<bool> ExistsByCodeAsync(string schoolCode);
    }
}
