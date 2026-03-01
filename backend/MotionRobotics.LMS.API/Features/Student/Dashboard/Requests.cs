using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Student;

namespace MotionRobotics.LMS.API.Features.Student.Dashboard;

public record GetStudentDashboardQuery(string UserId) : IQuery<StudentDashboardDto?>;
public record GetStudentProfileQuery(string UserId) : IQuery<StudentProfileDto?>;
