using MediatR;
using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.RoboticsLevels;

public record GetAllRoboticsLevelsQuery : IQuery<List<RoboticsLevelDto>>;
public record GetRoboticsLevelByIdQuery(int Id) : IQuery<RoboticsLevelDetailDto?>;
public record UpdateSyllabusUrlCommand(int Id, string SyllabusUrl) : ICommand<bool>;
public record SeedSyllabusUrlsCommand : ICommand;
public record SeedSampleExperimentsCommand : ICommand;
