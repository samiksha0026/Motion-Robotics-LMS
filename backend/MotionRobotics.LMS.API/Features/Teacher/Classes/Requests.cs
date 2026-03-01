using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Teacher;

namespace MotionRobotics.LMS.API.Features.Teacher.Classes;

public record GetClassDetailQuery(string UserId, int ClassId) : IQuery<ClassDetailDto?>;
public record GetClassStudentsQuery(string UserId, int ClassId) : IQuery<List<ClassStudentDto>>;
