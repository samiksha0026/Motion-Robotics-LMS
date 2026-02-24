using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.Models;

namespace MotionRobotics.LMS.API.Repositories.Admin
{
    public class SchoolRepository : ISchoolRepository
    {
        private readonly ApplicationDbContext _context;

        public SchoolRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<School?> GetByIdAsync(int id)
        {
            return await _context.Schools
                .Include(s => s.Classes)
                .Include(s => s.Students)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<School>> GetAllAsync()
        {
            return await _context.Schools
                .Include(s => s.Classes)
                .Include(s => s.Students)
                .OrderBy(s => s.SchoolName)
                .ToListAsync();
        }

        public async Task<School> CreateAsync(School school)
        {
            _context.Schools.Add(school);
            await _context.SaveChangesAsync();
            return school;
        }

        public async Task<School> UpdateAsync(School school)
        {
            school.UpdatedAt = DateTime.UtcNow;
            _context.Schools.Update(school);
            await _context.SaveChangesAsync();
            return school;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var school = await _context.Schools
                .Include(s => s.Classes)
                .Include(s => s.Students)
                .Include(s => s.Teachers)
                .Include(s => s.SchoolLevelMappings)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (school == null)
                return false;

            // Delete in proper order to avoid FK constraint violations

            // 1. Delete SchoolLevelMappings
            if (school.SchoolLevelMappings.Any())
            {
                _context.SchoolLevelMappings.RemoveRange(school.SchoolLevelMappings);
            }

            // 2. Delete StudentProgress for all students in this school
            var studentIds = school.Students.Select(s => s.Id).ToList();
            if (studentIds.Any())
            {
                var studentProgress = await _context.StudentProgress
                    .Where(sp => studentIds.Contains(sp.StudentId))
                    .ToListAsync();
                _context.StudentProgress.RemoveRange(studentProgress);

                // Delete ExamResults
                var examResults = await _context.ExamResults
                    .Where(er => studentIds.Contains(er.StudentId))
                    .ToListAsync();
                _context.ExamResults.RemoveRange(examResults);

                // Delete Certificates
                var certificates = await _context.Certificates
                    .Where(c => studentIds.Contains(c.StudentId))
                    .ToListAsync();
                _context.Certificates.RemoveRange(certificates);

                // Delete Attendances
                var attendances = await _context.Attendances
                    .Where(a => studentIds.Contains(a.StudentId))
                    .ToListAsync();
                _context.Attendances.RemoveRange(attendances);
            }

            // 3. Delete Students
            if (school.Students.Any())
            {
                _context.Students.RemoveRange(school.Students);
            }

            // 4. Delete TeacherClasses
            var teacherIds = school.Teachers.Select(t => t.Id).ToList();
            if (teacherIds.Any())
            {
                var teacherClasses = await _context.TeacherClasses
                    .Where(tc => teacherIds.Contains(tc.TeacherId))
                    .ToListAsync();
                _context.TeacherClasses.RemoveRange(teacherClasses);
            }

            // 5. Delete Teachers
            if (school.Teachers.Any())
            {
                _context.Teachers.RemoveRange(school.Teachers);
            }

            // 6. Delete Classes
            if (school.Classes.Any())
            {
                _context.Classes.RemoveRange(school.Classes);
            }

            // 7. Delete the School
            _context.Schools.Remove(school);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsByNameAsync(string schoolName)
        {
            return await _context.Schools
                .AnyAsync(s => s.SchoolName.ToLower() == schoolName.ToLower());
        }

        public async Task<bool> ExistsByCodeAsync(string schoolCode)
        {
            return await _context.Schools
                .AnyAsync(s => s.SchoolCode.ToLower() == schoolCode.ToLower());
        }
    }
}
