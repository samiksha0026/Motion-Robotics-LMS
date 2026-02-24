using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Controllers.Admin
{
    /// <summary>
    /// Read-only endpoints for robotics levels.
    /// Levels are fixed and seeded in the database.
    /// Accessible by SuperAdmin and SchoolAdmin.
    /// </summary>
    [ApiController]
    [Route("api/admin/robotics-levels")]
    [Authorize(Roles = "SuperAdmin,SchoolAdmin")]
    public class RoboticsLevelsController : ControllerBase
    {
        private readonly IRoboticsLevelService _roboticsLevelService;

        public RoboticsLevelsController(IRoboticsLevelService roboticsLevelService)
        {
            _roboticsLevelService = roboticsLevelService;
        }

        /// <summary>
        /// Get all robotics levels (6 fixed levels)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllLevels()
        {
            var levels = await _roboticsLevelService.GetAllLevelsAsync();
            return Ok(levels);
        }

        /// <summary>
        /// Get a specific level with its experiments
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLevelById(int id)
        {
            var level = await _roboticsLevelService.GetLevelWithExperimentsAsync(id);
            if (level == null)
                return NotFound(new { message = "Robotics level not found" });

            return Ok(level);
        }

        /// <summary>
        /// Update syllabus URL for a level (SuperAdmin only)
        /// </summary>
        [HttpPut("{id}/syllabus")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateSyllabusUrl(int id, [FromBody] UpdateSyllabusDto dto)
        {
            var success = await _roboticsLevelService.UpdateSyllabusUrlAsync(id, dto.SyllabusUrl);
            if (!success)
                return NotFound(new { message = "Robotics level not found" });

            return Ok(new { message = "Syllabus URL updated successfully" });
        }

        /// <summary>
        /// Seed syllabus URLs for all levels (one-time operation)
        /// </summary>
        [HttpPost("seed-syllabus")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> SeedSyllabusUrls()
        {
            await _roboticsLevelService.SeedSyllabusUrlsAsync();
            return Ok(new { message = "Syllabus URLs seeded successfully" });
        }

        /// <summary>
        /// Seed sample experiments for all levels (one-time operation)
        /// </summary>
        [HttpPost("seed-experiments")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> SeedExperiments()
        {
            await _roboticsLevelService.SeedSampleExperimentsAsync();
            return Ok(new { message = "Sample experiments seeded successfully" });
        }
    }

    public class UpdateSyllabusDto
    {
        public string SyllabusUrl { get; set; } = string.Empty;
    }
}
