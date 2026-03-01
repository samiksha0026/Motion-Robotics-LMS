using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Progress;

public record GetProgressStatisticsQuery(int? SchoolId) : IQuery<ProgressStatisticsDto>;
public record GetLevelWiseStatisticsQuery(int? SchoolId) : IQuery<List<LevelProgressStatsDto>>;
public record GetSchoolProgressQuery(int SchoolId, int? ClassId, int? RoboticsLevelId) : IQuery<List<StudentProgressOverviewDto>>;
public record GetStudentProgressQuery(int StudentId) : IQuery<StudentProgressOverviewDto?>;
