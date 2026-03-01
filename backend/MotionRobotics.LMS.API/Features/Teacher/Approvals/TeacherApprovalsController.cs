using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Teacher;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Features.Teacher.Approvals;

[ApiController]
[Route("api/teacher/approvals")]
[Authorize(Roles = "Teacher")]
[Tags("Teacher - Approvals")]
public class TeacherApprovalsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TeacherApprovalsController> _logger;
    public TeacherApprovalsController(IMediator mediator, ILogger<TeacherApprovalsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    private string? GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    [HttpGet("pending")]
    public async Task<ActionResult<List<PendingApprovalDto>>> GetPendingApprovals([FromQuery] int? classId)
    {
        try
        {
            var userId = GetUserId(); if (string.IsNullOrEmpty(userId)) return Unauthorized();
            return Ok(await _mediator.Send(new GetPendingApprovalsQuery(userId, classId)));
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error getting pending approvals"); return StatusCode(500, new { message = "An error occurred" }); }
    }

    [HttpPost("{progressId:int}/approve")]
    public async Task<IActionResult> ApproveProgress(int progressId, [FromBody] ApprovalRequestDto dto)
    {
        try
        {
            var userId = GetUserId(); if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var (success, message) = await _mediator.Send(new ApproveProgressCommand(userId, progressId, dto));
            return success ? Ok(new { message }) : BadRequest(new { message });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error approving progress"); return StatusCode(500, new { message = "An error occurred" }); }
    }

    [HttpPost("bulk-approve")]
    public async Task<IActionResult> BulkApproveProgress([FromBody] BulkApprovalRequestDto dto)
    {
        try
        {
            var userId = GetUserId(); if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var (success, message, processed) = await _mediator.Send(new BulkApproveProgressCommand(userId, dto));
            return success ? Ok(new { message, processed }) : BadRequest(new { message });
        }
        catch (Exception ex) { _logger.LogError(ex, "Error bulk approving progress"); return StatusCode(500, new { message = "An error occurred" }); }
    }
}
