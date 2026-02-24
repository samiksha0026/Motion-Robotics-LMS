using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Models;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Controllers.Admin
{
    /// <summary>
    /// Class management - accessible by SuperAdmin and SchoolAdmin.
    /// </summary>
    [ApiController]
    [Route("api/admin/classes")]
    [Authorize(Roles = "SuperAdmin,SchoolAdmin")]
    public class ClassesController : ControllerBase
    {
        private readonly IClassService _classService;
        private readonly ApplicationDbContext _context;

        public ClassesController(IClassService classService, ApplicationDbContext context)
        {
            _classService = classService;
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClass(int id)
        {
            var @class = await _classService.GetClassByIdAsync(id);
            if (@class == null)
                return NotFound(new { message = "Class not found" });

            return Ok(@class);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllClasses()
        {
            var classes = await _classService.GetAllClassesAsync();
            return Ok(classes);
        }

        [HttpGet("school/{schoolId}")]
        public async Task<IActionResult> GetClassesBySchool(int schoolId)
        {
            var classes = await _classService.GetClassesBySchoolAsync(schoolId);
            return Ok(classes);
        }

        [HttpPost]
        public async Task<IActionResult> CreateClass([FromBody] ClassCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var @class = await _classService.CreateClassAsync(dto);
                return CreatedAtAction(nameof(GetClass), new { id = @class.Id }, @class);
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClass(int id, [FromBody] ClassCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var @class = await _classService.UpdateClassAsync(id, dto);
                return Ok(@class);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Class not found" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClass(int id)
        {
            var result = await _classService.DeleteClassAsync(id);
            if (!result)
                return NotFound(new { message = "Class not found" });

            return Ok(new { message = "Class deleted successfully" });
        }

        /// <summary>
        /// Get all available robotics levels
        /// </summary>
        [HttpGet("robotics-levels")]
        public async Task<IActionResult> GetRoboticsLevels()
        {
            var levels = await _context.RoboticsLevels
                .OrderBy(l => l.LevelNumber)
                .Select(l => new
                {
                    l.Id,
                    l.LevelNumber,
                    l.Name,
                    l.Description,
                    ExperimentCount = l.Experiments.Count
                })
                .ToListAsync();

            return Ok(levels);
        }

        /// <summary>
        /// Assign a robotics level to a class (Admin only)
        /// </summary>
        [HttpPost("{classId}/assign-level")]
        public async Task<IActionResult> AssignLevelToClass(int classId, [FromBody] AssignLevelRequestDto dto)
        {
            // Verify class exists
            var cls = await _context.Classes
                .Include(c => c.School)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (cls == null)
                return NotFound(new { message = "Class not found" });

            // Verify level exists
            var level = await _context.RoboticsLevels.FindAsync(dto.RoboticsLevelId);
            if (level == null)
                return NotFound(new { message = "Robotics level not found" });

            // Get current academic year (or create default if none exists)
            var currentYear = await _context.AcademicYears
                .FirstOrDefaultAsync(ay => ay.IsCurrent);

            if (currentYear == null)
            {
                // Auto-create current academic year if none exists
                currentYear = new AcademicYear
                {
                    YearName = $"{DateTime.Now.Year}-{DateTime.Now.Year + 1}",
                    StartDate = new DateTime(DateTime.Now.Year, 6, 1),
                    EndDate = new DateTime(DateTime.Now.Year + 1, 5, 31),
                    IsCurrent = true
                };
                _context.AcademicYears.Add(currentYear);
                await _context.SaveChangesAsync();
            }

            // Check if mapping already exists
            var existingMapping = await _context.SchoolLevelMappings
                .FirstOrDefaultAsync(m =>
                    m.ClassId == classId &&
                    m.AcademicYearId == currentYear.Id);

            if (existingMapping != null)
            {
                // Update existing mapping
                existingMapping.RoboticsLevelId = dto.RoboticsLevelId;
                existingMapping.UpdatedAt = DateTime.UtcNow;
                _context.SchoolLevelMappings.Update(existingMapping);
            }
            else
            {
                // Create new mapping
                var mapping = new SchoolLevelMapping
                {
                    SchoolId = cls.SchoolId,
                    ClassId = classId,
                    RoboticsLevelId = dto.RoboticsLevelId,
                    AcademicYearId = currentYear.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _context.SchoolLevelMappings.Add(mapping);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Level '{level.Name}' assigned to class '{cls.ClassName}' successfully",
                classId = classId,
                className = cls.ClassName,
                levelId = dto.RoboticsLevelId,
                levelName = level.Name,
                levelNumber = level.LevelNumber
            });
        }

        /// <summary>
        /// Remove level assignment from a class (Admin only)
        /// </summary>
        [HttpDelete("{classId}/assign-level")]
        public async Task<IActionResult> RemoveLevelFromClass(int classId)
        {
            // Get current academic year
            var currentYear = await _context.AcademicYears
                .FirstOrDefaultAsync(ay => ay.IsCurrent);

            if (currentYear == null)
                return BadRequest(new { message = "No current academic year set" });

            var mapping = await _context.SchoolLevelMappings
                .FirstOrDefaultAsync(m =>
                    m.ClassId == classId &&
                    m.AcademicYearId == currentYear.Id);

            if (mapping == null)
                return NotFound(new { message = "No level assignment found for this class" });

            _context.SchoolLevelMappings.Remove(mapping);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Level assignment removed successfully" });
        }
    }

    public class AssignLevelRequestDto
    {
        public int RoboticsLevelId { get; set; }
    }
}
