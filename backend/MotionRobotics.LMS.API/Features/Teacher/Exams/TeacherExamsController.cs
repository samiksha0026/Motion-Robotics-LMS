using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Teacher;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Features.Teacher.Exams;

[ApiController]
[Route("api/teacher/exams")]
[Authorize(Roles = "Teacher")]
[Tags("Teacher - Exams")]
public class TeacherExamsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TeacherExamsController> _logger;
    public TeacherExamsController(IMediator mediator, ILogger<TeacherExamsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    private string? GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    [HttpGet("results")]
    public async Task<ActionResult<TeacherExamResultsListDto>> GetExamResults([FromQuery] int? classId, [FromQuery] int? examId)
    {
        try
        {
            var userId = GetUserId(); if (string.IsNullOrEmpty(userId)) return Unauthorized(new { message = "Unauthorized" });
            return Ok(await _mediator.Send(new GetTeacherExamResultsQuery(userId, classId, examId)));
        }
        catch (KeyNotFoundException ex) { return Unauthorized(new { message = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error getting exam results"); return StatusCode(500, new { message = "An error occurred" }); }
    }

    [HttpGet("{examId:int}/overview")]
    public async Task<ActionResult<TeacherExamOverviewDto>> GetExamOverview(int examId)
    {
        try
        {
            var userId = GetUserId(); if (string.IsNullOrEmpty(userId)) return Unauthorized(new { message = "Unauthorized" });
            var overview = await _mediator.Send(new GetTeacherExamOverviewQuery(userId, examId));
            return overview == null ? NotFound(new { message = "Exam not found" }) : Ok(overview);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error getting exam overview"); return StatusCode(500, new { message = "An error occurred" }); }
    }

    [HttpGet("eligibility")]
    public async Task<ActionResult<List<StudentExamEligibilityDto>>> GetStudentsEligibility(
        [FromQuery] int classId, [FromQuery] int roboticsLevelId)
    {
        try
        {
            var userId = GetUserId(); if (string.IsNullOrEmpty(userId)) return Unauthorized(new { message = "Unauthorized" });
            return Ok(await _mediator.Send(new GetStudentsExamEligibilityQuery(userId, classId, roboticsLevelId)));
        }
        catch (Exception ex) { _logger.LogError(ex, "Error getting students eligibility"); return StatusCode(500, new { message = "An error occurred" }); }
    }
}
