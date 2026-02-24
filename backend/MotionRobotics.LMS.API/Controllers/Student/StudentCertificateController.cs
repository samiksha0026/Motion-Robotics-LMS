using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.Services;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Controllers.Student
{
    /// <summary>
    /// Student certificate viewing endpoints
    /// </summary>
    [ApiController]
    [Route("api/student/certificates")]
    [Authorize(Roles = "Student")]
    [Tags("Student - Certificates")]
    public class StudentCertificateController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentCertificateController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        /// <summary>
        /// Get all certificates earned by the student
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCertificates()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var studentId = await _studentService.GetStudentIdByUserIdAsync(userId);
            if (!studentId.HasValue)
                return NotFound(new { message = "Student profile not found" });

            var certificates = await _studentService.GetCertificatesAsync(studentId.Value);
            return Ok(certificates);
        }

        /// <summary>
        /// Get details of a specific certificate
        /// </summary>
        [HttpGet("{certificateId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCertificate(int certificateId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var studentId = await _studentService.GetStudentIdByUserIdAsync(userId);
            if (!studentId.HasValue)
                return NotFound(new { message = "Student profile not found" });

            var certificate = await _studentService.GetCertificateAsync(studentId.Value, certificateId);
            if (certificate == null)
                return NotFound(new { message = "Certificate not found" });

            return Ok(certificate);
        }
    }
}
