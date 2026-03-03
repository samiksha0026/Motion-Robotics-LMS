using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Features.Admin.Classes;

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
    {
        // Multi-tenant isolation: SchoolAdmin can only see their school's teachers
        var schoolId = HttpContext.Items["SessionSchoolId"] as int?;
        var role = HttpContext.Items["SessionRole"] as string;

        if (role == "SchoolAdmin" && schoolId.HasValue)
            return Ok(await _mediator.Send(new GetTeachersBySchoolQuery(schoolId.Value)));

        // SuperAdmin sees all teachers
        return Ok(await _mediator.Send(new GetAllTeachersQuery()));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTeacher(int id)
    {
        var result = await _mediator.Send(new GetTeacherByIdQuery(id));
        if (result == null) return NotFound(new { message = "Teacher not found" });

        // Multi-tenant isolation: SchoolAdmin can only access their school's teachers
        var schoolId = HttpContext.Items["SessionSchoolId"] as int?;
        var role = HttpContext.Items["SessionRole"] as string;
        if (role == "SchoolAdmin" && schoolId.HasValue && result.SchoolId != schoolId.Value)
            return Forbid();

        return Ok(result);
    }

    [HttpGet("school/{schoolId:int}")]
    public async Task<IActionResult> GetTeachersBySchool(int schoolId)
    {
        // Multi-tenant isolation: SchoolAdmin can only access their own school
        var sessionSchoolId = HttpContext.Items["SessionSchoolId"] as int?;
        var role = HttpContext.Items["SessionRole"] as string;
        if (role == "SchoolAdmin" && sessionSchoolId.HasValue && schoolId != sessionSchoolId.Value)
            return Forbid();

        return Ok(await _mediator.Send(new GetTeachersBySchoolQuery(schoolId)));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTeacher([FromBody] TeacherCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Multi-tenant isolation: SchoolAdmin can only create teachers in their school
        var sessionSchoolId = HttpContext.Items["SessionSchoolId"] as int?;
        var role = HttpContext.Items["SessionRole"] as string;
        if (role == "SchoolAdmin" && sessionSchoolId.HasValue && dto.SchoolId != sessionSchoolId.Value)
            return Forbid();

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

        // Multi-tenant isolation: verify ownership before update
        var teacher = await _mediator.Send(new GetTeacherByIdQuery(id));
        if (teacher == null) return NotFound(new { message = "Teacher not found" });

        var sessionSchoolId = HttpContext.Items["SessionSchoolId"] as int?;
        var role = HttpContext.Items["SessionRole"] as string;
        if (role == "SchoolAdmin" && sessionSchoolId.HasValue && teacher.SchoolId != sessionSchoolId.Value)
            return Forbid();

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
        // Multi-tenant isolation: verify ownership before delete
        var teacher = await _mediator.Send(new GetTeacherByIdQuery(id));
        if (teacher == null) return NotFound(new { message = "Teacher not found" });

        var sessionSchoolId = HttpContext.Items["SessionSchoolId"] as int?;
        var role = HttpContext.Items["SessionRole"] as string;
        if (role == "SchoolAdmin" && sessionSchoolId.HasValue && teacher.SchoolId != sessionSchoolId.Value)
            return Forbid();

        var result = await _mediator.Send(new DeleteTeacherCommand(id));
        return result ? Ok(new { message = "Teacher deleted successfully" }) : NotFound(new { message = "Teacher not found" });
    }

    [HttpPost("{teacherId:int}/classes/{classId:int}")]
    public async Task<IActionResult> AssignClass(int teacherId, int classId)
    {
        // Multi-tenant isolation: verify teacher and class belong to admin's school
        var sessionSchoolId = HttpContext.Items["SessionSchoolId"] as int?;
        var role = HttpContext.Items["SessionRole"] as string;

        if (role == "SchoolAdmin" && sessionSchoolId.HasValue)
        {
            var teacher = await _mediator.Send(new GetTeacherByIdQuery(teacherId));
            if (teacher == null) return NotFound(new { message = "Teacher not found" });
            if (teacher.SchoolId != sessionSchoolId.Value) return Forbid();

            var cls = await _mediator.Send(new GetClassByIdQuery(classId));
            if (cls == null) return NotFound(new { message = "Class not found" });
            if (cls.SchoolId != sessionSchoolId.Value) return Forbid();
        }

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
        // Multi-tenant isolation: verify teacher and class belong to admin's school
        var sessionSchoolId = HttpContext.Items["SessionSchoolId"] as int?;
        var role = HttpContext.Items["SessionRole"] as string;

        if (role == "SchoolAdmin" && sessionSchoolId.HasValue)
        {
            var teacher = await _mediator.Send(new GetTeacherByIdQuery(teacherId));
            if (teacher == null) return NotFound(new { message = "Teacher not found" });
            if (teacher.SchoolId != sessionSchoolId.Value) return Forbid();

            var cls = await _mediator.Send(new GetClassByIdQuery(classId));
            if (cls == null) return NotFound(new { message = "Class not found" });
            if (cls.SchoolId != sessionSchoolId.Value) return Forbid();
        }

        try
        {
            await _mediator.Send(new RemoveClassFromTeacherCommand(teacherId, classId));
            return Ok(new { message = "Class removed successfully" });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }
}
