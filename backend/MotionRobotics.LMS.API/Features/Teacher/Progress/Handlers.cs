using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services;

namespace MotionRobotics.LMS.API.Features.Teacher.Progress;

public class GetTeacherStudentsProgressHandler : IQueryHandler<GetTeacherStudentsProgressQuery, List<StudentProgressOverviewDto>>
{
    private readonly ICertificateService _s;
    public GetTeacherStudentsProgressHandler(ICertificateService s) => _s = s;
    public async Task<List<StudentProgressOverviewDto>> Handle(GetTeacherStudentsProgressQuery r, CancellationToken ct)
    {
        var teacherId = await _s.GetTeacherIdByUserIdAsync(r.UserId) ?? throw new KeyNotFoundException("Teacher not found");
        return await _s.GetTeacherStudentsProgressAsync(teacherId, r.ClassId);
    }
}

public class GetTeacherStudentProgressHandler : IQueryHandler<GetTeacherStudentProgressQuery, StudentProgressOverviewDto?>
{
    private readonly ICertificateService _s;
    public GetTeacherStudentProgressHandler(ICertificateService s) => _s = s;
    public async Task<StudentProgressOverviewDto?> Handle(GetTeacherStudentProgressQuery r, CancellationToken ct)
    {
        var teacherId = await _s.GetTeacherIdByUserIdAsync(r.UserId) ?? throw new KeyNotFoundException("Teacher not found");
        return await _s.GetStudentProgressAsync(r.StudentId);
    }
}
