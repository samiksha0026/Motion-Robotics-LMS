using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Reports;

public record GetStudentReportQuery(int StudentId) : IQuery<ComprehensiveStudentReportDto>;
public record GetSchoolReportQuery(int SchoolId) : IQuery<ComprehensiveSchoolReportDto>;
public record GetPeriodReportQuery(DateTime StartDate, DateTime EndDate, int? SchoolId) : IQuery<PeriodReportDto>;
public record GetTopPerformersQuery(int? SchoolId, int Limit) : IQuery<List<TopPerformerDto>>;
