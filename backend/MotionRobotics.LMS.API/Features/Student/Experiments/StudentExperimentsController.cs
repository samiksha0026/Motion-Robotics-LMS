using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Student;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Features.Student.Experiments;

[ApiController]
[Route("api/student/experiments")]
[Authorize(Roles = "Student")]
[Tags("Student - Experiments")]
public class StudentExperimentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StudentExperimentsController> _logger;
    public StudentExperimentsController(IMediator mediator, ILogger<StudentExperimentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetExperiments()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var result = await _mediator.Send(new GetStudentExperimentsQuery(userId));
        return result == null ? NotFound(new { message = "No experiments found" }) : Ok(result);
    }

    [HttpGet("{experimentId:int}")]
    public async Task<IActionResult> GetExperimentDetail(int experimentId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var result = await _mediator.Send(new GetStudentExperimentDetailQuery(userId, experimentId));
        return result == null ? NotFound(new { message = "Experiment not found" }) : Ok(result);
    }

    [HttpPost("{experimentId:int}/submit")]
    public async Task<IActionResult> SubmitExperiment(int experimentId, [FromBody] ExperimentSubmissionDto dto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var (success, message, data) = await _mediator.Send(new SubmitStudentExperimentCommand(userId, experimentId, dto));
            return success ? Ok(new { message, data }) : BadRequest(new { message });
        }
        catch (Exception ex) { _logger.LogError(ex, "Error submitting experiment"); return StatusCode(500, new { message = "An error occurred" }); }
    }
}
