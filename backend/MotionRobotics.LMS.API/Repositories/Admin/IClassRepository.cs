using MotionRobotics.LMS.API.Models;

namespace MotionRobotics.LMS.API.Repositories.Admin
{
    public interface IClassRepository
    {
        Task<Class?> GetByIdAsync(int id);
        Task<List<Class>> GetBySchoolIdAsync(int schoolId);
        Task<List<Class>> GetAllAsync();
        Task<Class> CreateAsync(Class @class);
        Task<Class> UpdateAsync(Class @class);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsByNameAndSchoolAsync(string className, int schoolId);
    }
}
