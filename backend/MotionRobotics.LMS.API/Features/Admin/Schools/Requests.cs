using MediatR;
using Microsoft.AspNetCore.Http;
using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Schools;

// ── Queries ───────────────────────────────────────────────────────────────────
public record GetAllSchoolsQuery : IQuery<List<SchoolResponseDto>>;
public record GetSchoolByIdQuery(int Id) : IQuery<SchoolResponseDto?>;

// ── Commands ──────────────────────────────────────────────────────────────────
public record CreateSchoolCommand(SchoolCreateDto Data) : ICommand<SchoolResponseDto>;
public record UpdateSchoolCommand(int Id, SchoolCreateDto Data) : ICommand<SchoolResponseDto>;
public record DeleteSchoolCommand(int Id) : ICommand<bool>;
public record UploadSchoolLogoCommand(int Id, IFormFile File) : ICommand<string>;
public record ToggleSchoolStatusCommand(int Id) : ICommand<bool>;
public record ResetSchoolAdminPasswordCommand(int Id) : ICommand<string>;
