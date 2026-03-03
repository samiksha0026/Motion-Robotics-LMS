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

    // Helper to get session school context for multi-tenant isolation
    private (string? Role, int? SchoolId) GetSessionContext()
    {
        var role = HttpContext.Items["SessionRole"] as string;
        var schoolId = HttpContext.Items["SessionSchoolId"] as int?;
        return (role, schoolId);
    }

    [HttpGet]
    public async Task<ActionResult<CertificateListDto>> GetAllCertificates(
        [FromQuery] int? schoolId, [FromQuery] int? roboticsLevelId, [FromQuery] int? academicYearId)
    {
        // Multi-tenant: SchoolAdmin can only see their school's certificates
        var (role, sessionSchoolId) = GetSessionContext();
        if (role == "SchoolAdmin" && sessionSchoolId.HasValue)
        {
            if (schoolId.HasValue && schoolId.Value != sessionSchoolId.Value)
                return Forbid();
            schoolId = sessionSchoolId;
        }

        try { return Ok(await _mediator.Send(new GetAllCertificatesQuery(schoolId, roboticsLevelId, academicYearId))); }
        catch (Exception ex) { _logger.LogError(ex, "Error getting certificates"); return StatusCode(500, new { message = "An error occurred" }); }
    }

    [HttpGet("{certificateId:int}")]
    public async Task<ActionResult<CertificateDetailDto>> GetCertificate(int certificateId)
    {
        var cert = await _mediator.Send(new GetCertificateByIdQuery(certificateId));
        if (cert == null) return NotFound(new { message = "Certificate not found" });

        // Multi-tenant: SchoolAdmin can only access their school's certificates
        var (role, sessionSchoolId) = GetSessionContext();
        if (role == "SchoolAdmin" && sessionSchoolId.HasValue && cert.SchoolId != sessionSchoolId.Value)
            return Forbid();

        return Ok(cert);
    }

    [HttpGet("{certificateId:int}/html")]
    public async Task<IActionResult> GetCertificateHtml(int certificateId)
    {
        try
        {
            // First check certificate ownership
            var cert = await _mediator.Send(new GetCertificateByIdQuery(certificateId));
            if (cert == null) return NotFound(new { message = "Certificate not found" });

            // Multi-tenant: SchoolAdmin can only access their school's certificates
            var (role, sessionSchoolId) = GetSessionContext();
            if (role == "SchoolAdmin" && sessionSchoolId.HasValue && cert.SchoolId != sessionSchoolId.Value)
                return Forbid();

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
