using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Lab;

[ApiController]
[Route("api/admin/lab")]
[Authorize(Roles = "SuperAdmin,SchoolAdmin")]
[Tags("Admin - Lab")]
public class LabController : ControllerBase
{
    private readonly IMediator _mediator;
    public LabController(IMediator mediator) => _mediator = mediator;

    // ── GET /api/admin/lab/schools  (SuperAdmin only) ──────────────────────
    [HttpGet("schools")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAllSchoolsLabSummary()
        => Ok(await _mediator.Send(new GetAllSchoolsLabSummaryQuery()));

    // ── GET /api/admin/lab/{schoolId} ────────────────────────────────────
    [HttpGet("{schoolId:int}")]
    public async Task<IActionResult> GetLabInfo(int schoolId)
    {
        if (!CanAccessSchool(schoolId, out var forbidden)) return forbidden!;

        var result = await _mediator.Send(new GetLabInfoQuery(schoolId));
        // Return empty shell if school exists but has no lab info yet
        return result == null
            ? NotFound(new { message = "School not found." })
            : Ok(result);
    }

    // ── PUT /api/admin/lab/{schoolId} ────────────────────────────────────
    [HttpPut("{schoolId:int}")]
    public async Task<IActionResult> UpdateLabInfo(int schoolId, [FromBody] UpdateLabInfoDto dto)
    {
        if (!CanAccessSchool(schoolId, out var forbidden)) return forbidden!;
        try
        {
            var result = await _mediator.Send(new UpdateLabInfoCommand(schoolId, dto));
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    // ── POST /api/admin/lab/{schoolId}/photos ────────────────────────────
    [HttpPost("{schoolId:int}/photos")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPhoto(int schoolId, [FromForm] IFormFile file, [FromForm] string? caption)
    {
        if (!CanAccessSchool(schoolId, out var forbidden)) return forbidden!;
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Please attach an image file." });
        try
        {
            var result = await _mediator.Send(new UploadLabPhotoCommand(schoolId, file, caption));
            return Ok(result);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    // ── DELETE /api/admin/lab/{schoolId}/photos/{photoId} ────────────────
    [HttpDelete("{schoolId:int}/photos/{photoId:int}")]
    public async Task<IActionResult> DeletePhoto(int schoolId, int photoId)
    {
        if (!CanAccessSchool(schoolId, out var forbidden)) return forbidden!;
        var deleted = await _mediator.Send(new DeleteLabPhotoCommand(photoId, schoolId));
        return deleted ? Ok(new { message = "Photo deleted." }) : NotFound(new { message = "Photo not found." });
    }

    // ── Helper: enforce school-scope for SchoolAdmin ──────────────────────
    private bool CanAccessSchool(int schoolId, out IActionResult? error)
    {
        var role = HttpContext.Items["SessionRole"] as string;
        var sessionSchoolId = HttpContext.Items["SessionSchoolId"] as int?;

        if (role == "SchoolAdmin" && sessionSchoolId.HasValue && sessionSchoolId.Value != schoolId)
        {
            error = Forbid();
            return false;
        }
        error = null;
        return true;
    }
}
