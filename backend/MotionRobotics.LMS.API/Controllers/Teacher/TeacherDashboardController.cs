using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.Services;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Controllers.Teacher
{
    /// <summary>
    /// Teacher dashboard and profile endpoints
    /// </summary>
    [ApiController]
    [Route("api/teacher/dashboard")]
    [Authorize(Roles = "Teacher")]
    [Tags("Teacher - Dashboard")]
    public class TeacherDashboardController : ControllerBase
    {
        private readonly ITeacherService _teacherService;

        public TeacherDashboardController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        /// <summary>
        /// Get teacher dashboard with overview of classes, students, and pending approvals
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

            var teacherId = await _teacherService.GetTeacherIdByUserIdAsync(userId);
            if (!teacherId.HasValue)
                return NotFound(new { message = "Teacher profile not found" });

            var dashboard = await _teacherService.GetDashboardAsync(teacherId.Value);
            if (dashboard == null)
                return NotFound(new { message = "Dashboard data not available" });

            return Ok(dashboard);
        }

        /// <summary>
        /// Get list of classes assigned to the teacher
        /// </summary>
        [HttpGet("classes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAssignedClasses()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var teacherId = await _teacherService.GetTeacherIdByUserIdAsync(userId);
            if (!teacherId.HasValue)
                return NotFound(new { message = "Teacher profile not found" });

            var classes = await _teacherService.GetAssignedClassesAsync(teacherId.Value);
            return Ok(classes);
        }
    }
}
