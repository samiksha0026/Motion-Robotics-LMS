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

    // Helper to get session school context for multi-tenant isolation
    private (string? Role, int? SchoolId) GetSessionContext()
    {
        var role = HttpContext.Items["SessionRole"] as string;
        var schoolId = HttpContext.Items["SessionSchoolId"] as int?;
        return (role, schoolId);
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<ProgressStatisticsDto>> GetStatistics([FromQuery] int? schoolId)
    {
        // Multi-tenant: SchoolAdmin can only see their school's statistics
        var (role, sessionSchoolId) = GetSessionContext();
        if (role == "SchoolAdmin" && sessionSchoolId.HasValue)
        {
            if (schoolId.HasValue && schoolId.Value != sessionSchoolId.Value)
                return Forbid();
            schoolId = sessionSchoolId;
        }

        try { return Ok(await _mediator.Send(new GetProgressStatisticsQuery(schoolId))); }
        catch (Exception ex) { _logger.LogError(ex, "Error getting progress statistics"); return StatusCode(500, new { message = "An error occurred" }); }
    }

    [HttpGet("statistics/levels")]
    public async Task<ActionResult<List<LevelProgressStatsDto>>> GetLevelStatistics([FromQuery] int? schoolId)
    {
        // Multi-tenant: SchoolAdmin can only see their school's statistics
        var (role, sessionSchoolId) = GetSessionContext();
        if (role == "SchoolAdmin" && sessionSchoolId.HasValue)
        {
            if (schoolId.HasValue && schoolId.Value != sessionSchoolId.Value)
                return Forbid();
            schoolId = sessionSchoolId;
        }

        try { return Ok(await _mediator.Send(new GetLevelWiseStatisticsQuery(schoolId))); }
        catch (Exception ex) { _logger.LogError(ex, "Error getting level statistics"); return StatusCode(500, new { message = "An error occurred" }); }
    }

    [HttpGet("school/{schoolId:int}")]
    public async Task<ActionResult<List<StudentProgressOverviewDto>>> GetSchoolProgress(
        int schoolId, [FromQuery] int? classId, [FromQuery] int? roboticsLevelId)
    {
        // Multi-tenant: SchoolAdmin can only access their own school
        var (role, sessionSchoolId) = GetSessionContext();
        if (role == "SchoolAdmin" && sessionSchoolId.HasValue && schoolId != sessionSchoolId.Value)
            return Forbid();

        try { return Ok(await _mediator.Send(new GetSchoolProgressQuery(schoolId, classId, roboticsLevelId))); }
        catch (Exception ex) { _logger.LogError(ex, "Error getting school progress"); return StatusCode(500, new { message = "An error occurred" }); }
    }

    [HttpGet("student/{studentId:int}")]
    public async Task<ActionResult<StudentProgressOverviewDto>> GetStudentProgress(int studentId)
    {
        var progress = await _mediator.Send(new GetStudentProgressQuery(studentId));
        if (progress == null) return NotFound(new { message = "Student not found" });

        // Multi-tenant: SchoolAdmin can only access their school's students
        var (role, sessionSchoolId) = GetSessionContext();
        if (role == "SchoolAdmin" && sessionSchoolId.HasValue && progress.SchoolId != sessionSchoolId.Value)
            return Forbid();

        return Ok(progress);
    }
}
