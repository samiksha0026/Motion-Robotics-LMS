using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.LevelMappings;

[ApiController]
[Route("api/admin/level-mappings")]
[Authorize(Roles = "SuperAdmin")]
[Tags("Admin - Level Mappings")]
public class LevelMappingsController : ControllerBase
{
    private readonly IMediator _mediator;
    public LevelMappingsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAllMappings([FromQuery] int? schoolId, [FromQuery] int? academicYearId)
        => Ok(await _mediator.Send(new GetAllMappingsQuery(schoolId, academicYearId)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetMappingById(int id)
    {
        var result = await _mediator.Send(new GetMappingByIdQuery(id));
        return result == null ? NotFound(new { message = "Mapping not found" }) : Ok(result);
    }

    [HttpGet("school/{schoolId:int}/year/{academicYearId:int}")]
    public async Task<IActionResult> GetSchoolLevelAssignments(int schoolId, int academicYearId)
    {
        var result = await _mediator.Send(new GetSchoolLevelAssignmentsQuery(schoolId, academicYearId));
        return result == null ? NotFound(new { message = "School or academic year not found" }) : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMapping([FromBody] LevelMappingCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _mediator.Send(new CreateMappingCommand(dto));
            return CreatedAtAction(nameof(GetMappingById), new { id = result.Id }, result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> CreateBulkMappings([FromBody] BulkLevelMappingDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _mediator.Send(new CreateBulkMappingsCommand(dto));
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateMapping(int id, [FromBody] LevelMappingUpdateDto dto)
    {
        try
        {
            var result = await _mediator.Send(new UpdateMappingCommand(id, dto));
            return Ok(result);
        }
        catch (KeyNotFoundException) { return NotFound(new { message = "Mapping not found" }); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteMapping(int id)
    {
        var result = await _mediator.Send(new DeleteMappingCommand(id));
        return result ? Ok(new { message = "Mapping deleted" }) : NotFound(new { message = "Mapping not found" });
    }

    [HttpDelete("school/{schoolId:int}/year/{academicYearId:int}")]
    public async Task<IActionResult> DeleteSchoolMappings(int schoolId, int academicYearId)
    {
        var result = await _mediator.Send(new DeleteSchoolMappingsCommand(schoolId, academicYearId));
        return result ? Ok(new { message = "School mappings deleted" }) : NotFound(new { message = "No mappings found" });
    }
}
