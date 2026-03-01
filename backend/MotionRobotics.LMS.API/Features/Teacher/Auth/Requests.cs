using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Teacher.Auth;

public record TeacherLoginCommand(AdminLoginRequestDto Data) : ICommand<object?>;
