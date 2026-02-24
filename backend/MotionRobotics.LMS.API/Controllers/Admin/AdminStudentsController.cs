using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Controllers.Admin
{
    /// <summary>
    /// Student management - accessible by SuperAdmin and SchoolAdmin.
    /// </summary>
    [ApiController]
    [Route("api/admin/students")]
    [Authorize(Roles = "SuperAdmin,SchoolAdmin")]
    public class AdminStudentsController : ControllerBase
    {
        private readonly IAdminStudentService _studentService;

        public AdminStudentsController(IAdminStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateStudent([FromBody] StudentCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var student = await _studentService.CreateStudentAsync(dto);
                return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, student);
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
        public async Task<IActionResult> GetStudent(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
                return NotFound(new { message = "Student not found" });

            return Ok(student);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await _studentService.GetAllStudentsAsync();
            return Ok(students);
        }

        [HttpGet("school/{schoolId}")]
        public async Task<IActionResult> GetStudentsBySchool(int schoolId)
        {
            var students = await _studentService.GetStudentsBySchoolAsync(schoolId);
            return Ok(students);
        }

        [HttpGet("class/{classId}")]
        public async Task<IActionResult> GetStudentsByClass(int classId)
        {
            var students = await _studentService.GetStudentsByClassAsync(classId);
            return Ok(students);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var result = await _studentService.DeleteStudentAsync(id);
            if (!result)
                return NotFound(new { message = "Student not found" });

            return Ok(new { message = "Student deleted successfully" });
        }
    }
}
