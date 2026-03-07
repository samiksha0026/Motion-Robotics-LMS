using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Student.Lab;

public record GetStudentLabQuery(string UserId) : IQuery<LabInfoDto?>;
