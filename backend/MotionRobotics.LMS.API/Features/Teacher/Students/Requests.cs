using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Teacher;

namespace MotionRobotics.LMS.API.Features.Teacher.Students;

public record GetStudentProgressQuery(string UserId, int StudentId) : IQuery<StudentDetailedProgressDto?>;
