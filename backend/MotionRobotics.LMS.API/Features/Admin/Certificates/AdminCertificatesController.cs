using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Certificates;

[ApiController]
[Route("api/admin/certificates")]
[Authorize(Roles = "SuperAdmin,SchoolAdmin")]
[Tags("Admin - Certificates")]
public class AdminCertificatesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AdminCertificatesController> _logger;
    public AdminCertificatesController(IMediator mediator, ILogger<AdminCertificatesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<CertificateListDto>> GetAllCertificates(
        [FromQuery] int? schoolId, [FromQuery] int? roboticsLevelId, [FromQuery] int? academicYearId)
    {
        try { return Ok(await _mediator.Send(new GetAllCertificatesQuery(schoolId, roboticsLevelId, academicYearId))); }
        catch (Exception ex) { _logger.LogError(ex, "Error getting certificates"); return StatusCode(500, new { message = "An error occurred" }); }
    }

    [HttpGet("{certificateId:int}")]
    public async Task<ActionResult<CertificateDetailDto>> GetCertificate(int certificateId)
    {
        var cert = await _mediator.Send(new GetCertificateByIdQuery(certificateId));
        return cert == null ? NotFound(new { message = "Certificate not found" }) : Ok(cert);
    }

    [HttpGet("{certificateId:int}/html")]
    public async Task<IActionResult> GetCertificateHtml(int certificateId)
    {
        try
        {
            var html = await _mediator.Send(new GetCertificateHtmlQuery(certificateId));
            return Content(html, "text/html");
        }
        catch (KeyNotFoundException) { return NotFound(new { message = "Certificate not found" }); }
        catch (Exception ex) { _logger.LogError(ex, "Error generating certificate HTML"); return StatusCode(500, new { message = "An error occurred" }); }
    }

    [HttpPost("{certificateId:int}/regenerate")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> RegenerateCertificate(int certificateId, [FromBody] RegenerateRequest? request = null)
    {
        var result = await _mediator.Send(new RegenerateCertificateCommand(certificateId, request?.CustomTitle));
        return result ? Ok(new { message = "Certificate regenerated successfully" }) : NotFound(new { message = "Certificate not found" });
    }

    public record RegenerateRequest(string? CustomTitle);
}
