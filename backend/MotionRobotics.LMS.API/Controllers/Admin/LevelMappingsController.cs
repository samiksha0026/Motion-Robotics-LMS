using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Controllers.Admin
{
    /// <summary>
    /// SuperAdmin endpoints for managing school-level mappings.
    /// Maps which class gets which robotics level for each academic year.
    /// </summary>
    [ApiController]
    [Route("api/admin/level-mappings")]
    [Authorize(Roles = "SuperAdmin")]
    public class LevelMappingsController : ControllerBase
    {
        private readonly ILevelMappingService _levelMappingService;

        public LevelMappingsController(ILevelMappingService levelMappingService)
        {
            _levelMappingService = levelMappingService;
        }

        /// <summary>
        /// Get all level mappings, optionally filtered by school or academic year
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllMappings(
            [FromQuery] int? schoolId = null,
            [FromQuery] int? academicYearId = null)
        {
            var mappings = await _levelMappingService.GetAllMappingsAsync(schoolId, academicYearId);
            return Ok(mappings);
        }

        /// <summary>
        /// Get a specific mapping by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMappingById(int id)
        {
            var mapping = await _levelMappingService.GetMappingByIdAsync(id);
            if (mapping == null)
                return NotFound(new { message = "Mapping not found" });

            return Ok(mapping);
        }

        /// <summary>
        /// Get all level assignments for a specific school in an academic year
        /// Returns mappings and available classes for assignment
        /// </summary>
        [HttpGet("school/{schoolId}/year/{academicYearId}")]
        public async Task<IActionResult> GetSchoolLevelAssignments(int schoolId, int academicYearId)
        {
            var result = await _levelMappingService.GetSchoolLevelAssignmentsAsync(schoolId, academicYearId);
            if (result == null)
                return NotFound(new { message = "School or academic year not found" });

            return Ok(result);
        }

        /// <summary>
        /// Create a single level mapping
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateMapping([FromBody] LevelMappingCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var mapping = await _levelMappingService.CreateMappingAsync(dto);
                return CreatedAtAction(nameof(GetMappingById), new { id = mapping.Id }, mapping);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create multiple level mappings at once for a school
        /// </summary>
        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulkMappings([FromBody] BulkLevelMappingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var mappings = await _levelMappingService.CreateBulkMappingsAsync(dto);
                return Ok(new
                {
                    message = $"Successfully created {mappings.Count} mappings",
                    mappings
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update a mapping's robotics level
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMapping(int id, [FromBody] LevelMappingUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var mapping = await _levelMappingService.UpdateMappingAsync(id, dto);
                return Ok(mapping);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a specific mapping
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMapping(int id)
        {
            try
            {
                var success = await _levelMappingService.DeleteMappingAsync(id);
                if (!success)
                    return NotFound(new { message = "Mapping not found" });

                return Ok(new { message = "Mapping deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete all mappings for a school in a specific academic year
        /// </summary>
        [HttpDelete("school/{schoolId}/year/{academicYearId}")]
        public async Task<IActionResult> DeleteSchoolMappings(int schoolId, int academicYearId)
        {
            try
            {
                var success = await _levelMappingService.DeleteSchoolMappingsAsync(schoolId, academicYearId);
                if (!success)
                    return NotFound(new { message = "No mappings found for this school and year" });

                return Ok(new { message = "All mappings deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
