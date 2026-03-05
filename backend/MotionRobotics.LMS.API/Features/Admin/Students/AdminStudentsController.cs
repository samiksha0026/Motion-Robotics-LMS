using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Features.Admin.Classes;

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
    {
        // Multi-tenant isolation: SchoolAdmin can only see their school's students
        var schoolId = HttpContext.Items["SessionSchoolId"] as int?;
        var role = HttpContext.Items["SessionRole"] as string;

        if (role == "SchoolAdmin" && schoolId.HasValue)
            return Ok(await _mediator.Send(new GetStudentsBySchoolQuery(schoolId.Value)));

        // SuperAdmin sees all students
        return Ok(await _mediator.Send(new GetAllStudentsQuery()));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetStudent(int id)
    {
        var result = await _mediator.Send(new GetStudentByIdQuery(id));
        if (result == null) return NotFound(new { message = "Student not found" });

        // Multi-tenant isolation: SchoolAdmin can only access their school's students
        var schoolId = HttpContext.Items["SessionSchoolId"] as int?;
        var role = HttpContext.Items["SessionRole"] as string;
        if (role == "SchoolAdmin" && schoolId.HasValue && result.SchoolId != schoolId.Value)
            return Forbid();

        return Ok(result);
    }

    [HttpGet("school/{schoolId:int}")]
    public async Task<IActionResult> GetStudentsBySchool(int schoolId)
    {
        // Multi-tenant isolation: SchoolAdmin can only access their own school
        var sessionSchoolId = HttpContext.Items["SessionSchoolId"] as int?;
        var role = HttpContext.Items["SessionRole"] as string;
        if (role == "SchoolAdmin" && sessionSchoolId.HasValue && schoolId != sessionSchoolId.Value)
            return Forbid();

        return Ok(await _mediator.Send(new GetStudentsBySchoolQuery(schoolId)));
    }

    [HttpGet("class/{classId:int}")]
    public async Task<IActionResult> GetStudentsByClass(int classId)
    {
        // Multi-tenant isolation: SchoolAdmin can only access their school's classes
        var sessionSchoolId = HttpContext.Items["SessionSchoolId"] as int?;
        var role = HttpContext.Items["SessionRole"] as string;

        if (role == "SchoolAdmin" && sessionSchoolId.HasValue)
        {
            var cls = await _mediator.Send(new GetClassByIdQuery(classId));
            if (cls == null) return NotFound(new { message = "Class not found" });
            if (cls.SchoolId != sessionSchoolId.Value) return Forbid();
        }

        return Ok(await _mediator.Send(new GetStudentsByClassQuery(classId)));
    }

    [HttpPost]
    public async Task<IActionResult> CreateStudent([FromBody] StudentCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Multi-tenant isolation: SchoolAdmin can only create students in their school
        var sessionSchoolId = HttpContext.Items["SessionSchoolId"] as int?;
        var role = HttpContext.Items["SessionRole"] as string;
        if (role == "SchoolAdmin" && sessionSchoolId.HasValue && dto.SchoolId != sessionSchoolId.Value)
            return Forbid();

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
        // Multi-tenant isolation: verify ownership before delete
        var student = await _mediator.Send(new GetStudentByIdQuery(id));
        if (student == null) return NotFound(new { message = "Student not found" });

        var sessionSchoolId = HttpContext.Items["SessionSchoolId"] as int?;
        var role = HttpContext.Items["SessionRole"] as string;
        if (role == "SchoolAdmin" && sessionSchoolId.HasValue && student.SchoolId != sessionSchoolId.Value)
            return Forbid();

        var result = await _mediator.Send(new DeleteStudentCommand(id));
        return result ? Ok(new { message = "Student deleted successfully" }) : NotFound(new { message = "Student not found" });
    }

    // ────────────────────────────────────────────────────────────────────────────
    // EXCEL BULK IMPORT
    // ────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Upload an Excel file (.xlsx) to bulk-create students.
    /// Valid rows are inserted; invalid rows are returned in the error list.
    /// Default password for every imported student: Student@123
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(MotionRobotics.LMS.API.DTOs.Admin.StudentImportResultDto), 200)]
    public async Task<IActionResult> UploadStudents([FromForm] IFormFile file)
    {
        // Resolve school from session (SchoolAdmin) or reject
        var sessionSchoolId = HttpContext.Items["SessionSchoolId"] as int?;
        var role = HttpContext.Items["SessionRole"] as string;

        if (role == "SchoolAdmin" && !sessionSchoolId.HasValue)
            return Forbid();

        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Please attach an Excel file (.xlsx)." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".xlsx")
            return BadRequest(new { message = "Only .xlsx files are accepted. Download the template to get started." });

        try
        {
            var result = await _mediator.Send(
                new ImportStudentsCommand(file, sessionSchoolId!.Value));

            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>
    /// Download a pre-formatted Excel template with headers and sample rows.
    /// Share this with the school so they fill in the correct format.
    /// </summary>
    [HttpGet("template")]
    public async Task<IActionResult> DownloadTemplate()
    {
        var bytes = await _mediator.Send(new DownloadStudentTemplateQuery());
        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "students_import_template.xlsx");
    }
}
