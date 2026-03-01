using MediatR;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Teacher.Auth;

[ApiController]
[Route("api/teacher/auth")]
[Tags("Teacher - Auth")]
public class TeacherAuthController : ControllerBase
{
    private readonly IMediator _mediator;
    public TeacherAuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AdminLoginRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.Send(new TeacherLoginCommand(request));
        return result == null ? Unauthorized(new { message = "Invalid email or password or not a teacher" }) : Ok(result);
    }
}
