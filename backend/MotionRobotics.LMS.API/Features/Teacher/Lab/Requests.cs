using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Teacher.Lab;

public record GetTeacherLabQuery(string UserId) : IQuery<LabInfoDto?>;
