using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/reports")]
    [Authorize(Roles = "SuperAdmin,SchoolAdmin")]
    public class AdminReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public AdminReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        /// <summary>
        /// Get comprehensive student report including attendance, progress, exams, and certificates
        /// </summary>
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentReport(int studentId)
        {
            try
            {
                var report = await _reportService.GetStudentComprehensiveReportAsync(studentId);
                return Ok(new { success = true, data = report });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get comprehensive school report including statistics, class summaries, and top performers
        /// </summary>
        [HttpGet("school/{schoolId}")]
        public async Task<IActionResult> GetSchoolReport(int schoolId)
        {
            try
            {
                var report = await _reportService.GetSchoolComprehensiveReportAsync(schoolId);
                return Ok(new { success = true, data = report });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get period-based report (weekly, monthly, custom date range)
        /// </summary>
        [HttpGet("period")]
        public async Task<IActionResult> GetPeriodReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? schoolId)
        {
            try
            {
                if (startDate > endDate)
                    return BadRequest(new { success = false, message = "Start date must be before end date" });

                var report = await _reportService.GetPeriodReportAsync(startDate, endDate, schoolId);
                return Ok(new { success = true, data = report });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get weekly report (last 7 days)
        /// </summary>
        [HttpGet("weekly")]
        public async Task<IActionResult> GetWeeklyReport([FromQuery] int? schoolId)
        {
            try
            {
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddDays(-7);
                var report = await _reportService.GetPeriodReportAsync(startDate, endDate, schoolId);
                return Ok(new { success = true, data = report });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get monthly report (last 30 days)
        /// </summary>
        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthlyReport([FromQuery] int? schoolId)
        {
            try
            {
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddDays(-30);
                var report = await _reportService.GetPeriodReportAsync(startDate, endDate, schoolId);
                return Ok(new { success = true, data = report });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get top performers across all categories
        /// </summary>
        [HttpGet("top-performers")]
        public async Task<IActionResult> GetTopPerformers(
            [FromQuery] int? schoolId,
            [FromQuery] int limit = 10)
        {
            try
            {
                if (limit < 1 || limit > 100)
                    limit = 10;

                var performers = await _reportService.GetTopPerformersAsync(schoolId, limit);
                return Ok(new { success = true, data = performers });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
