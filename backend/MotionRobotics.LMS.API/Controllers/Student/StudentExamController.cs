using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Student;
using MotionRobotics.LMS.API.Services;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Controllers.Student
{
    [ApiController]
    [Route("api/student/exam")]
    [Authorize(Roles = "Student")]
    public class StudentExamController : ControllerBase
    {
        private readonly IExamService _examService;
        private readonly ILogger<StudentExamController> _logger;

        public StudentExamController(IExamService examService, ILogger<StudentExamController> logger)
        {
            _examService = examService;
            _logger = logger;
        }

        /// <summary>
        /// Check if student is eligible to take the exam
        /// </summary>
        [HttpGet("eligibility")]
        public async Task<ActionResult<ExamEligibilityDto>> CheckEligibility()
        {
            try
            {
                var studentId = await GetStudentIdAsync();
                if (!studentId.HasValue)
                {
                    return Unauthorized(new { message = "Student not found" });
                }

                var eligibility = await _examService.CheckExamEligibilityAsync(studentId.Value);
                return Ok(eligibility);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking exam eligibility");
                return StatusCode(500, new { message = "An error occurred while checking eligibility" });
            }
        }

        /// <summary>
        /// Start the exam and get questions
        /// </summary>
        [HttpPost("start")]
        public async Task<ActionResult<ExamQuestionsDto>> StartExam()
        {
            try
            {
                var studentId = await GetStudentIdAsync();
                if (!studentId.HasValue)
                {
                    return Unauthorized(new { message = "Student not found" });
                }

                var exam = await _examService.StartExamAsync(studentId.Value);
                if (exam == null)
                {
                    return BadRequest(new { message = "You are not eligible to take this exam. Complete all experiments first." });
                }
                return Ok(exam);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting exam");
                return StatusCode(500, new { message = "An error occurred while starting the exam" });
            }
        }

        /// <summary>
        /// Submit exam answers
        /// </summary>
        [HttpPost("submit")]
        public async Task<ActionResult<StudentExamResultDto>> SubmitExam([FromBody] ExamAnswerSubmitDto dto)
        {
            try
            {
                var studentId = await GetStudentIdAsync();
                if (!studentId.HasValue)
                {
                    return Unauthorized(new { message = "Student not found" });
                }

                var result = await _examService.SubmitExamAsync(studentId.Value, dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting exam");
                return StatusCode(500, new { message = "An error occurred while submitting the exam" });
            }
        }

        /// <summary>
        /// Get exam history
        /// </summary>
        [HttpGet("history")]
        public async Task<ActionResult<ExamHistoryDto>> GetExamHistory()
        {
            try
            {
                var studentId = await GetStudentIdAsync();
                if (!studentId.HasValue)
                {
                    return Unauthorized(new { message = "Student not found" });
                }

                var history = await _examService.GetExamHistoryAsync(studentId.Value);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exam history");
                return StatusCode(500, new { message = "An error occurred while retrieving exam history" });
            }
        }

        /// <summary>
        /// Get specific exam result with answers
        /// </summary>
        [HttpGet("results/{resultId:int}")]
        public async Task<ActionResult<StudentExamResultDto>> GetExamResult(int resultId)
        {
            try
            {
                var studentId = await GetStudentIdAsync();
                if (!studentId.HasValue)
                {
                    return Unauthorized(new { message = "Student not found" });
                }

                var result = await _examService.GetExamResultAsync(studentId.Value, resultId);
                if (result == null)
                {
                    return NotFound(new { message = "Exam result not found" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exam result");
                return StatusCode(500, new { message = "An error occurred while retrieving the exam result" });
            }
        }

        private async Task<int?> GetStudentIdAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }
            return await _examService.GetStudentIdByUserIdAsync(userId);
        }
    }
}
