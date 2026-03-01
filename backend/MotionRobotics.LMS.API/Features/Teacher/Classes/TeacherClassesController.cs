using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Features.Teacher.Classes;

[ApiController]
[Route("api/teacher/classes")]
[Authorize(Roles = "Teacher")]
[Tags("Teacher - Classes")]
public class TeacherClassesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TeacherClassesController> _logger;
    public TeacherClassesController(IMediator mediator, ILogger<TeacherClassesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    private string? GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    [HttpGet("{classId:int}")]
    public async Task<IActionResult> GetClassDetail(int classId)
    {
        try
        {
            var userId = GetUserId(); if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var result = await _mediator.Send(new GetClassDetailQuery(userId, classId));
            return result == null ? NotFound(new { message = "Class not found or not assigned to you" }) : Ok(result);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error getting class detail"); return StatusCode(500, new { message = "An error occurred" }); }
    }

    [HttpGet("{classId:int}/students")]
    public async Task<IActionResult> GetClassStudents(int classId)
    {
        try
        {
            var userId = GetUserId(); if (string.IsNullOrEmpty(userId)) return Unauthorized();
            return Ok(await _mediator.Send(new GetClassStudentsQuery(userId, classId)));
        }
        catch (Exception ex) { _logger.LogError(ex, "Error getting class students"); return StatusCode(500, new { message = "An error occurred" }); }
    }
}
