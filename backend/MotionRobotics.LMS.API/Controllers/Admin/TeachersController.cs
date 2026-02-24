using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Controllers.Admin
{
    /// <summary>
    /// Teacher management - accessible by SuperAdmin (all schools) and SchoolAdmin (own school only).
    /// </summary>
    [ApiController]
    [Route("api/admin/teachers")]
    [Authorize(Roles = "SuperAdmin,SchoolAdmin")]
    public class TeachersController : ControllerBase
    {
        private readonly IAdminTeacherService _teacherService;
        private readonly UserManager<IdentityUser> _userManager;

        public TeachersController(IAdminTeacherService teacherService, UserManager<IdentityUser> userManager)
        {
            _teacherService = teacherService;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTeacher([FromBody] TeacherCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var teacher = await _teacherService.CreateTeacherAsync(dto);
                return CreatedAtAction(nameof(GetTeacher), new { id = teacher.Id }, teacher);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTeacher(int id)
        {
            var teacher = await _teacherService.GetTeacherByIdAsync(id);
            if (teacher == null)
                return NotFound(new { message = "Teacher not found" });

            return Ok(teacher);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTeachers()
        {
            var teachers = await _teacherService.GetAllTeachersAsync();
            return Ok(teachers);
        }

        [HttpGet("school/{schoolId}")]
        public async Task<IActionResult> GetTeachersBySchool(int schoolId)
        {
            var teachers = await _teacherService.GetTeachersBySchoolAsync(schoolId);
            return Ok(teachers);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTeacher(int id, [FromBody] TeacherCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var teacher = await _teacherService.UpdateTeacherAsync(id, dto);
                return Ok(teacher);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Teacher not found" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var result = await _teacherService.DeleteTeacherAsync(id);
            if (!result)
                return NotFound(new { message = "Teacher not found" });

            return Ok(new { message = "Teacher deleted successfully" });
        }

        [HttpPost("{teacherId}/classes/{classId}")]
        public async Task<IActionResult> AssignClass(int teacherId, int classId)
        {
            try
            {
                await _teacherService.AssignClassToTeacherAsync(teacherId, classId);
                return Ok(new { message = "Class assigned successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{teacherId}/classes/{classId}")]
        public async Task<IActionResult> RemoveClass(int teacherId, int classId)
        {
            try
            {
                await _teacherService.RemoveClassFromTeacherAsync(teacherId, classId);
                return Ok(new { message = "Class removed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Clean up orphaned user account by email (SuperAdmin only).
        /// Use this when a teacher/student was deleted but the Identity user remains.
        /// </summary>
        [HttpDelete("cleanup-user/{email}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CleanupOrphanedUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var roles = await _userManager.GetRolesAsync(user);

            // Don't allow deleting SuperAdmin accounts
            if (roles.Contains("SuperAdmin"))
                return BadRequest(new { message = "Cannot delete SuperAdmin accounts" });

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { message = "Failed to delete user", errors = result.Errors });

            return Ok(new { message = $"User '{email}' deleted successfully" });
        }
    }
}
