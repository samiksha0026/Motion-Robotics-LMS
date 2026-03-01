using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Student;
using MotionRobotics.LMS.API.Services;

namespace MotionRobotics.LMS.API.Features.Student.Dashboard;

public class GetStudentDashboardHandler : IQueryHandler<GetStudentDashboardQuery, StudentDashboardDto?>
{
    private readonly IStudentService _s;
    public GetStudentDashboardHandler(IStudentService s) => _s = s;
    public async Task<StudentDashboardDto?> Handle(GetStudentDashboardQuery r, CancellationToken ct)
    {
        var studentId = await _s.GetStudentIdByUserIdAsync(r.UserId);
        return studentId.HasValue ? await _s.GetDashboardAsync(studentId.Value) : null;
    }
}

public class GetStudentProfileHandler : IQueryHandler<GetStudentProfileQuery, StudentProfileDto?>
{
    private readonly IStudentService _s;
    public GetStudentProfileHandler(IStudentService s) => _s = s;
    public async Task<StudentProfileDto?> Handle(GetStudentProfileQuery r, CancellationToken ct)
    {
        var studentId = await _s.GetStudentIdByUserIdAsync(r.UserId);
        return studentId.HasValue ? await _s.GetProfileAsync(studentId.Value) : null;
    }
}
