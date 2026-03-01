using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Features.Student.Attendance;

public class GetMyAttendanceHandler : IQueryHandler<GetMyAttendanceQuery, List<AttendanceResponseDto>>
{
    private readonly IAttendanceService _s;
    public GetMyAttendanceHandler(IAttendanceService s) => _s = s;
    public Task<List<AttendanceResponseDto>> Handle(GetMyAttendanceQuery r, CancellationToken ct) => _s.GetStudentAttendanceAsync(r.StudentId);
}

public class GetMyAttendanceReportHandler : IQueryHandler<GetMyAttendanceReportQuery, StudentAttendanceReportDto>
{
    private readonly IAttendanceService _s;
    public GetMyAttendanceReportHandler(IAttendanceService s) => _s = s;
    public Task<StudentAttendanceReportDto> Handle(GetMyAttendanceReportQuery r, CancellationToken ct) => _s.GetStudentAttendanceReportAsync(r.StudentId, r.StartDate, r.EndDate);
}
