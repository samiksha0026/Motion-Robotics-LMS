using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Controllers.Teacher
{
    [ApiController]
    [Route("api/teacher/progress")]
    [Authorize(Roles = "Teacher")]
    public class TeacherProgressController : ControllerBase
    {
        private readonly ICertificateService _certificateService;
        private readonly ILogger<TeacherProgressController> _logger;

        public TeacherProgressController(ICertificateService certificateService, ILogger<TeacherProgressController> logger)
        {
            _certificateService = certificateService;
            _logger = logger;
        }

        /// <summary>
        /// Get progress for all students in teacher's classes
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<StudentProgressOverviewDto>>> GetStudentsProgress([FromQuery] int? classId = null)
        {
            try
            {
                var teacherId = await GetTeacherIdAsync();
                if (!teacherId.HasValue)
                {
                    return Unauthorized(new { message = "Teacher not found" });
                }

                var progress = await _certificateService.GetTeacherStudentsProgressAsync(teacherId.Value, classId);
                return Ok(progress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting students progress for teacher");
                return StatusCode(500, new { message = "An error occurred while retrieving students progress" });
            }
        }

        /// <summary>
        /// Get detailed progress for a specific student
        /// </summary>
        [HttpGet("student/{studentId:int}")]
        public async Task<ActionResult<StudentProgressOverviewDto>> GetStudentProgress(int studentId)
        {
            try
            {
                var teacherId = await GetTeacherIdAsync();
                if (!teacherId.HasValue)
                {
                    return Unauthorized(new { message = "Teacher not found" });
                }

                // Verify teacher has access to this student
                var allProgress = await _certificateService.GetTeacherStudentsProgressAsync(teacherId.Value);
                if (!allProgress.Any(p => p.StudentId == studentId))
                {
                    return Forbid();
                }

                var progress = await _certificateService.GetStudentProgressAsync(studentId);
                if (progress == null)
                {
                    return NotFound(new { message = "Student not found" });
                }
                return Ok(progress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student progress {StudentId}", studentId);
                return StatusCode(500, new { message = "An error occurred while retrieving student progress" });
            }
        }

        private async Task<int?> GetTeacherIdAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }
            return await _certificateService.GetTeacherIdByUserIdAsync(userId);
        }
    }
}
