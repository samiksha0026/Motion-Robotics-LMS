using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Teacher;
using MotionRobotics.LMS.API.Services;

namespace MotionRobotics.LMS.API.Features.Teacher.Classes;

public class GetClassDetailHandler : IQueryHandler<GetClassDetailQuery, ClassDetailDto?>
{
    private readonly ITeacherService _s;
    public GetClassDetailHandler(ITeacherService s) => _s = s;
    public async Task<ClassDetailDto?> Handle(GetClassDetailQuery r, CancellationToken ct)
    {
        var teacherId = await _s.GetTeacherIdByUserIdAsync(r.UserId);
        return teacherId.HasValue ? await _s.GetClassDetailAsync(teacherId.Value, r.ClassId) : null;
    }
}

public class GetClassStudentsHandler : IQueryHandler<GetClassStudentsQuery, List<ClassStudentDto>>
{
    private readonly ITeacherService _s;
    public GetClassStudentsHandler(ITeacherService s) => _s = s;
    public async Task<List<ClassStudentDto>> Handle(GetClassStudentsQuery r, CancellationToken ct)
    {
        var teacherId = await _s.GetTeacherIdByUserIdAsync(r.UserId);
        return teacherId.HasValue ? await _s.GetClassStudentsAsync(teacherId.Value, r.ClassId) : new List<ClassStudentDto>();
    }
}
