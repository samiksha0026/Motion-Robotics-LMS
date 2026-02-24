using MotionRobotics.LMS.API.Models;

namespace MotionRobotics.LMS.API.Repositories.Admin
{
    public interface IAttendanceRepository
    {
        Task<Attendance?> GetByIdAsync(int id);
        Task<List<Attendance>> GetByStudentAsync(int studentId);
        Task<List<Attendance>> GetByClassAsync(int classId);
        Task<List<Attendance>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<Attendance>> GetByClassAndDateAsync(int classId, DateTime date);
        Task<Attendance> CreateAsync(Attendance attendance);
        Task<Attendance> UpdateAsync(Attendance attendance);
        Task<bool> DeleteAsync(int id);

        // New methods for bulk operations and reporting
        Task<List<Attendance>> CreateBulkAsync(List<Attendance> attendances);
        Task<List<Attendance>> GetBySchoolAsync(int schoolId);
        Task<List<Attendance>> GetBySchoolAndDateRangeAsync(int schoolId, DateTime startDate, DateTime endDate);
        Task<List<Attendance>> GetByStudentAndDateRangeAsync(int studentId, DateTime startDate, DateTime endDate);
        Task<List<Attendance>> GetByClassAndDateRangeAsync(int classId, DateTime startDate, DateTime endDate);
        Task<List<Attendance>> GetAllAsync(int? schoolId = null, int? classId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<bool> ExistsAsync(int studentId, int classId, DateTime date);
    }
}
