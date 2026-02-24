using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services;

namespace MotionRobotics.LMS.API.Controllers
{
    /// <summary>
    /// Public certificate verification endpoint
    /// </summary>
    [ApiController]
    [Route("api/certificates")]
    public class CertificatesController : ControllerBase
    {
        private readonly ICertificateService _certificateService;
        private readonly ILogger<CertificatesController> _logger;

        public CertificatesController(ICertificateService certificateService, ILogger<CertificatesController> logger)
        {
            _certificateService = certificateService;
            _logger = logger;
        }

        /// <summary>
        /// Public endpoint to verify a certificate by its number
        /// </summary>
        [HttpGet("verify/{certificateNumber}")]
        public async Task<ActionResult<CertificateVerificationDto>> VerifyCertificate(string certificateNumber)
        {
            try
            {
                var result = await _certificateService.VerifyCertificateAsync(certificateNumber);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying certificate {CertificateNumber}", certificateNumber);
                return StatusCode(500, new { message = "An error occurred while verifying the certificate" });
            }
        }

        /// <summary>
        /// Public endpoint to view certificate HTML
        /// </summary>
        [HttpGet("view/{certificateNumber}")]
        public async Task<ActionResult> ViewCertificate(string certificateNumber)
        {
            try
            {
                var certificate = await _certificateService.GetCertificateByNumberAsync(certificateNumber);
                if (certificate == null)
                {
                    return NotFound(new { message = "Certificate not found" });
                }

                var html = await _certificateService.GenerateCertificateHtmlAsync(certificate.Id);
                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error viewing certificate {CertificateNumber}", certificateNumber);
                return StatusCode(500, new { message = "An error occurred while loading the certificate" });
            }
        }
    }
}
