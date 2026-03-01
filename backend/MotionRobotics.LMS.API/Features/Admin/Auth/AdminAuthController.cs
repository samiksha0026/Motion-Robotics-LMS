using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Auth;

[ApiController]
[Route("api/admin/auth")]
[Tags("Admin - Auth")]
public class AdminAuthController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminAuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] AdminLoginRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(new AdminLoginCommand(request));
        if (result == null)
            return Unauthorized(new { message = "Invalid email or password" });

        return Ok(result);
    }
}
