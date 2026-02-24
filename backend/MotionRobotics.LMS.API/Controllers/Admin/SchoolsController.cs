using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Controllers.Admin
{
    /// <summary>
    /// SuperAdmin-only endpoints for managing schools.
    /// SuperAdmin creates schools and their SchoolAdmin accounts.
    /// </summary>
    [ApiController]
    [Route("api/admin/schools")]
    [Authorize(Roles = "SuperAdmin")]
    public class SchoolsController : ControllerBase
    {
        private readonly ISchoolService _schoolService;

        public SchoolsController(ISchoolService schoolService)
        {
            _schoolService = schoolService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSchool(int id)
        {
            var school = await _schoolService.GetSchoolByIdAsync(id);
            if (school == null)
                return NotFound(new { message = "School not found" });

            return Ok(school);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSchools()
        {
            var schools = await _schoolService.GetAllSchoolsAsync();
            return Ok(schools);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSchool([FromBody] SchoolCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var school = await _schoolService.CreateSchoolAsync(dto);
                return CreatedAtAction(nameof(GetSchool), new { id = school.Id }, school);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchool(int id, [FromBody] SchoolCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var school = await _schoolService.UpdateSchoolAsync(id, dto);
                return Ok(school);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "School not found" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchool(int id)
        {
            var result = await _schoolService.DeleteSchoolAsync(id);
            if (!result)
                return NotFound(new { message = "School not found" });

            return Ok(new { message = "School deleted successfully" });
        }

        [HttpPost("{id}/logo")]
        public async Task<IActionResult> UploadLogo(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded" });

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return BadRequest(new { message = "Only image files (jpg, png, gif, webp) are allowed" });

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "File size must be less than 5MB" });

            try
            {
                var logoUrl = await _schoolService.UploadLogoAsync(id, file);
                return Ok(new { logoUrl });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "School not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Failed to upload logo: {ex.Message}" });
            }
        }

        /// <summary>
        /// Toggle school active status (enable/disable school)
        /// </summary>
        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleSchoolStatus(int id)
        {
            try
            {
                var result = await _schoolService.ToggleSchoolStatusAsync(id);
                return Ok(new
                {
                    message = result ? "School activated" : "School deactivated",
                    isActive = result
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "School not found" });
            }
        }

        /// <summary>
        /// Reset SchoolAdmin password and return new credentials
        /// </summary>
        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetSchoolAdminPassword(int id)
        {
            try
            {
                var newPassword = await _schoolService.ResetSchoolAdminPasswordAsync(id);
                return Ok(new
                {
                    message = "Password reset successfully",
                    newPassword = newPassword
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
    }
}
