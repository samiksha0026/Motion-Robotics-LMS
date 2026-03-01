using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services;

namespace MotionRobotics.LMS.API.Features.Admin.Progress;

public class GetProgressStatisticsHandler : IQueryHandler<GetProgressStatisticsQuery, ProgressStatisticsDto>
{
    private readonly ICertificateService _s;
    public GetProgressStatisticsHandler(ICertificateService s) => _s = s;
    public Task<ProgressStatisticsDto> Handle(GetProgressStatisticsQuery r, CancellationToken ct) => _s.GetProgressStatisticsAsync(r.SchoolId);
}

public class GetLevelWiseStatisticsHandler : IQueryHandler<GetLevelWiseStatisticsQuery, List<LevelProgressStatsDto>>
{
    private readonly ICertificateService _s;
    public GetLevelWiseStatisticsHandler(ICertificateService s) => _s = s;
    public Task<List<LevelProgressStatsDto>> Handle(GetLevelWiseStatisticsQuery r, CancellationToken ct) => _s.GetLevelWiseStatisticsAsync(r.SchoolId);
}

public class GetSchoolProgressHandler : IQueryHandler<GetSchoolProgressQuery, List<StudentProgressOverviewDto>>
{
    private readonly ICertificateService _s;
    public GetSchoolProgressHandler(ICertificateService s) => _s = s;
    public Task<List<StudentProgressOverviewDto>> Handle(GetSchoolProgressQuery r, CancellationToken ct) => _s.GetSchoolProgressAsync(r.SchoolId, r.ClassId, r.RoboticsLevelId);
}

public class GetStudentProgressHandler : IQueryHandler<GetStudentProgressQuery, StudentProgressOverviewDto?>
{
    private readonly ICertificateService _s;
    public GetStudentProgressHandler(ICertificateService s) => _s = s;
    public Task<StudentProgressOverviewDto?> Handle(GetStudentProgressQuery r, CancellationToken ct) => _s.GetStudentProgressAsync(r.StudentId);
}
