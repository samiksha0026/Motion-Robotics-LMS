using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MotionRobotics.LMS.API.Features.Admin.Attendance;

[ApiController]
[Route("api/admin/attendance")]
[Authorize(Roles = "SuperAdmin,SchoolAdmin")]
[Tags("Admin - Attendance")]
public class AdminAttendanceController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminAttendanceController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAllAttendance(
        [FromQuery] int? schoolId, [FromQuery] int? classId,
        [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var data = await _mediator.Send(new GetAllAttendanceQuery(schoolId, classId, startDate, endDate));
            return Ok(new { success = true, data });
        }
        catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAttendance(int id)
    {
        var record = await _mediator.Send(new GetAttendanceByIdQuery(id));
        return record == null
            ? NotFound(new { success = false, message = "Attendance record not found" })
            : Ok(new { success = true, data = record });
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetAttendanceSummary(
        [FromQuery] int? schoolId, [FromQuery] int? classId,
        [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var data = await _mediator.Send(new GetAttendanceSummaryQuery(schoolId, classId, startDate, endDate));
        return Ok(new { success = true, data });
    }

    [HttpGet("student/{studentId:int}")]
    public async Task<IActionResult> GetStudentAttendanceReport(int studentId,
        [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var data = await _mediator.Send(new GetStudentAttendanceReportQuery(studentId, startDate, endDate));
        return Ok(new { success = true, data });
    }

    [HttpGet("class/{classId:int}")]
    public async Task<IActionResult> GetClassAttendanceReport(int classId,
        [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var data = await _mediator.Send(new GetClassAttendanceReportQuery(classId, startDate, endDate));
        return Ok(new { success = true, data });
    }

    [HttpGet("school/{schoolId:int}")]
    public async Task<IActionResult> GetSchoolAttendanceReport(int schoolId,
        [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var data = await _mediator.Send(new GetSchoolAttendanceReportQuery(schoolId, startDate, endDate));
        return Ok(new { success = true, data });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAttendance(int id)
    {
        var result = await _mediator.Send(new DeleteAttendanceCommand(id));
        return result
            ? Ok(new { success = true, message = "Attendance record deleted" })
            : NotFound(new { success = false, message = "Attendance record not found" });
    }
}
