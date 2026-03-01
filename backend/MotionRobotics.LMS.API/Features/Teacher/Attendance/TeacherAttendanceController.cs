using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Features.Teacher.Attendance;

[ApiController]
[Route("api/teacher/attendance")]
[Authorize(Roles = "Teacher")]
[Tags("Teacher - Attendance")]
public class TeacherAttendanceController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TeacherAttendanceController> _logger;
    public TeacherAttendanceController(IMediator mediator, ILogger<TeacherAttendanceController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    private string? GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    [HttpPost("record")]
    public async Task<IActionResult> RecordAttendance([FromBody] AttendanceCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });
        try
        {
            var userId = GetUserId(); if (string.IsNullOrEmpty(userId)) return Unauthorized(new { success = false });
            var attendance = await _mediator.Send(new RecordAttendanceCommand(userId, dto));
            return CreatedAtAction(nameof(GetAttendance), new { id = attendance.Id },
                new { success = true, message = "Attendance recorded successfully", data = attendance });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { success = false, message = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error recording attendance"); return BadRequest(new { success = false, message = ex.Message }); }
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> RecordBulkAttendance([FromBody] BulkAttendanceCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });
        try
        {
            var userId = GetUserId(); if (string.IsNullOrEmpty(userId)) return Unauthorized(new { success = false });
            var result = await _mediator.Send(new RecordBulkAttendanceCommand(userId, dto));
            return Ok(new { success = true, message = "Bulk attendance recorded successfully", data = result });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error recording bulk attendance"); return BadRequest(new { success = false, message = ex.Message }); }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAttendance(int id)
    {
        var attendance = await _mediator.Send(new GetAttendanceByIdQuery(id));
        return attendance == null ? NotFound(new { success = false, message = "Attendance record not found" }) : Ok(new { success = true, data = attendance });
    }

    [HttpGet("class/{classId:int}")]
    public async Task<IActionResult> GetClassAttendance(int classId)
        => Ok(new { success = true, data = await _mediator.Send(new GetClassAttendanceQuery(classId)) });

    [HttpGet("class/{classId:int}/date")]
    public async Task<IActionResult> GetClassAttendanceByDate(int classId, [FromQuery] DateTime date)
        => Ok(new { success = true, data = await _mediator.Send(new GetClassAttendanceByDateQuery(classId, date)) });

    [HttpGet("student/{studentId:int}")]
    public async Task<IActionResult> GetStudentAttendance(int studentId)
        => Ok(new { success = true, data = await _mediator.Send(new GetStudentAttendanceQuery(studentId)) });

    [HttpGet("student/{studentId:int}/report")]
    public async Task<IActionResult> GetStudentAttendanceReport(int studentId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try { return Ok(new { success = true, data = await _mediator.Send(new GetStudentAttendanceReportQuery(studentId, startDate, endDate)) }); }
        catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAttendance(int id, [FromBody] AttendanceCreateDto dto)
    {
        try { return Ok(new { success = true, data = await _mediator.Send(new UpdateAttendanceCommand(id, dto)) }); }
        catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAttendance(int id)
    {
        var result = await _mediator.Send(new DeleteAttendanceCommand(id));
        return result ? Ok(new { success = true, message = "Deleted" }) : NotFound(new { success = false, message = "Not found" });
    }
}
