using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services;
using MotionRobotics.LMS.API.Services.Admin;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Controllers.Teacher
{
    [ApiController]
    [Route("api/teacher/attendance")]
    [Authorize(Roles = "Teacher")]
    public class TeacherAttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        private readonly ITeacherService _teacherService;

        public TeacherAttendanceController(IAttendanceService attendanceService, ITeacherService teacherService)
        {
            _attendanceService = attendanceService;
            _teacherService = teacherService;
        }

        /// <summary>
        /// Record single attendance
        /// </summary>
        [HttpPost("record")]
        public async Task<IActionResult> RecordAttendance([FromBody] AttendanceCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User ID not found in token" });

                var teacherId = await _teacherService.GetTeacherIdByUserIdAsync(userId);
                if (!teacherId.HasValue)
                    return NotFound(new { success = false, message = "Teacher profile not found" });

                var attendance = await _attendanceService.RecordAttendanceAsync(teacherId.Value, dto);
                return CreatedAtAction(nameof(GetAttendance), new { id = attendance.Id },
                    new { success = true, message = "Attendance recorded successfully", data = attendance });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Record bulk attendance for a class
        /// </summary>
        [HttpPost("bulk")]
        public async Task<IActionResult> RecordBulkAttendance([FromBody] BulkAttendanceCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User ID not found in token" });

                var teacherId = await _teacherService.GetTeacherIdByUserIdAsync(userId);
                if (!teacherId.HasValue)
                    return NotFound(new { success = false, message = "Teacher profile not found" });

                var result = await _attendanceService.RecordBulkAttendanceAsync(teacherId.Value, dto);
                return Ok(new { success = true, message = "Bulk attendance recorded successfully", data = result });
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
        /// Get all attendance for a class
        /// </summary>
        [HttpGet("class/{classId}")]
        public async Task<IActionResult> GetClassAttendance(int classId)
        {
            var attendances = await _attendanceService.GetClassAttendanceAsync(classId);
            return Ok(new { success = true, data = attendances });
        }

        /// <summary>
        /// Get class attendance for a specific date
        /// </summary>
        [HttpGet("class/{classId}/date")]
        public async Task<IActionResult> GetClassAttendanceByDate(int classId, [FromQuery] DateTime date)
        {
            var attendances = await _attendanceService.GetClassAttendanceByDateAsync(classId, date);
            return Ok(new { success = true, data = attendances });
        }

        /// <summary>
        /// Get student attendance history
        /// </summary>
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentAttendance(int studentId)
        {
            var attendances = await _attendanceService.GetStudentAttendanceAsync(studentId);
            return Ok(new { success = true, data = attendances });
        }

        /// <summary>
        /// Get student attendance report
        /// </summary>
        [HttpGet("student/{studentId}/report")]
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
        }

        /// <summary>
        /// Get class attendance report
        /// </summary>
        [HttpGet("class/{classId}/report")]
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
            catch (KeyNotFoundException)
            {
                return NotFound(new { success = false, message = "Attendance record not found" });
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
