using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Features.Teacher.Lab;

[ApiController]
[Route("api/teacher/lab")]
[Authorize(Roles = "Teacher")]
[Tags("Teacher - Lab")]
public class TeacherLabController : ControllerBase
{
    private readonly IMediator _mediator;
    public TeacherLabController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetLab()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _mediator.Send(new GetTeacherLabQuery(userId));
        return result == null
            ? NotFound(new { message = "Lab information not available." })
            : Ok(result);
    }
}
