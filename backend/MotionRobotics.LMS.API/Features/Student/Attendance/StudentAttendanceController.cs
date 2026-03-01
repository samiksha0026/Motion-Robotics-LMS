using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Features.Student.Attendance;

[ApiController]
[Route("api/student/attendance")]
[Authorize(Roles = "Student")]
[Tags("Student - Attendance")]
public class StudentAttendanceController : ControllerBase
{
    private readonly IMediator _mediator;
    public StudentAttendanceController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetMyAttendance()
    {
        var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(studentId) || !int.TryParse(studentId, out var parsedId))
            return Unauthorized(new { success = false, message = "Student ID not found in token" });
        var result = await _mediator.Send(new GetMyAttendanceQuery(parsedId));
        return Ok(new { success = true, data = result });
    }

    [HttpGet("report")]
    public async Task<IActionResult> GetMyAttendanceReport(
        [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studentId) || !int.TryParse(studentId, out var parsedId))
                return Unauthorized(new { success = false, message = "Student ID not found in token" });
            var report = await _mediator.Send(new GetMyAttendanceReportQuery(parsedId, startDate, endDate));
            return Ok(new { success = true, data = report });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
        catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
    }
}
