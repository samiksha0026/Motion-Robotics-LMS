using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Schools;

[ApiController]
[Route("api/admin/schools")]
[Authorize(Roles = "SuperAdmin")]
[Tags("Admin - Schools")]
public class SchoolsController : ControllerBase
{
    private readonly IMediator _mediator;
    public SchoolsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAllSchools()
        => Ok(await _mediator.Send(new GetAllSchoolsQuery()));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetSchool(int id)
    {
        var school = await _mediator.Send(new GetSchoolByIdQuery(id));
        return school == null ? NotFound(new { message = "School not found" }) : Ok(school);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSchool([FromBody] SchoolCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var school = await _mediator.Send(new CreateSchoolCommand(dto));
            return CreatedAtAction(nameof(GetSchool), new { id = school.Id }, school);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateSchool(int id, [FromBody] SchoolCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var school = await _mediator.Send(new UpdateSchoolCommand(id, dto));
            return Ok(school);
        }
        catch (KeyNotFoundException) { return NotFound(new { message = "School not found" }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteSchool(int id)
    {
        var result = await _mediator.Send(new DeleteSchoolCommand(id));
        return result ? Ok(new { message = "School deleted successfully" }) : NotFound(new { message = "School not found" });
    }

    [HttpPost("{id:int}/logo")]
    public async Task<IActionResult> UploadLogo(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded" });

        var allowed = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowed.Contains(file.ContentType.ToLower()))
            return BadRequest(new { message = "Only image files are allowed" });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "File size must be less than 5MB" });

        try
        {
            var logoUrl = await _mediator.Send(new UploadSchoolLogoCommand(id, file));
            return Ok(new { logoUrl });
        }
        catch (KeyNotFoundException) { return NotFound(new { message = "School not found" }); }
    }

    [HttpPatch("{id:int}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var result = await _mediator.Send(new ToggleSchoolStatusCommand(id));
        return result ? Ok(new { message = "School status updated" }) : NotFound(new { message = "School not found" });
    }

    [HttpPost("{id:int}/reset-admin-password")]
    public async Task<IActionResult> ResetAdminPassword(int id)
    {
        try
        {
            var tempPassword = await _mediator.Send(new ResetSchoolAdminPasswordCommand(id));
            return Ok(new { message = "Password reset successfully", temporaryPassword = tempPassword });
        }
        catch (KeyNotFoundException) { return NotFound(new { message = "School not found" }); }
    }
}
