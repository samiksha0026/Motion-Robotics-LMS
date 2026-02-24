using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Controllers.Admin
{
    /// <summary>
    /// SuperAdmin-only endpoints for managing academic years.
    /// </summary>
    [ApiController]
    [Route("api/admin/academic-years")]
    [Authorize(Roles = "SuperAdmin")]
    public class AcademicYearsController : ControllerBase
    {
        private readonly IAcademicYearService _academicYearService;

        public AcademicYearsController(IAcademicYearService academicYearService)
        {
            _academicYearService = academicYearService;
        }

        /// <summary>
        /// Get all academic years
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "SuperAdmin,SchoolAdmin")]
        public async Task<IActionResult> GetAllAcademicYears()
        {
            var years = await _academicYearService.GetAllAcademicYearsAsync();
            return Ok(years);
        }

        /// <summary>
        /// Get current academic year
        /// </summary>
        [HttpGet("current")]
        [Authorize(Roles = "SuperAdmin,SchoolAdmin,Teacher")]
        public async Task<IActionResult> GetCurrentAcademicYear()
        {
            var year = await _academicYearService.GetCurrentAcademicYearAsync();
            if (year == null)
                return NotFound(new { message = "No current academic year set" });

            return Ok(year);
        }

        /// <summary>
        /// Get academic year by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,SchoolAdmin")]
        public async Task<IActionResult> GetAcademicYearById(int id)
        {
            var year = await _academicYearService.GetAcademicYearByIdAsync(id);
            if (year == null)
                return NotFound(new { message = "Academic year not found" });

            return Ok(year);
        }

        /// <summary>
        /// Create a new academic year
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAcademicYear([FromBody] AcademicYearCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var year = await _academicYearService.CreateAcademicYearAsync(dto);
                return CreatedAtAction(nameof(GetAcademicYearById), new { id = year.Id }, year);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update an academic year
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAcademicYear(int id, [FromBody] AcademicYearCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var year = await _academicYearService.UpdateAcademicYearAsync(id, dto);
                return Ok(year);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Academic year not found" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Set an academic year as current
        /// </summary>
        [HttpPatch("{id}/set-current")]
        public async Task<IActionResult> SetCurrentAcademicYear(int id)
        {
            var success = await _academicYearService.SetCurrentAcademicYearAsync(id);
            if (!success)
                return NotFound(new { message = "Academic year not found" });

            return Ok(new { message = "Academic year set as current" });
        }

        /// <summary>
        /// Delete an academic year (only if no data attached)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAcademicYear(int id)
        {
            try
            {
                var success = await _academicYearService.DeleteAcademicYearAsync(id);
                if (!success)
                    return NotFound(new { message = "Academic year not found" });

                return Ok(new { message = "Academic year deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
