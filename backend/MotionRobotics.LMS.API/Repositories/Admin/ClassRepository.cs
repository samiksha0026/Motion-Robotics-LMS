using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.Models;

namespace MotionRobotics.LMS.API.Repositories.Admin
{
    public class ClassRepository : IClassRepository
    {
        private readonly ApplicationDbContext _context;

        public ClassRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Class?> GetByIdAsync(int id)
        {
            return await _context.Classes
                .Include(c => c.School)
                .Include(c => c.Students)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Class>> GetBySchoolIdAsync(int schoolId)
        {
            return await _context.Classes
                .Where(c => c.SchoolId == schoolId)
                .Include(c => c.School)
                .Include(c => c.Students)
                .OrderBy(c => c.ClassName)
                .ToListAsync();
        }

        public async Task<List<Class>> GetAllAsync()
        {
            return await _context.Classes
                .Include(c => c.School)
                .Include(c => c.Students)
                .OrderBy(c => c.ClassName)
                .ToListAsync();
        }

        public async Task<Class> CreateAsync(Class @class)
        {
            _context.Classes.Add(@class);
            await _context.SaveChangesAsync();
            return @class;
        }

        public async Task<Class> UpdateAsync(Class @class)
        {
            @class.UpdatedAt = DateTime.UtcNow;
            _context.Classes.Update(@class);
            await _context.SaveChangesAsync();
            return @class;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var @class = await _context.Classes.FindAsync(id);
            if (@class == null)
                return false;

            _context.Classes.Remove(@class);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsByNameAndSchoolAsync(string className, int schoolId)
        {
            return await _context.Classes
                .AnyAsync(c => c.ClassName.ToLower() == className.ToLower() && c.SchoolId == schoolId);
        }
    }
}
