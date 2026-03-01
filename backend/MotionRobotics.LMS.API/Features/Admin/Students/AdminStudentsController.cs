using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Students;

[ApiController]
[Route("api/admin/students")]
[Authorize(Roles = "SuperAdmin,SchoolAdmin")]
[Tags("Admin - Students")]
public class AdminStudentsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminStudentsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAllStudents()
        => Ok(await _mediator.Send(new GetAllStudentsQuery()));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetStudent(int id)
    {
        var result = await _mediator.Send(new GetStudentByIdQuery(id));
        return result == null ? NotFound(new { message = "Student not found" }) : Ok(result);
    }

    [HttpGet("school/{schoolId:int}")]
    public async Task<IActionResult> GetStudentsBySchool(int schoolId)
        => Ok(await _mediator.Send(new GetStudentsBySchoolQuery(schoolId)));

    [HttpGet("class/{classId:int}")]
    public async Task<IActionResult> GetStudentsByClass(int classId)
        => Ok(await _mediator.Send(new GetStudentsByClassQuery(classId)));

    [HttpPost]
    public async Task<IActionResult> CreateStudent([FromBody] StudentCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _mediator.Send(new CreateStudentCommand(dto));
            return CreatedAtAction(nameof(GetStudent), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteStudent(int id)
    {
        var result = await _mediator.Send(new DeleteStudentCommand(id));
        return result ? Ok(new { message = "Student deleted successfully" }) : NotFound(new { message = "Student not found" });
    }
}
