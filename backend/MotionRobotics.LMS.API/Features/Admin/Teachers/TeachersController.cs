using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Teachers;

[ApiController]
[Route("api/admin/teachers")]
[Authorize(Roles = "SuperAdmin,SchoolAdmin")]
[Tags("Admin - Teachers")]
public class TeachersController : ControllerBase
{
    private readonly IMediator _mediator;
    public TeachersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAllTeachers()
        => Ok(await _mediator.Send(new GetAllTeachersQuery()));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTeacher(int id)
    {
        var result = await _mediator.Send(new GetTeacherByIdQuery(id));
        return result == null ? NotFound(new { message = "Teacher not found" }) : Ok(result);
    }

    [HttpGet("school/{schoolId:int}")]
    public async Task<IActionResult> GetTeachersBySchool(int schoolId)
        => Ok(await _mediator.Send(new GetTeachersBySchoolQuery(schoolId)));

    [HttpPost]
    public async Task<IActionResult> CreateTeacher([FromBody] TeacherCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _mediator.Send(new CreateTeacherCommand(dto));
            return CreatedAtAction(nameof(GetTeacher), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateTeacher(int id, [FromBody] TeacherCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _mediator.Send(new UpdateTeacherCommand(id, dto));
            return Ok(result);
        }
        catch (KeyNotFoundException) { return NotFound(new { message = "Teacher not found" }); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTeacher(int id)
    {
        var result = await _mediator.Send(new DeleteTeacherCommand(id));
        return result ? Ok(new { message = "Teacher deleted successfully" }) : NotFound(new { message = "Teacher not found" });
    }

    [HttpPost("{teacherId:int}/classes/{classId:int}")]
    public async Task<IActionResult> AssignClass(int teacherId, int classId)
    {
        try
        {
            await _mediator.Send(new AssignClassToTeacherCommand(teacherId, classId));
            return Ok(new { message = "Class assigned successfully" });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("{teacherId:int}/classes/{classId:int}")]
    public async Task<IActionResult> RemoveClass(int teacherId, int classId)
    {
        try
        {
            await _mediator.Send(new RemoveClassFromTeacherCommand(teacherId, classId));
            return Ok(new { message = "Class removed successfully" });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }
}
