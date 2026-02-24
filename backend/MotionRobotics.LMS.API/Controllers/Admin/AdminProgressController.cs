using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services;

namespace MotionRobotics.LMS.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/progress")]
    [Authorize(Roles = "SuperAdmin,SchoolAdmin")]
    public class AdminProgressController : ControllerBase
    {
        private readonly ICertificateService _certificateService;
        private readonly ILogger<AdminProgressController> _logger;

        public AdminProgressController(ICertificateService certificateService, ILogger<AdminProgressController> logger)
        {
            _certificateService = certificateService;
            _logger = logger;
        }

        /// <summary>
        /// Get overall progress statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<ProgressStatisticsDto>> GetStatistics([FromQuery] int? schoolId = null)
        {
            try
            {
                var stats = await _certificateService.GetProgressStatisticsAsync(schoolId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting progress statistics");
                return StatusCode(500, new { message = "An error occurred while retrieving statistics" });
            }
        }

        /// <summary>
        /// Get level-wise progress statistics
        /// </summary>
        [HttpGet("statistics/levels")]
        public async Task<ActionResult<List<LevelProgressStatsDto>>> GetLevelStatistics([FromQuery] int? schoolId = null)
        {
            try
            {
                var stats = await _certificateService.GetLevelWiseStatisticsAsync(schoolId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting level statistics");
                return StatusCode(500, new { message = "An error occurred while retrieving level statistics" });
            }
        }

        /// <summary>
        /// Get student progress by student ID
        /// </summary>
        [HttpGet("student/{studentId:int}")]
        public async Task<ActionResult<StudentProgressOverviewDto>> GetStudentProgress(int studentId)
        {
            try
            {
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

        /// <summary>
        /// Get all students progress for a school
        /// </summary>
        [HttpGet("school/{schoolId:int}")]
        public async Task<ActionResult<List<StudentProgressOverviewDto>>> GetSchoolProgress(
            int schoolId,
            [FromQuery] int? classId = null,
            [FromQuery] int? roboticsLevelId = null)
        {
            try
            {
                var progress = await _certificateService.GetSchoolProgressAsync(schoolId, classId, roboticsLevelId);
                return Ok(progress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting school progress {SchoolId}", schoolId);
                return StatusCode(500, new { message = "An error occurred while retrieving school progress" });
            }
        }
    }
}
