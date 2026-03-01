using MediatR;
using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Teachers;

public record GetAllTeachersQuery : IQuery<List<TeacherResponseDto>>;
public record GetTeacherByIdQuery(int Id) : IQuery<TeacherResponseDto?>;
public record GetTeachersBySchoolQuery(int SchoolId) : IQuery<List<TeacherResponseDto>>;
public record CreateTeacherCommand(TeacherCreateDto Data) : ICommand<TeacherResponseDto>;
public record UpdateTeacherCommand(int Id, TeacherCreateDto Data) : ICommand<TeacherResponseDto>;
public record DeleteTeacherCommand(int Id) : ICommand<bool>;
public record AssignClassToTeacherCommand(int TeacherId, int ClassId) : ICommand;
public record RemoveClassFromTeacherCommand(int TeacherId, int ClassId) : ICommand;
