using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Features.Teacher.Dashboard;

[ApiController]
[Route("api/teacher/dashboard")]
[Authorize(Roles = "Teacher")]
[Tags("Teacher - Dashboard")]
public class TeacherDashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TeacherDashboardController> _logger;
    public TeacherDashboardController(IMediator mediator, ILogger<TeacherDashboardController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    private string? GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        try
        {
            var userId = GetUserId(); if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var result = await _mediator.Send(new GetTeacherDashboardQuery(userId));
            return result == null ? NotFound(new { message = "Teacher not found" }) : Ok(result);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error getting teacher dashboard"); return StatusCode(500, new { message = "An error occurred" }); }
    }

    [HttpGet("classes")]
    public async Task<IActionResult> GetAssignedClasses()
    {
        try
        {
            var userId = GetUserId(); if (string.IsNullOrEmpty(userId)) return Unauthorized();
            return Ok(await _mediator.Send(new GetAssignedClassesQuery(userId)));
        }
        catch (Exception ex) { _logger.LogError(ex, "Error getting assigned classes"); return StatusCode(500, new { message = "An error occurred" }); }
    }
}
