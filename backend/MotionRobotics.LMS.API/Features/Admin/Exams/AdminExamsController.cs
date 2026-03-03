using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Exams;

[ApiController]
[Route("api/admin/exams")]
[Authorize(Roles = "SuperAdmin,SchoolAdmin")]
[Tags("Admin - Exams")]
public class AdminExamsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AdminExamsController> _logger;
    public AdminExamsController(IMediator mediator, ILogger<AdminExamsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // Helper to get session school context for multi-tenant isolation
    private (string? Role, int? SchoolId) GetSessionContext()
    {
        var role = HttpContext.Items["SessionRole"] as string;
        var schoolId = HttpContext.Items["SessionSchoolId"] as int?;
        return (role, schoolId);
    }

    [HttpGet]
    public async Task<ActionResult<ExamListDto>> GetAllExams([FromQuery] int? roboticsLevelId, [FromQuery] bool? isActive)
    {
        // Exams are global (shared curriculum), all admins can view
        try { return Ok(await _mediator.Send(new GetAllExamsQuery(roboticsLevelId, isActive))); }
        catch (Exception ex) { _logger.LogError(ex, "Error getting exams"); return StatusCode(500, new { message = "An error occurred" }); }
    }

    [HttpGet("{examId:int}")]
    public async Task<ActionResult<ExamDetailDto>> GetExam(int examId)
    {
        var exam = await _mediator.Send(new GetExamByIdQuery(examId));
        return exam == null ? NotFound(new { message = "Exam not found" }) : Ok(exam);
    }

    [HttpGet("{examId:int}/results")]
    public async Task<ActionResult<ExamResultsListDto>> GetExamResults(int examId, [FromQuery] int? schoolId, [FromQuery] int? classId)
    {
        // Multi-tenant: SchoolAdmin can only see their school's results
        var (role, sessionSchoolId) = GetSessionContext();
        if (role == "SchoolAdmin" && sessionSchoolId.HasValue)
        {
            if (schoolId.HasValue && schoolId.Value != sessionSchoolId.Value)
                return Forbid();
            schoolId = sessionSchoolId; // Force filter to their school
        }

        try { return Ok(await _mediator.Send(new GetExamResultsQuery(examId, schoolId, classId))); }
        catch (Exception ex) { _logger.LogError(ex, "Error getting exam results"); return StatusCode(500, new { message = "An error occurred" }); }
    }

    [HttpGet("{examId:int}/statistics")]
    public async Task<ActionResult<ExamStatisticsDto>> GetExamStatistics(int examId)
    {
        // Multi-tenant: SchoolAdmin sees school-scoped results; SuperAdmin sees all
        var (role, sessionSchoolId) = GetSessionContext();
        int? schoolId = null;
        if (role == "SchoolAdmin" && sessionSchoolId.HasValue)
            schoolId = sessionSchoolId;

        var stats = await _mediator.Send(new GetExamStatisticsQuery(examId, schoolId));
        return stats == null ? NotFound(new { message = "Exam not found" }) : Ok(stats);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ExamDetailDto>> CreateExam([FromBody] ExamCreateDto dto)
    {
        try
        {
            if (dto.Questions.Count == 0) return BadRequest(new { message = "Exam must have at least one question" });
            var exam = await _mediator.Send(new CreateExamCommand(dto));
            return CreatedAtAction(nameof(GetExam), new { examId = exam.Id }, exam);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error creating exam"); return StatusCode(500, new { message = "An error occurred" }); }
    }

    [HttpPut("{examId:int}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ExamDetailDto>> UpdateExam(int examId, [FromBody] ExamUpdateDto dto)
    {
        var exam = await _mediator.Send(new UpdateExamCommand(examId, dto));
        return exam == null ? NotFound(new { message = "Exam not found" }) : Ok(exam);
    }

    [HttpDelete("{examId:int}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteExam(int examId)
    {
        var result = await _mediator.Send(new DeleteExamCommand(examId));
        return result ? Ok(new { message = "Exam deleted" }) : NotFound(new { message = "Exam not found" });
    }
}
