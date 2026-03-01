using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Teacher.Progress;

public record GetTeacherStudentsProgressQuery(string UserId, int? ClassId) : IQuery<List<StudentProgressOverviewDto>>;
public record GetTeacherStudentProgressQuery(string UserId, int StudentId) : IQuery<StudentProgressOverviewDto?>;
