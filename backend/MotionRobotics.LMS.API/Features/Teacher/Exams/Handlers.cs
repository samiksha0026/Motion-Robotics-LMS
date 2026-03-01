using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Teacher;
using MotionRobotics.LMS.API.Services;

namespace MotionRobotics.LMS.API.Features.Teacher.Exams;

public class GetTeacherExamResultsHandler : IQueryHandler<GetTeacherExamResultsQuery, TeacherExamResultsListDto>
{
    private readonly IExamService _s;
    public GetTeacherExamResultsHandler(IExamService s) => _s = s;
    public async Task<TeacherExamResultsListDto> Handle(GetTeacherExamResultsQuery r, CancellationToken ct)
    {
        var teacherId = await _s.GetTeacherIdByUserIdAsync(r.UserId) ?? throw new KeyNotFoundException("Teacher not found");
        return await _s.GetTeacherExamResultsAsync(teacherId, r.ClassId, r.ExamId);
    }
}

public class GetTeacherExamOverviewHandler : IQueryHandler<GetTeacherExamOverviewQuery, TeacherExamOverviewDto?>
{
    private readonly IExamService _s;
    public GetTeacherExamOverviewHandler(IExamService s) => _s = s;
    public async Task<TeacherExamOverviewDto?> Handle(GetTeacherExamOverviewQuery r, CancellationToken ct)
    {
        var teacherId = await _s.GetTeacherIdByUserIdAsync(r.UserId) ?? throw new KeyNotFoundException("Teacher not found");
        return await _s.GetTeacherExamOverviewAsync(teacherId, r.ExamId);
    }
}

public class GetStudentsExamEligibilityHandler : IQueryHandler<GetStudentsExamEligibilityQuery, List<StudentExamEligibilityDto>>
{
    private readonly IExamService _s;
    public GetStudentsExamEligibilityHandler(IExamService s) => _s = s;
    public async Task<List<StudentExamEligibilityDto>> Handle(GetStudentsExamEligibilityQuery r, CancellationToken ct)
    {
        var teacherId = await _s.GetTeacherIdByUserIdAsync(r.UserId) ?? throw new KeyNotFoundException("Teacher not found");
        return await _s.GetStudentsExamEligibilityAsync(teacherId, r.ClassId, r.RoboticsLevelId);
    }
}
