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
    {
        // Multi-tenant isolation: SchoolAdmin can only see their school's classes
        var schoolId = HttpContext.Items["SessionSchoolId"] as int?;
        var role = HttpContext.Items["SessionRole"] as string;

        if (role == "SchoolAdmin" && schoolId.HasValue)
            return Ok(await _mediator.Send(new GetClassesBySchoolQuery(schoolId.Value)));

        // SuperAdmin sees all classes
        return Ok(await _mediator.Send(new GetAllClassesQuery()));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetClass(int id)
    {
        var result = await _mediator.Send(new GetClassByIdQuery(id));
        if (result == null) return NotFound(new { message = "Class not found" });

        // Multi-tenant isolation: SchoolAdmin can only access their school's classes
        var schoolId = HttpContext.Items["SessionSchoolId"] as int?;
        var role = HttpContext.Items["SessionRole"] as string;
        if (role == "SchoolAdmin" && schoolId.HasValue && result.SchoolId != schoolId.Value)
            return Forbid();

        return Ok(result);
    }

    [HttpGet("school/{schoolId:int}")]
    public async Task<IActionResult> GetClassesBySchool(int schoolId)
    {
        // Multi-tenant isolation: SchoolAdmin can only access their own school
        var sessionSchoolId = HttpContext.Items["SessionSchoolId"] as int?;
        var role = HttpContext.Items["SessionRole"] as string;
        if (role == "SchoolAdmin" && sessionSchoolId.HasValue && schoolId != sessionSchoolId.Value)
            return Forbid();

        return Ok(await _mediator.Send(new GetClassesBySchoolQuery(schoolId)));
    }

    [HttpPost]
    public async Task<IActionResult> CreateClass([FromBody] ClassCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Multi-tenant isolation: SchoolAdmin can only create classes in their school
        var sessionSchoolId = HttpContext.Items["SessionSchoolId"] as int?;
        var role = HttpContext.Items["SessionRole"] as string;
        if (role == "SchoolAdmin" && sessionSchoolId.HasValue && dto.SchoolId != sessionSchoolId.Value)
            return Forbid();

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

        // Multi-tenant isolation: verify ownership before update
        var existingClass = await _mediator.Send(new GetClassByIdQuery(id));
        if (existingClass == null) return NotFound(new { message = "Class not found" });

        var sessionSchoolId = HttpContext.Items["SessionSchoolId"] as int?;
        var role = HttpContext.Items["SessionRole"] as string;
        if (role == "SchoolAdmin" && sessionSchoolId.HasValue && existingClass.SchoolId != sessionSchoolId.Value)
            return Forbid();

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
        // Multi-tenant isolation: verify ownership before delete
        var existingClass = await _mediator.Send(new GetClassByIdQuery(id));
        if (existingClass == null) return NotFound(new { message = "Class not found" });

        var sessionSchoolId = HttpContext.Items["SessionSchoolId"] as int?;
        var role = HttpContext.Items["SessionRole"] as string;
        if (role == "SchoolAdmin" && sessionSchoolId.HasValue && existingClass.SchoolId != sessionSchoolId.Value)
            return Forbid();

        var result = await _mediator.Send(new DeleteClassCommand(id));
        return result ? Ok(new { message = "Class deleted successfully" }) : NotFound(new { message = "Class not found" });
    }
}
