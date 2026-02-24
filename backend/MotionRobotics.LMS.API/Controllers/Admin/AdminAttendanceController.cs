using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/attendance")]
    [Authorize(Roles = "SuperAdmin,SchoolAdmin")]
    public class AdminAttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AdminAttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        /// <summary>
        /// Get all attendance records with optional filters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllAttendance(
            [FromQuery] int? schoolId,
            [FromQuery] int? classId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var attendances = await _attendanceService.GetAllAttendanceAsync(schoolId, classId, startDate, endDate);
                return Ok(new { success = true, data = attendances });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get attendance by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAttendance(int id)
        {
            var attendance = await _attendanceService.GetAttendanceByIdAsync(id);
            if (attendance == null)
                return NotFound(new { success = false, message = "Attendance record not found" });

            return Ok(new { success = true, data = attendance });
        }

        /// <summary>
        /// Get attendance summary with optional filters
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetAttendanceSummary(
            [FromQuery] int? schoolId,
            [FromQuery] int? classId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var summary = await _attendanceService.GetAttendanceSummaryAsync(schoolId, classId, startDate, endDate);
                return Ok(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get student attendance report
        /// </summary>
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentAttendanceReport(
            int studentId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var report = await _attendanceService.GetStudentAttendanceReportAsync(studentId, startDate, endDate);
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
        /// Get class attendance report
        /// </summary>
        [HttpGet("class/{classId}")]
        public async Task<IActionResult> GetClassAttendanceReport(
            int classId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var report = await _attendanceService.GetClassAttendanceReportAsync(classId, startDate, endDate);
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
        /// Get school attendance report
        /// </summary>
        [HttpGet("school/{schoolId}")]
        public async Task<IActionResult> GetSchoolAttendanceReport(
            int schoolId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var report = await _attendanceService.GetSchoolAttendanceReportAsync(schoolId, startDate, endDate);
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
        /// Update attendance record
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAttendance(int id, [FromBody] AttendanceCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

            try
            {
                var attendance = await _attendanceService.UpdateAttendanceAsync(id, dto);
                return Ok(new { success = true, message = "Attendance updated successfully", data = attendance });
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
        /// Delete attendance record
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttendance(int id)
        {
            var result = await _attendanceService.DeleteAttendanceAsync(id);
            if (!result)
                return NotFound(new { success = false, message = "Attendance record not found" });

            return Ok(new { success = true, message = "Attendance record deleted successfully" });
        }
    }
}
