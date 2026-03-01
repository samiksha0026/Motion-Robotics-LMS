using MediatR;
using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Classes;

public record GetAllClassesQuery : IQuery<List<ClassResponseDto>>;
public record GetClassByIdQuery(int Id) : IQuery<ClassResponseDto?>;
public record GetClassesBySchoolQuery(int SchoolId) : IQuery<List<ClassResponseDto>>;
public record CreateClassCommand(ClassCreateDto Data) : ICommand<ClassResponseDto>;
public record UpdateClassCommand(int Id, ClassCreateDto Data) : ICommand<ClassResponseDto>;
public record DeleteClassCommand(int Id) : ICommand<bool>;
