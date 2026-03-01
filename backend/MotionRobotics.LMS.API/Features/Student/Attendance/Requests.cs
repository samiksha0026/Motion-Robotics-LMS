using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Student.Attendance;

public record GetMyAttendanceQuery(int StudentId) : IQuery<List<AttendanceResponseDto>>;
public record GetMyAttendanceReportQuery(int StudentId, DateTime? StartDate, DateTime? EndDate) : IQuery<StudentAttendanceReportDto>;
