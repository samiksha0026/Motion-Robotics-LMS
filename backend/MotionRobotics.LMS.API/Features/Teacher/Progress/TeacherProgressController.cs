using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Features.Teacher.Progress;

[ApiController]
[Route("api/teacher/progress")]
[Authorize(Roles = "Teacher")]
[Tags("Teacher - Progress")]
public class TeacherProgressController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TeacherProgressController> _logger;
    public TeacherProgressController(IMediator mediator, ILogger<TeacherProgressController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    private string? GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    [HttpGet]
    public async Task<ActionResult<List<StudentProgressOverviewDto>>> GetStudentsProgress([FromQuery] int? classId)
    {
        try
        {
            var userId = GetUserId(); if (string.IsNullOrEmpty(userId)) return Unauthorized();
            return Ok(await _mediator.Send(new GetTeacherStudentsProgressQuery(userId, classId)));
        }
        catch (KeyNotFoundException ex) { return Unauthorized(new { message = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error getting students progress"); return StatusCode(500, new { message = "An error occurred" }); }
    }

    [HttpGet("student/{studentId:int}")]
    public async Task<ActionResult<StudentProgressOverviewDto>> GetStudentProgress(int studentId)
    {
        try
        {
            var userId = GetUserId(); if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var result = await _mediator.Send(new GetTeacherStudentProgressQuery(userId, studentId));
            return result == null ? NotFound(new { message = "Student not found" }) : Ok(result);
        }
        catch (KeyNotFoundException ex) { return Unauthorized(new { message = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error getting student progress"); return StatusCode(500, new { message = "An error occurred" }); }
    }
}
