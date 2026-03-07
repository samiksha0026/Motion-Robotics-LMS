using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Features.Student.Lab;

[ApiController]
[Route("api/student/lab")]
[Authorize(Roles = "Student")]
[Tags("Student - Lab")]
public class StudentLabController : ControllerBase
{
    private readonly IMediator _mediator;
    public StudentLabController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetLab()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _mediator.Send(new GetStudentLabQuery(userId));
        return result == null
            ? NotFound(new { message = "Lab information not available." })
            : Ok(result);
    }
}
