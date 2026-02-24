using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services;

namespace MotionRobotics.LMS.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/certificates")]
    [Authorize(Roles = "SuperAdmin,SchoolAdmin")]
    public class AdminCertificatesController : ControllerBase
    {
        private readonly ICertificateService _certificateService;
        private readonly ILogger<AdminCertificatesController> _logger;

        public AdminCertificatesController(ICertificateService certificateService, ILogger<AdminCertificatesController> logger)
        {
            _certificateService = certificateService;
            _logger = logger;
        }

        /// <summary>
        /// Get all certificates with optional filters
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<CertificateListDto>> GetAllCertificates(
            [FromQuery] int? schoolId = null,
            [FromQuery] int? roboticsLevelId = null,
            [FromQuery] int? academicYearId = null)
        {
            try
            {
                var certificates = await _certificateService.GetAllCertificatesAsync(schoolId, roboticsLevelId, academicYearId);
                return Ok(certificates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting certificates");
                return StatusCode(500, new { message = "An error occurred while retrieving certificates" });
            }
        }

        /// <summary>
        /// Get certificate by ID
        /// </summary>
        [HttpGet("{certificateId:int}")]
        public async Task<ActionResult<CertificateDetailDto>> GetCertificate(int certificateId)
        {
            try
            {
                var certificate = await _certificateService.GetCertificateByIdAsync(certificateId);
                if (certificate == null)
                {
                    return NotFound(new { message = "Certificate not found" });
                }
                return Ok(certificate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting certificate {CertificateId}", certificateId);
                return StatusCode(500, new { message = "An error occurred while retrieving the certificate" });
            }
        }

        /// <summary>
        /// Get certificate HTML for rendering/printing
        /// </summary>
        [HttpGet("{certificateId:int}/html")]
        public async Task<ActionResult> GetCertificateHtml(int certificateId)
        {
            try
            {
                var html = await _certificateService.GenerateCertificateHtmlAsync(certificateId);
                if (string.IsNullOrEmpty(html))
                {
                    return NotFound(new { message = "Certificate not found" });
                }
                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating certificate HTML {CertificateId}", certificateId);
                return StatusCode(500, new { message = "An error occurred while generating the certificate" });
            }
        }

        /// <summary>
        /// Regenerate certificate with optional custom title
        /// </summary>
        [HttpPost("{certificateId:int}/regenerate")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult> RegenerateCertificate(int certificateId, [FromBody] RegenerateCertificateDto? dto = null)
        {
            try
            {
                var result = await _certificateService.RegenerateCertificateAsync(certificateId, dto?.CustomTitle);
                if (!result)
                {
                    return NotFound(new { message = "Certificate not found" });
                }
                return Ok(new { message = "Certificate regenerated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating certificate {CertificateId}", certificateId);
                return StatusCode(500, new { message = "An error occurred while regenerating the certificate" });
            }
        }
    }
}
