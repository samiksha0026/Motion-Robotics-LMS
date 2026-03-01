using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Progress;

[ApiController]
[Route("api/admin/progress")]
[Authorize(Roles = "SuperAdmin,SchoolAdmin")]
[Tags("Admin - Progress")]
public class AdminProgressController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AdminProgressController> _logger;
    public AdminProgressController(IMediator mediator, ILogger<AdminProgressController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<ProgressStatisticsDto>> GetStatistics([FromQuery] int? schoolId)
    {
        try { return Ok(await _mediator.Send(new GetProgressStatisticsQuery(schoolId))); }
        catch (Exception ex) { _logger.LogError(ex, "Error getting progress statistics"); return StatusCode(500, new { message = "An error occurred" }); }
    }

    [HttpGet("statistics/levels")]
    public async Task<ActionResult<List<LevelProgressStatsDto>>> GetLevelStatistics([FromQuery] int? schoolId)
    {
        try { return Ok(await _mediator.Send(new GetLevelWiseStatisticsQuery(schoolId))); }
        catch (Exception ex) { _logger.LogError(ex, "Error getting level statistics"); return StatusCode(500, new { message = "An error occurred" }); }
    }

    [HttpGet("school/{schoolId:int}")]
    public async Task<ActionResult<List<StudentProgressOverviewDto>>> GetSchoolProgress(
        int schoolId, [FromQuery] int? classId, [FromQuery] int? roboticsLevelId)
    {
        try { return Ok(await _mediator.Send(new GetSchoolProgressQuery(schoolId, classId, roboticsLevelId))); }
        catch (Exception ex) { _logger.LogError(ex, "Error getting school progress"); return StatusCode(500, new { message = "An error occurred" }); }
    }

    [HttpGet("student/{studentId:int}")]
    public async Task<ActionResult<StudentProgressOverviewDto>> GetStudentProgress(int studentId)
    {
        var progress = await _mediator.Send(new GetStudentProgressQuery(studentId));
        return progress == null ? NotFound(new { message = "Student not found" }) : Ok(progress);
    }
}
