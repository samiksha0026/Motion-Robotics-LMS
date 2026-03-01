using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Teacher;
using MotionRobotics.LMS.API.Services;

namespace MotionRobotics.LMS.API.Features.Teacher.Dashboard;

public class GetTeacherDashboardHandler : IQueryHandler<GetTeacherDashboardQuery, TeacherDashboardDto?>
{
    private readonly ITeacherService _s;
    public GetTeacherDashboardHandler(ITeacherService s) => _s = s;
    public async Task<TeacherDashboardDto?> Handle(GetTeacherDashboardQuery r, CancellationToken ct)
    {
        var teacherId = await _s.GetTeacherIdByUserIdAsync(r.UserId);
        return teacherId.HasValue ? await _s.GetDashboardAsync(teacherId.Value) : null;
    }
}

public class GetAssignedClassesHandler : IQueryHandler<GetAssignedClassesQuery, List<AssignedClassDto>>
{
    private readonly ITeacherService _s;
    public GetAssignedClassesHandler(ITeacherService s) => _s = s;
    public async Task<List<AssignedClassDto>> Handle(GetAssignedClassesQuery r, CancellationToken ct)
    {
        var teacherId = await _s.GetTeacherIdByUserIdAsync(r.UserId);
        return teacherId.HasValue ? await _s.GetAssignedClassesAsync(teacherId.Value) : new List<AssignedClassDto>();
    }
}
