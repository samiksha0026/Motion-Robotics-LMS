using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Reports;

public class GetStudentReportHandler : IQueryHandler<GetStudentReportQuery, ComprehensiveStudentReportDto>
{
    private readonly IReportService _s;
    public GetStudentReportHandler(IReportService s) => _s = s;
    public Task<ComprehensiveStudentReportDto> Handle(GetStudentReportQuery r, CancellationToken ct) => _s.GetStudentComprehensiveReportAsync(r.StudentId);
}

public class GetSchoolReportHandler : IQueryHandler<GetSchoolReportQuery, ComprehensiveSchoolReportDto>
{
    private readonly IReportService _s;
    public GetSchoolReportHandler(IReportService s) => _s = s;
    public Task<ComprehensiveSchoolReportDto> Handle(GetSchoolReportQuery r, CancellationToken ct) => _s.GetSchoolComprehensiveReportAsync(r.SchoolId);
}

public class GetPeriodReportHandler : IQueryHandler<GetPeriodReportQuery, PeriodReportDto>
{
    private readonly IReportService _s;
    public GetPeriodReportHandler(IReportService s) => _s = s;
    public Task<PeriodReportDto> Handle(GetPeriodReportQuery r, CancellationToken ct) => _s.GetPeriodReportAsync(r.StartDate, r.EndDate, r.SchoolId);
}

public class GetTopPerformersHandler : IQueryHandler<GetTopPerformersQuery, List<TopPerformerDto>>
{
    private readonly IReportService _s;
    public GetTopPerformersHandler(IReportService s) => _s = s;
    public Task<List<TopPerformerDto>> Handle(GetTopPerformersQuery r, CancellationToken ct) => _s.GetTopPerformersAsync(r.SchoolId, r.Limit);
}
