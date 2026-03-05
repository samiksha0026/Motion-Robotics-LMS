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

// ── Excel Import ─────────────────────────────────────────────────────────────
/// <summary>Bulk-create students from an uploaded Excel file.</summary>
public record ImportStudentsCommand(IFormFile File, int SchoolId) : ICommand<StudentImportResultDto>;

/// <summary>Generate and return an Excel template file as raw bytes.</summary>
public record DownloadStudentTemplateQuery : IQuery<byte[]>;
