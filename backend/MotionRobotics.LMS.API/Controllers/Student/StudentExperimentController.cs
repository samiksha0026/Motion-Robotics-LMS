using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Student;
using MotionRobotics.LMS.API.Services;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Controllers.Student
{
    /// <summary>
    /// Student experiment viewing and submission endpoints
    /// </summary>
    [ApiController]
    [Route("api/student/experiments")]
    [Authorize(Roles = "Student")]
    [Tags("Student - Experiments")]
    public class StudentExperimentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentExperimentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        /// <summary>
        /// Get all experiments for the student's current level with status
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetExperiments()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var studentId = await _studentService.GetStudentIdByUserIdAsync(userId);
            if (!studentId.HasValue)
                return NotFound(new { message = "Student profile not found" });

            var experiments = await _studentService.GetExperimentsAsync(studentId.Value);
            return Ok(experiments);
        }

        /// <summary>
        /// Get detailed information about a specific experiment
        /// </summary>
        [HttpGet("{experimentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetExperimentDetail(int experimentId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var studentId = await _studentService.GetStudentIdByUserIdAsync(userId);
            if (!studentId.HasValue)
                return NotFound(new { message = "Student profile not found" });

            var experiment = await _studentService.GetExperimentDetailAsync(studentId.Value, experimentId);
            if (experiment == null)
                return NotFound(new { message = "Experiment not found" });

            return Ok(experiment);
        }

        /// <summary>
        /// Submit completion for an experiment (marks as completed, pending teacher approval)
        /// </summary>
        [HttpPost("{experimentId}/submit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SubmitExperiment(int experimentId, [FromBody] ExperimentSubmissionDto submission)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var studentId = await _studentService.GetStudentIdByUserIdAsync(userId);
            if (!studentId.HasValue)
                return NotFound(new { message = "Student profile not found" });

            var (success, message, data) = await _studentService.SubmitExperimentAsync(studentId.Value, experimentId, submission);

            if (!success)
                return BadRequest(new { message });

            return Ok(data);
        }
    }
}
