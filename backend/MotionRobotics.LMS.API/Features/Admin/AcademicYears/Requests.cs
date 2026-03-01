using MediatR;
using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.AcademicYears;

public record GetAllAcademicYearsQuery : IQuery<List<AcademicYearDto>>;
public record GetCurrentAcademicYearQuery : IQuery<AcademicYearDto?>;
public record GetAcademicYearByIdQuery(int Id) : IQuery<AcademicYearDto?>;
public record CreateAcademicYearCommand(AcademicYearCreateDto Data) : ICommand<AcademicYearDto>;
public record UpdateAcademicYearCommand(int Id, AcademicYearCreateDto Data) : ICommand<AcademicYearDto>;
public record SetCurrentAcademicYearCommand(int Id) : ICommand<bool>;
public record DeleteAcademicYearCommand(int Id) : ICommand<bool>;
