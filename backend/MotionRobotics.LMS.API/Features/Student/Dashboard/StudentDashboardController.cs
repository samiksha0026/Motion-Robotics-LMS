using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Features.Student.Dashboard;

[ApiController]
[Route("api/student/dashboard")]
[Authorize(Roles = "Student")]
[Tags("Student - Dashboard")]
public class StudentDashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    public StudentDashboardController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var result = await _mediator.Send(new GetStudentDashboardQuery(userId));
        if (result == null) return NotFound(new { message = "Dashboard data not available" });
        return Ok(result);
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var result = await _mediator.Send(new GetStudentProfileQuery(userId));
        if (result == null) return NotFound(new { message = "Profile not found" });
        return Ok(result);
    }
}
