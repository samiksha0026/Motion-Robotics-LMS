using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.Services;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Controllers.Teacher
{
    /// <summary>
    /// Teacher student progress viewing endpoints
    /// </summary>
    [ApiController]
    [Route("api/teacher/students")]
    [Authorize(Roles = "Teacher")]
    [Tags("Teacher - Students")]
    public class TeacherStudentController : ControllerBase
    {
        private readonly ITeacherService _teacherService;

        public TeacherStudentController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        /// <summary>
        /// Get detailed progress for a specific student
        /// </summary>
        [HttpGet("{studentId}/progress")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStudentProgress(int studentId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var teacherId = await _teacherService.GetTeacherIdByUserIdAsync(userId);
            if (!teacherId.HasValue)
                return NotFound(new { message = "Teacher profile not found" });

            var progress = await _teacherService.GetStudentProgressAsync(teacherId.Value, studentId);
            if (progress == null)
                return NotFound(new { message = "Student not found or you are not assigned to this student's class" });

            return Ok(progress);
        }
    }
}
