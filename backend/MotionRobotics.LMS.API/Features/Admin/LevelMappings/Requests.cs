using MediatR;
using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.LevelMappings;

public record GetAllMappingsQuery(int? SchoolId, int? AcademicYearId) : IQuery<List<LevelMappingDto>>;
public record GetMappingByIdQuery(int Id) : IQuery<LevelMappingDto?>;
public record GetSchoolLevelAssignmentsQuery(int SchoolId, int AcademicYearId) : IQuery<SchoolLevelAssignmentsDto?>;
public record CreateMappingCommand(LevelMappingCreateDto Data) : ICommand<LevelMappingDto>;
public record CreateBulkMappingsCommand(BulkLevelMappingDto Data) : ICommand<List<LevelMappingDto>>;
public record UpdateMappingCommand(int Id, LevelMappingUpdateDto Data) : ICommand<LevelMappingDto>;
public record DeleteMappingCommand(int Id) : ICommand<bool>;
public record DeleteSchoolMappingsCommand(int SchoolId, int AcademicYearId) : ICommand<bool>;
