using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.Models;

namespace MotionRobotics.LMS.API.Repositories.Admin
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly ApplicationDbContext _context;

        public AttendanceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Attendance?> GetByIdAsync(int id)
        {
            return await _context.Attendances
                .Include(a => a.Student)
                    .ThenInclude(s => s!.School)
                .Include(a => a.Class)
                .Include(a => a.Teacher)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<Attendance>> GetByStudentAsync(int studentId)
        {
            return await _context.Attendances
                .Where(a => a.StudentId == studentId)
                .Include(a => a.Student)
                .Include(a => a.Class)
                .Include(a => a.Teacher)
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();
        }

        public async Task<List<Attendance>> GetByClassAsync(int classId)
        {
            return await _context.Attendances
                .Where(a => a.ClassId == classId)
                .Include(a => a.Student)
                .Include(a => a.Class)
                .Include(a => a.Teacher)
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();
        }

        public async Task<List<Attendance>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Attendances
                .Where(a => a.AttendanceDate >= startDate && a.AttendanceDate <= endDate)
                .Include(a => a.Student)
                .Include(a => a.Class)
                .Include(a => a.Teacher)
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();
        }

        public async Task<List<Attendance>> GetByClassAndDateAsync(int classId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            return await _context.Attendances
                .Where(a => a.ClassId == classId &&
                            a.AttendanceDate >= startOfDay &&
                            a.AttendanceDate < endOfDay)
                .Include(a => a.Student)
                .Include(a => a.Class)
                .Include(a => a.Teacher)
                .OrderBy(a => a.Student!.FullName)
                .ToListAsync();
        }

        public async Task<Attendance> CreateAsync(Attendance attendance)
        {
            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();
            return attendance;
        }

        public async Task<Attendance> UpdateAsync(Attendance attendance)
        {
            _context.Attendances.Update(attendance);
            await _context.SaveChangesAsync();
            return attendance;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null)
                return false;

            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Attendance>> CreateBulkAsync(List<Attendance> attendances)
        {
            _context.Attendances.AddRange(attendances);
            await _context.SaveChangesAsync();
            return attendances;
        }

        public async Task<List<Attendance>> GetBySchoolAsync(int schoolId)
        {
            return await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Class)
                .Include(a => a.Teacher)
                .Where(a => a.Student!.SchoolId == schoolId)
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();
        }

        public async Task<List<Attendance>> GetBySchoolAndDateRangeAsync(int schoolId, DateTime startDate, DateTime endDate)
        {
            return await _context.Attendances
                .Include(a => a.Student)
                    .ThenInclude(s => s!.School)
                .Include(a => a.Class)
                .Include(a => a.Teacher)
                .Where(a => a.Student!.SchoolId == schoolId &&
                            a.AttendanceDate >= startDate.Date &&
                            a.AttendanceDate < endDate.Date.AddDays(1))
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();
        }

        public async Task<List<Attendance>> GetByStudentAndDateRangeAsync(int studentId, DateTime startDate, DateTime endDate)
        {
            return await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Class)
                .Include(a => a.Teacher)
                .Where(a => a.StudentId == studentId &&
                            a.AttendanceDate >= startDate.Date &&
                            a.AttendanceDate < endDate.Date.AddDays(1))
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();
        }

        public async Task<List<Attendance>> GetByClassAndDateRangeAsync(int classId, DateTime startDate, DateTime endDate)
        {
            return await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Class)
                .Include(a => a.Teacher)
                .Where(a => a.ClassId == classId &&
                            a.AttendanceDate >= startDate.Date &&
                            a.AttendanceDate < endDate.Date.AddDays(1))
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();
        }

        public async Task<List<Attendance>> GetAllAsync(int? schoolId = null, int? classId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Attendances
                .Include(a => a.Student)
                    .ThenInclude(s => s!.School)
                .Include(a => a.Class)
                .Include(a => a.Teacher)
                .AsQueryable();

            if (schoolId.HasValue)
                query = query.Where(a => a.Student!.SchoolId == schoolId.Value);

            if (classId.HasValue)
                query = query.Where(a => a.ClassId == classId.Value);

            if (startDate.HasValue)
                query = query.Where(a => a.AttendanceDate >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(a => a.AttendanceDate < endDate.Value.Date.AddDays(1));

            return await query.OrderByDescending(a => a.AttendanceDate).ToListAsync();
        }

        public async Task<bool> ExistsAsync(int studentId, int classId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            return await _context.Attendances
                .AnyAsync(a => a.StudentId == studentId &&
                              a.ClassId == classId &&
                              a.AttendanceDate >= startOfDay &&
                              a.AttendanceDate < endOfDay);
        }
    }
}
