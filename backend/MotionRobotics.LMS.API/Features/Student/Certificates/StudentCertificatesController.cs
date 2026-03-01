using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Features.Student.Certificates;

[ApiController]
[Route("api/student/certificates")]
[Authorize(Roles = "Student")]
[Tags("Student - Certificates")]
public class StudentCertificatesController : ControllerBase
{
    private readonly IMediator _mediator;
    public StudentCertificatesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetCertificates()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        return Ok(await _mediator.Send(new GetStudentCertificatesQuery(userId)));
    }

    [HttpGet("{certificateId:int}")]
    public async Task<IActionResult> GetCertificate(int certificateId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var result = await _mediator.Send(new GetStudentCertificateQuery(userId, certificateId));
        return result == null ? NotFound(new { message = "Certificate not found" }) : Ok(result);
    }
}
