using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.AcademicYears;

[ApiController]
[Route("api/admin/academic-years")]
[Authorize(Roles = "SuperAdmin")]
[Tags("Admin - Academic Years")]
public class AcademicYearsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AcademicYearsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,SchoolAdmin")]
    public async Task<IActionResult> GetAllAcademicYears()
        => Ok(await _mediator.Send(new GetAllAcademicYearsQuery()));

    [HttpGet("current")]
    [Authorize(Roles = "SuperAdmin,SchoolAdmin,Teacher")]
    public async Task<IActionResult> GetCurrentAcademicYear()
    {
        var year = await _mediator.Send(new GetCurrentAcademicYearQuery());
        return year == null ? NotFound(new { message = "No current academic year set" }) : Ok(year);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "SuperAdmin,SchoolAdmin")]
    public async Task<IActionResult> GetAcademicYear(int id)
    {
        var year = await _mediator.Send(new GetAcademicYearByIdQuery(id));
        return year == null ? NotFound(new { message = "Academic year not found" }) : Ok(year);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAcademicYear([FromBody] AcademicYearCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _mediator.Send(new CreateAcademicYearCommand(dto));
            return CreatedAtAction(nameof(GetAcademicYear), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAcademicYear(int id, [FromBody] AcademicYearCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _mediator.Send(new UpdateAcademicYearCommand(id, dto));
            return Ok(result);
        }
        catch (KeyNotFoundException) { return NotFound(new { message = "Academic year not found" }); }
    }

    [HttpPatch("{id:int}/set-current")]
    public async Task<IActionResult> SetCurrentAcademicYear(int id)
    {
        var result = await _mediator.Send(new SetCurrentAcademicYearCommand(id));
        return result ? Ok(new { message = "Academic year set as current" }) : NotFound(new { message = "Academic year not found" });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAcademicYear(int id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteAcademicYearCommand(id));
            return result ? Ok(new { message = "Academic year deleted" }) : NotFound(new { message = "Academic year not found" });
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
