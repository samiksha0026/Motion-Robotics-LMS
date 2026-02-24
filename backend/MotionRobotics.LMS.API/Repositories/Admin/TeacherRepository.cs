using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.Models;

namespace MotionRobotics.LMS.API.Repositories.Admin
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly ApplicationDbContext _context;

        public TeacherRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Teacher?> GetByIdAsync(int id)
        {
            return await _context.Teachers
                .Include(t => t.School)
                .Include(t => t.TeacherClasses)
                .ThenInclude(tc => tc.Class)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<Teacher>> GetBySchoolIdAsync(int schoolId)
        {
            return await _context.Teachers
                .Where(t => t.SchoolId == schoolId)
                .Include(t => t.School)
                .Include(t => t.TeacherClasses)
                .ThenInclude(tc => tc.Class)
                .OrderBy(t => t.FullName)
                .ToListAsync();
        }

        public async Task<List<Teacher>> GetAllAsync()
        {
            return await _context.Teachers
                .Include(t => t.School)
                .Include(t => t.TeacherClasses)
                .ThenInclude(tc => tc.Class)
                .OrderBy(t => t.FullName)
                .ToListAsync();
        }

        public async Task<Teacher> CreateAsync(Teacher teacher)
        {
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();
            return teacher;
        }

        public async Task<Teacher> UpdateAsync(Teacher teacher)
        {
            teacher.UpdatedAt = DateTime.UtcNow;
            _context.Teachers.Update(teacher);
            await _context.SaveChangesAsync();
            return teacher;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.TeacherClasses)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (teacher == null)
                return false;

            // Remove all TeacherClass associations first
            if (teacher.TeacherClasses != null && teacher.TeacherClasses.Any())
            {
                _context.TeacherClasses.RemoveRange(teacher.TeacherClasses);
            }

            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Teachers
                .AnyAsync(t => t.Email.ToLower() == email.ToLower());
        }

        public async Task AssignClassToTeacherAsync(int teacherId, int classId)
        {
            var existing = await _context.TeacherClasses
                .FirstOrDefaultAsync(tc => tc.TeacherId == teacherId && tc.ClassId == classId);

            if (existing == null)
            {
                var teacherClass = new TeacherClass
                {
                    TeacherId = teacherId,
                    ClassId = classId
                };
                _context.TeacherClasses.Add(teacherClass);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveClassFromTeacherAsync(int teacherId, int classId)
        {
            var teacherClass = await _context.TeacherClasses
                .FirstOrDefaultAsync(tc => tc.TeacherId == teacherId && tc.ClassId == classId);

            if (teacherClass != null)
            {
                _context.TeacherClasses.Remove(teacherClass);
                await _context.SaveChangesAsync();
            }
        }
    }
}
