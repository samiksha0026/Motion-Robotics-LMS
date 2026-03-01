using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Classes;

[ApiController]
[Route("api/admin/classes")]
[Authorize(Roles = "SuperAdmin,SchoolAdmin")]
[Tags("Admin - Classes")]
public class ClassesController : ControllerBase
{
    private readonly IMediator _mediator;
    public ClassesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAllClasses()
        => Ok(await _mediator.Send(new GetAllClassesQuery()));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetClass(int id)
    {
        var result = await _mediator.Send(new GetClassByIdQuery(id));
        return result == null ? NotFound(new { message = "Class not found" }) : Ok(result);
    }

    [HttpGet("school/{schoolId:int}")]
    public async Task<IActionResult> GetClassesBySchool(int schoolId)
        => Ok(await _mediator.Send(new GetClassesBySchoolQuery(schoolId)));

    [HttpPost]
    public async Task<IActionResult> CreateClass([FromBody] ClassCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _mediator.Send(new CreateClassCommand(dto));
            return CreatedAtAction(nameof(GetClass), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateClass(int id, [FromBody] ClassCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _mediator.Send(new UpdateClassCommand(id, dto));
            return Ok(result);
        }
        catch (KeyNotFoundException) { return NotFound(new { message = "Class not found" }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteClass(int id)
    {
        var result = await _mediator.Send(new DeleteClassCommand(id));
        return result ? Ok(new { message = "Class deleted successfully" }) : NotFound(new { message = "Class not found" });
    }
}
