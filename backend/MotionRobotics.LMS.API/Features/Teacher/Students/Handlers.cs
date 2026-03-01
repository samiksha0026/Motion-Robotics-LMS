using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Teacher;
using MotionRobotics.LMS.API.Services;

namespace MotionRobotics.LMS.API.Features.Teacher.Students;

public class GetStudentProgressHandler : IQueryHandler<GetStudentProgressQuery, StudentDetailedProgressDto?>
{
    private readonly ITeacherService _s;
    public GetStudentProgressHandler(ITeacherService s) => _s = s;
    public async Task<StudentDetailedProgressDto?> Handle(GetStudentProgressQuery r, CancellationToken ct)
    {
        var teacherId = await _s.GetTeacherIdByUserIdAsync(r.UserId);
        return teacherId.HasValue ? await _s.GetStudentProgressAsync(teacherId.Value, r.StudentId) : null;
    }
}
