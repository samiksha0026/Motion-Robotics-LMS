using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Reports;

[ApiController]
[Route("api/admin/reports")]
[Authorize(Roles = "SuperAdmin,SchoolAdmin")]
[Tags("Admin - Reports")]
public class AdminReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AdminReportsController> _logger;
    public AdminReportsController(IMediator mediator, ILogger<AdminReportsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("student/{studentId:int}")]
    public async Task<ActionResult<ComprehensiveStudentReportDto>> GetStudentReport(int studentId)
    {
        try { return Ok(await _mediator.Send(new GetStudentReportQuery(studentId))); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error getting student report"); return StatusCode(500, new { message = "An error occurred" }); }
    }

    [HttpGet("school/{schoolId:int}")]
    public async Task<ActionResult<ComprehensiveSchoolReportDto>> GetSchoolReport(int schoolId)
    {
        try { return Ok(await _mediator.Send(new GetSchoolReportQuery(schoolId))); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error getting school report"); return StatusCode(500, new { message = "An error occurred" }); }
    }

    [HttpGet("period")]
    public async Task<ActionResult<PeriodReportDto>> GetPeriodReport(
        [FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int? schoolId)
    {
        try { return Ok(await _mediator.Send(new GetPeriodReportQuery(startDate, endDate, schoolId))); }
        catch (Exception ex) { _logger.LogError(ex, "Error getting period report"); return StatusCode(500, new { message = "An error occurred" }); }
    }

    [HttpGet("top-performers")]
    public async Task<ActionResult<List<TopPerformerDto>>> GetTopPerformers(
        [FromQuery] int? schoolId, [FromQuery] int limit = 10)
    {
        try { return Ok(await _mediator.Send(new GetTopPerformersQuery(schoolId, limit))); }
        catch (Exception ex) { _logger.LogError(ex, "Error getting top performers"); return StatusCode(500, new { message = "An error occurred" }); }
    }
}
