using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.Services;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Controllers.Student
{
    /// <summary>
    /// Student dashboard and profile endpoints
    /// </summary>
    [ApiController]
    [Route("api/student/dashboard")]
    [Authorize(Roles = "Student")]
    [Tags("Student - Dashboard")]
    public class StudentDashboardController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentDashboardController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        /// <summary>
        /// Get student dashboard with overview of progress, current level, and next experiment
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDashboard()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var studentId = await _studentService.GetStudentIdByUserIdAsync(userId);
            if (!studentId.HasValue)
                return NotFound(new { message = "Student profile not found" });

            var dashboard = await _studentService.GetDashboardAsync(studentId.Value);
            if (dashboard == null)
                return NotFound(new { message = "Dashboard data not available" });

            return Ok(dashboard);
        }

        /// <summary>
        /// Get student profile information
        /// </summary>
        [HttpGet("profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var studentId = await _studentService.GetStudentIdByUserIdAsync(userId);
            if (!studentId.HasValue)
                return NotFound(new { message = "Student profile not found" });

            var profile = await _studentService.GetProfileAsync(studentId.Value);
            if (profile == null)
                return NotFound(new { message = "Profile not found" });

            return Ok(profile);
        }
    }
}
