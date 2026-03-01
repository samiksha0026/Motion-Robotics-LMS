using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Features.Teacher.Students;

[ApiController]
[Route("api/teacher/students")]
[Authorize(Roles = "Teacher")]
[Tags("Teacher - Students")]
public class TeacherStudentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TeacherStudentsController> _logger;
    public TeacherStudentsController(IMediator mediator, ILogger<TeacherStudentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("{studentId:int}/progress")]
    public async Task<IActionResult> GetStudentProgress(int studentId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var result = await _mediator.Send(new GetStudentProgressQuery(userId, studentId));
            return result == null ? NotFound(new { message = "Student not found or not in your class" }) : Ok(result);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error getting student progress"); return StatusCode(500, new { message = "An error occurred" }); }
    }
}
