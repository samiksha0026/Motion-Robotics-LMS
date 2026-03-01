using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Teacher;

namespace MotionRobotics.LMS.API.Features.Teacher.Dashboard;

public record GetTeacherDashboardQuery(string UserId) : IQuery<TeacherDashboardDto?>;
public record GetAssignedClassesQuery(string UserId) : IQuery<List<AssignedClassDto>>;
