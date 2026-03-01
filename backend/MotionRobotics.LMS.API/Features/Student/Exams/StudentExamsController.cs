using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Student;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Features.Student.Exams;

[ApiController]
[Route("api/student/exam")]
[Authorize(Roles = "Student")]
[Tags("Student - Exams")]
public class StudentExamsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StudentExamsController> _logger;
    public StudentExamsController(IMediator mediator, ILogger<StudentExamsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    private string? GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    [HttpGet("eligibility")]
    public async Task<IActionResult> CheckEligibility()
    {
        try
        {
            var userId = GetUserId(); if (string.IsNullOrEmpty(userId)) return Unauthorized();
            return Ok(await _mediator.Send(new CheckExamEligibilityQuery(userId)));
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartExam()
    {
        try
        {
            var userId = GetUserId(); if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var result = await _mediator.Send(new StartExamQuery(userId));
            return result == null ? BadRequest(new { message = "Cannot start exam" }) : Ok(result);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error starting exam"); return StatusCode(500, new { message = "An error occurred" }); }
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitExam([FromBody] ExamAnswerSubmitDto dto)
    {
        try
        {
            var userId = GetUserId(); if (string.IsNullOrEmpty(userId)) return Unauthorized();
            return Ok(await _mediator.Send(new SubmitExamCommand(userId, dto)));
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error submitting exam"); return StatusCode(500, new { message = "An error occurred" }); }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        try
        {
            var userId = GetUserId(); if (string.IsNullOrEmpty(userId)) return Unauthorized();
            return Ok(await _mediator.Send(new GetExamHistoryQuery(userId)));
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpGet("results/{resultId:int}")]
    public async Task<IActionResult> GetResult(int resultId)
    {
        try
        {
            var userId = GetUserId(); if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var result = await _mediator.Send(new GetExamResultQuery(userId, resultId));
            return result == null ? NotFound(new { message = "Result not found" }) : Ok(result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }
}
