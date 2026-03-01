using MediatR;
using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Students;

public record GetAllStudentsQuery : IQuery<List<StudentResponseDto>>;
public record GetStudentByIdQuery(int Id) : IQuery<StudentResponseDto?>;
public record GetStudentsBySchoolQuery(int SchoolId) : IQuery<List<StudentResponseDto>>;
public record GetStudentsByClassQuery(int ClassId) : IQuery<List<StudentResponseDto>>;
public record CreateStudentCommand(StudentCreateDto Data) : ICommand<StudentResponseDto>;
public record DeleteStudentCommand(int Id) : ICommand<bool>;
