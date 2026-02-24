using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Services.Admin
{
    public interface IAttendanceService
    {
        Task<AttendanceResponseDto?> GetAttendanceByIdAsync(int id);
        Task<List<AttendanceResponseDto>> GetStudentAttendanceAsync(int studentId);
        Task<List<AttendanceResponseDto>> GetClassAttendanceAsync(int classId);
        Task<List<AttendanceResponseDto>> GetAttendanceByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<AttendanceResponseDto>> GetClassAttendanceByDateAsync(int classId, DateTime date);
        Task<AttendanceResponseDto> RecordAttendanceAsync(int teacherId, AttendanceCreateDto dto);
        Task<AttendanceResponseDto> UpdateAttendanceAsync(int id, AttendanceCreateDto dto);
        Task<bool> DeleteAttendanceAsync(int id);

        // New methods for bulk operations and reporting
        Task<BulkAttendanceResponseDto> RecordBulkAttendanceAsync(int teacherId, BulkAttendanceCreateDto dto);
        Task<List<AttendanceResponseDto>> GetAllAttendanceAsync(int? schoolId = null, int? classId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<AttendanceSummaryDto> GetAttendanceSummaryAsync(int? schoolId = null, int? classId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<StudentAttendanceReportDto> GetStudentAttendanceReportAsync(int studentId, DateTime? startDate = null, DateTime? endDate = null);
        Task<ClassAttendanceReportDto> GetClassAttendanceReportAsync(int classId, DateTime? startDate = null, DateTime? endDate = null);
        Task<SchoolAttendanceReportDto> GetSchoolAttendanceReportAsync(int schoolId, DateTime? startDate = null, DateTime? endDate = null);
    }
}
