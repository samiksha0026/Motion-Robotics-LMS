using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services;

namespace MotionRobotics.LMS.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/exams")]
    [Authorize(Roles = "SuperAdmin,SchoolAdmin")]
    public class AdminExamsController : ControllerBase
    {
        private readonly IExamService _examService;
        private readonly ILogger<AdminExamsController> _logger;

        public AdminExamsController(IExamService examService, ILogger<AdminExamsController> logger)
        {
            _examService = examService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new exam for a robotics level
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ExamDetailDto>> CreateExam([FromBody] ExamCreateDto dto)
        {
            try
            {
                if (dto.Questions.Count == 0)
                {
                    return BadRequest(new { message = "Exam must have at least one question" });
                }

                var exam = await _examService.CreateExamAsync(dto);
                return CreatedAtAction(nameof(GetExam), new { examId = exam.Id }, exam);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating exam");
                return StatusCode(500, new { message = "An error occurred while creating the exam" });
            }
        }

        /// <summary>
        /// Get all exams
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ExamListDto>> GetAllExams(
            [FromQuery] int? roboticsLevelId = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var exams = await _examService.GetAllExamsAsync(roboticsLevelId, isActive);
                return Ok(exams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exams");
                return StatusCode(500, new { message = "An error occurred while retrieving exams" });
            }
        }

        /// <summary>
        /// Get exam by ID with all questions
        /// </summary>
        [HttpGet("{examId:int}")]
        public async Task<ActionResult<ExamDetailDto>> GetExam(int examId)
        {
            try
            {
                var exam = await _examService.GetExamByIdAsync(examId);
                if (exam == null)
                {
                    return NotFound(new { message = "Exam not found" });
                }
                return Ok(exam);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exam {ExamId}", examId);
                return StatusCode(500, new { message = "An error occurred while retrieving the exam" });
            }
        }

        /// <summary>
        /// Update an exam
        /// </summary>
        [HttpPut("{examId:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ExamDetailDto>> UpdateExam(int examId, [FromBody] ExamUpdateDto dto)
        {
            try
            {
                var exam = await _examService.UpdateExamAsync(examId, dto);
                if (exam == null)
                {
                    return NotFound(new { message = "Exam not found" });
                }
                return Ok(exam);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating exam {ExamId}", examId);
                return StatusCode(500, new { message = "An error occurred while updating the exam" });
            }
        }

        /// <summary>
        /// Delete (or deactivate) an exam
        /// </summary>
        [HttpDelete("{examId:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult> DeleteExam(int examId)
        {
            try
            {
                var result = await _examService.DeleteExamAsync(examId);
                if (!result)
                {
                    return NotFound(new { message = "Exam not found" });
                }
                return Ok(new { message = "Exam deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting exam {ExamId}", examId);
                return StatusCode(500, new { message = "An error occurred while deleting the exam" });
            }
        }

        /// <summary>
        /// Get exam results
        /// </summary>
        [HttpGet("{examId:int}/results")]
        public async Task<ActionResult<ExamResultsListDto>> GetExamResults(
            int examId,
            [FromQuery] int? schoolId = null,
            [FromQuery] int? classId = null)
        {
            try
            {
                var results = await _examService.GetExamResultsAsync(examId, schoolId, classId);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exam results for {ExamId}", examId);
                return StatusCode(500, new { message = "An error occurred while retrieving exam results" });
            }
        }

        /// <summary>
        /// Get exam statistics
        /// </summary>
        [HttpGet("{examId:int}/statistics")]
        public async Task<ActionResult<ExamStatisticsDto>> GetExamStatistics(int examId)
        {
            try
            {
                var stats = await _examService.GetExamStatisticsAsync(examId);
                if (stats == null)
                {
                    return NotFound(new { message = "Exam not found" });
                }
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exam statistics for {ExamId}", examId);
                return StatusCode(500, new { message = "An error occurred while retrieving exam statistics" });
            }
        }
    }
}
