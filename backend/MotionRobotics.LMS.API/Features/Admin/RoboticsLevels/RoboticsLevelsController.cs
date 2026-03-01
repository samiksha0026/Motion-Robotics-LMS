using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.RoboticsLevels;

public record UpdateSyllabusRequest(string SyllabusUrl);

[ApiController]
[Route("api/admin/robotics-levels")]
[Authorize(Roles = "SuperAdmin,SchoolAdmin")]
[Tags("Admin - Robotics Levels")]
public class RoboticsLevelsController : ControllerBase
{
    private readonly IMediator _mediator;
    public RoboticsLevelsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAllLevels()
        => Ok(await _mediator.Send(new GetAllRoboticsLevelsQuery()));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetLevelById(int id)
    {
        var level = await _mediator.Send(new GetRoboticsLevelByIdQuery(id));
        return level == null ? NotFound(new { message = "Robotics level not found" }) : Ok(level);
    }

    [HttpPut("{id:int}/syllabus")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateSyllabusUrl(int id, [FromBody] UpdateSyllabusRequest dto)
    {
        var success = await _mediator.Send(new UpdateSyllabusUrlCommand(id, dto.SyllabusUrl));
        return success ? Ok(new { message = "Syllabus URL updated successfully" }) : NotFound(new { message = "Robotics level not found" });
    }

    [HttpPost("seed-syllabus")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> SeedSyllabusUrls()
    {
        await _mediator.Send(new SeedSyllabusUrlsCommand());
        return Ok(new { message = "Syllabus URLs seeded successfully" });
    }

    [HttpPost("seed-experiments")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> SeedExperiments()
    {
        await _mediator.Send(new SeedSampleExperimentsCommand());
        return Ok(new { message = "Sample experiments seeded successfully" });
    }
}
