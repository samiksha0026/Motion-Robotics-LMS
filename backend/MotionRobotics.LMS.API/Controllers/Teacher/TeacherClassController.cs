using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.Services;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Controllers.Teacher
{
    /// <summary>
    /// Teacher class management endpoints
    /// </summary>
    [ApiController]
    [Route("api/teacher/classes")]
    [Authorize(Roles = "Teacher")]
    [Tags("Teacher - Classes")]
    public class TeacherClassController : ControllerBase
    {
        private readonly ITeacherService _teacherService;

        public TeacherClassController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        /// <summary>
        /// Get detailed information about a specific class
        /// </summary>
        [HttpGet("{classId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetClassDetail(int classId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var teacherId = await _teacherService.GetTeacherIdByUserIdAsync(userId);
            if (!teacherId.HasValue)
                return NotFound(new { message = "Teacher profile not found" });

            var classDetail = await _teacherService.GetClassDetailAsync(teacherId.Value, classId);
            if (classDetail == null)
                return NotFound(new { message = "Class not found or you are not assigned to this class" });

            return Ok(classDetail);
        }

        /// <summary>
        /// Get list of students in a specific class
        /// </summary>
        [HttpGet("{classId}/students")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetClassStudents(int classId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var teacherId = await _teacherService.GetTeacherIdByUserIdAsync(userId);
            if (!teacherId.HasValue)
                return NotFound(new { message = "Teacher profile not found" });

            if (!await _teacherService.IsTeacherAssignedToClass(teacherId.Value, classId))
                return Forbid();

            var students = await _teacherService.GetClassStudentsAsync(teacherId.Value, classId);
            return Ok(students);
        }
    }
}
