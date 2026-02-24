using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Teacher;
using MotionRobotics.LMS.API.Services;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Controllers.Teacher
{
    /// <summary>
    /// Teacher approval workflow endpoints - approve/reject student experiment completions
    /// </summary>
    [ApiController]
    [Route("api/teacher/approvals")]
    [Authorize(Roles = "Teacher")]
    [Tags("Teacher - Approvals")]
    public class TeacherApprovalController : ControllerBase
    {
        private readonly ITeacherService _teacherService;

        public TeacherApprovalController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        /// <summary>
        /// Get all pending approvals for the teacher
        /// </summary>
        [HttpGet("pending")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPendingApprovals([FromQuery] int? classId = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var teacherId = await _teacherService.GetTeacherIdByUserIdAsync(userId);
            if (!teacherId.HasValue)
                return NotFound(new { message = "Teacher profile not found" });

            var pendingApprovals = await _teacherService.GetPendingApprovalsAsync(teacherId.Value, classId);
            return Ok(pendingApprovals);
        }

        /// <summary>
        /// Approve or reject a single student progress entry
        /// </summary>
        [HttpPost("{progressId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApproveProgress(int progressId, [FromBody] ApprovalRequestDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var teacherId = await _teacherService.GetTeacherIdByUserIdAsync(userId);
            if (!teacherId.HasValue)
                return NotFound(new { message = "Teacher profile not found" });

            var (success, message) = await _teacherService.ApproveProgressAsync(teacherId.Value, progressId, request);

            if (!success)
            {
                if (message.Contains("not found"))
                    return NotFound(new { message });
                if (message.Contains("Not authorized"))
                    return Forbid();
                return BadRequest(new { message });
            }

            return Ok(new { message });
        }

        /// <summary>
        /// Bulk approve or reject multiple student progress entries
        /// </summary>
        [HttpPost("bulk")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> BulkApprove([FromBody] BulkApprovalRequestDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var teacherId = await _teacherService.GetTeacherIdByUserIdAsync(userId);
            if (!teacherId.HasValue)
                return NotFound(new { message = "Teacher profile not found" });

            var (success, message, processed) = await _teacherService.BulkApproveProgressAsync(teacherId.Value, request);

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message, processed, total = request.ProgressIds.Count });
        }
    }
}
