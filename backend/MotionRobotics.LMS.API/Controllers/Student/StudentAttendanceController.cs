using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.Services.Admin;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Controllers.Student
{
    [ApiController]
    [Route("api/student/attendance")]
    [Authorize(Roles = "Student")]
    public class StudentAttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public StudentAttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        /// <summary>
        /// Get current student's attendance history
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyAttendance()
        {
            try
            {
                var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentId) || !int.TryParse(studentId, out var parsedStudentId))
                    return Unauthorized(new { success = false, message = "Student ID not found in token" });

                var attendances = await _attendanceService.GetStudentAttendanceAsync(parsedStudentId);
                return Ok(new { success = true, data = attendances });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get current student's attendance report
        /// </summary>
        [HttpGet("report")]
        public async Task<IActionResult> GetMyAttendanceReport(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentId) || !int.TryParse(studentId, out var parsedStudentId))
                    return Unauthorized(new { success = false, message = "Student ID not found in token" });

                var report = await _attendanceService.GetStudentAttendanceReportAsync(parsedStudentId, startDate, endDate);
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
        /// Get current student's attendance summary
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetMyAttendanceSummary()
        {
            try
            {
                var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentId) || !int.TryParse(studentId, out var parsedStudentId))
                    return Unauthorized(new { success = false, message = "Student ID not found in token" });

                var report = await _attendanceService.GetStudentAttendanceReportAsync(parsedStudentId, null, null);

                var summary = new
                {
                    TotalDays = report.TotalDays,
                    PresentDays = report.PresentDays,
                    AbsentDays = report.AbsentDays,
                    AttendancePercentage = report.AttendancePercentage,
                    Grade = GetAttendanceGrade(report.AttendancePercentage)
                };

                return Ok(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        private static string GetAttendanceGrade(double percentage)
        {
            return percentage switch
            {
                >= 95 => "Excellent",
                >= 85 => "Very Good",
                >= 75 => "Good",
                >= 60 => "Satisfactory",
                >= 50 => "Needs Improvement",
                _ => "Poor"
            };
        }
    }
}
