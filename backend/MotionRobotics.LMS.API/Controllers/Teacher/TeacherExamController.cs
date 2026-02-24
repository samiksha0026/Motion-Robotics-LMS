using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Teacher;
using MotionRobotics.LMS.API.Services;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Controllers.Teacher
{
    [ApiController]
    [Route("api/teacher/exams")]
    [Authorize(Roles = "Teacher")]
    public class TeacherExamController : ControllerBase
    {
        private readonly IExamService _examService;
        private readonly ILogger<TeacherExamController> _logger;

        public TeacherExamController(IExamService examService, ILogger<TeacherExamController> logger)
        {
            _examService = examService;
            _logger = logger;
        }

        /// <summary>
        /// Get exam results for students in teacher's classes
        /// </summary>
        [HttpGet("results")]
        public async Task<ActionResult<TeacherExamResultsListDto>> GetExamResults(
            [FromQuery] int? classId = null,
            [FromQuery] int? examId = null)
        {
            try
            {
                var teacherId = await GetTeacherIdAsync();
                if (!teacherId.HasValue)
                {
                    return Unauthorized(new { message = "Teacher not found" });
                }

                var results = await _examService.GetTeacherExamResultsAsync(teacherId.Value, classId, examId);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exam results for teacher");
                return StatusCode(500, new { message = "An error occurred while retrieving exam results" });
            }
        }

        /// <summary>
        /// Get exam overview with class summaries
        /// </summary>
        [HttpGet("{examId:int}/overview")]
        public async Task<ActionResult<TeacherExamOverviewDto>> GetExamOverview(int examId)
        {
            try
            {
                var teacherId = await GetTeacherIdAsync();
                if (!teacherId.HasValue)
                {
                    return Unauthorized(new { message = "Teacher not found" });
                }

                var overview = await _examService.GetTeacherExamOverviewAsync(teacherId.Value, examId);
                if (overview == null)
                {
                    return NotFound(new { message = "Exam not found" });
                }
                return Ok(overview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exam overview for teacher");
                return StatusCode(500, new { message = "An error occurred while retrieving exam overview" });
            }
        }

        /// <summary>
        /// Get students' exam eligibility for a class and level
        /// </summary>
        [HttpGet("eligibility")]
        public async Task<ActionResult<List<StudentExamEligibilityDto>>> GetStudentsEligibility(
            [FromQuery] int classId,
            [FromQuery] int roboticsLevelId)
        {
            try
            {
                var teacherId = await GetTeacherIdAsync();
                if (!teacherId.HasValue)
                {
                    return Unauthorized(new { message = "Teacher not found" });
                }

                var eligibility = await _examService.GetStudentsExamEligibilityAsync(teacherId.Value, classId, roboticsLevelId);
                return Ok(eligibility);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting students eligibility");
                return StatusCode(500, new { message = "An error occurred while checking eligibility" });
            }
        }

        private async Task<int?> GetTeacherIdAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }
            return await _examService.GetTeacherIdByUserIdAsync(userId);
        }
    }
}
