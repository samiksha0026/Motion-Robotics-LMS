using MediatR;
using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Attendance;

public record GetAllAttendanceQuery(int? SchoolId, int? ClassId, DateTime? StartDate, DateTime? EndDate) : IQuery<List<AttendanceResponseDto>>;
public record GetAttendanceByIdQuery(int Id) : IQuery<AttendanceResponseDto?>;
public record GetAttendanceSummaryQuery(int? SchoolId, int? ClassId, DateTime? StartDate, DateTime? EndDate) : IQuery<AttendanceSummaryDto>;
public record GetStudentAttendanceReportQuery(int StudentId, DateTime? StartDate, DateTime? EndDate) : IQuery<StudentAttendanceReportDto>;
public record GetClassAttendanceReportQuery(int ClassId, DateTime? StartDate, DateTime? EndDate) : IQuery<ClassAttendanceReportDto>;
public record GetSchoolAttendanceReportQuery(int SchoolId, DateTime? StartDate, DateTime? EndDate) : IQuery<SchoolAttendanceReportDto>;
public record DeleteAttendanceCommand(int Id) : ICommand<bool>;
