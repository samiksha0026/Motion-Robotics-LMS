using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Teacher.Attendance;

public record RecordAttendanceCommand(string UserId, AttendanceCreateDto Data) : ICommand<AttendanceResponseDto>;
public record RecordBulkAttendanceCommand(string UserId, BulkAttendanceCreateDto Data) : ICommand<BulkAttendanceResponseDto>;
public record GetAttendanceByIdQuery(int Id) : IQuery<AttendanceResponseDto?>;
public record GetClassAttendanceQuery(int ClassId) : IQuery<List<AttendanceResponseDto>>;
public record GetClassAttendanceByDateQuery(int ClassId, DateTime Date) : IQuery<List<AttendanceResponseDto>>;
public record GetStudentAttendanceQuery(int StudentId) : IQuery<List<AttendanceResponseDto>>;
public record GetStudentAttendanceReportQuery(int StudentId, DateTime? StartDate, DateTime? EndDate) : IQuery<StudentAttendanceReportDto>;
public record UpdateAttendanceCommand(int Id, AttendanceCreateDto Data) : ICommand<AttendanceResponseDto>;
public record DeleteAttendanceCommand(int Id) : ICommand<bool>;
