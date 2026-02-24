using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Models;
using MotionRobotics.LMS.API.Repositories.Admin;
using Microsoft.EntityFrameworkCore;

namespace MotionRobotics.LMS.API.Services.Admin
{
    public class ClassService : IClassService
    {
        private readonly IClassRepository _classRepository;
        private readonly ApplicationDbContext _context;

        public ClassService(IClassRepository classRepository, ApplicationDbContext context)
        {
            _classRepository = classRepository;
            _context = context;
        }

        public async Task<ClassResponseDto?> GetClassByIdAsync(int id)
        {
            var @class = await _classRepository.GetByIdAsync(id);
            if (@class == null)
                return null;

            return MapToDto(@class);
        }

        public async Task<List<ClassResponseDto>> GetAllClassesAsync()
        {
            var classes = await _classRepository.GetAllAsync();
            var dtos = new List<ClassResponseDto>();

            // Get current academic year
            var currentYear = await _context.AcademicYears.FirstOrDefaultAsync(a => a.IsCurrent);

            // Get all level mappings for current year
            var levelMappings = currentYear != null
                ? await _context.SchoolLevelMappings
                    .Include(m => m.RoboticsLevel)
                    .Where(m => m.AcademicYearId == currentYear.Id)
                    .ToListAsync()
                : new List<SchoolLevelMapping>();

            // Get all teacher-class assignments
            var teacherClasses = await _context.TeacherClasses
                .Include(tc => tc.Teacher)
                .ToListAsync();

            foreach (var c in classes)
            {
                var dto = MapToDto(c);
                var mapping = levelMappings.FirstOrDefault(m => m.ClassId == c.Id);
                if (mapping != null)
                {
                    dto.RoboticsLevelId = mapping.RoboticsLevelId;
                    dto.RoboticsLevelName = mapping.RoboticsLevel?.Name;
                    dto.RoboticsLevelNumber = mapping.RoboticsLevel?.LevelNumber;
                }

                // Get assigned teachers
                var classTeachers = teacherClasses.Where(tc => tc.ClassId == c.Id).ToList();
                if (classTeachers.Any())
                {
                    dto.TeacherName = classTeachers.First().Teacher?.FullName;
                    dto.TeacherNames = classTeachers.Select(tc => tc.Teacher?.FullName).Where(name => !string.IsNullOrEmpty(name)).Cast<string>().ToList();
                }

                dtos.Add(dto);
            }

            return dtos;
        }

        public async Task<List<ClassResponseDto>> GetClassesBySchoolAsync(int schoolId)
        {
            var classes = await _classRepository.GetBySchoolIdAsync(schoolId);
            var dtos = new List<ClassResponseDto>();

            // Get current academic year
            var currentYear = await _context.AcademicYears.FirstOrDefaultAsync(a => a.IsCurrent);

            // Get level mappings for this school and current year
            var levelMappings = currentYear != null
                ? await _context.SchoolLevelMappings
                    .Include(m => m.RoboticsLevel)
                    .Where(m => m.SchoolId == schoolId && m.AcademicYearId == currentYear.Id)
                    .ToListAsync()
                : new List<SchoolLevelMapping>();

            // Get all teacher-class assignments for this school
            var teacherClasses = await _context.TeacherClasses
                .Include(tc => tc.Teacher)
                .Where(tc => tc.Class.SchoolId == schoolId)
                .ToListAsync();

            foreach (var c in classes)
            {
                var dto = MapToDto(c);
                var mapping = levelMappings.FirstOrDefault(m => m.ClassId == c.Id);
                if (mapping != null)
                {
                    dto.RoboticsLevelId = mapping.RoboticsLevelId;
                    dto.RoboticsLevelName = mapping.RoboticsLevel?.Name;
                    dto.RoboticsLevelNumber = mapping.RoboticsLevel?.LevelNumber;
                }

                // Get assigned teachers
                var classTeachers = teacherClasses.Where(tc => tc.ClassId == c.Id).ToList();
                if (classTeachers.Any())
                {
                    dto.TeacherName = classTeachers.First().Teacher?.FullName;
                    dto.TeacherNames = classTeachers.Select(tc => tc.Teacher?.FullName).Where(name => !string.IsNullOrEmpty(name)).Cast<string>().ToList();
                }

                dtos.Add(dto);
            }

            return dtos;
        }

        public async Task<ClassResponseDto> CreateClassAsync(ClassCreateDto dto)
        {
            // Validation
            if (await _classRepository.ExistsByNameAndSchoolAsync(dto.ClassName, dto.SchoolId))
                throw new InvalidOperationException("Class with this name already exists in this school");

            var @class = new Class
            {
                ClassName = dto.ClassName,
                SchoolId = dto.SchoolId,
                CreatedAt = DateTime.UtcNow
            };

            var createdClass = await _classRepository.CreateAsync(@class);

            // If robotics level is provided, create the SchoolLevelMapping
            if (dto.RoboticsLevelId.HasValue && dto.RoboticsLevelId.Value > 0)
            {
                // Get current academic year
                var currentYear = await _context.AcademicYears.FirstOrDefaultAsync(a => a.IsCurrent);
                if (currentYear != null)
                {
                    // Check if mapping already exists
                    var existingMapping = await _context.SchoolLevelMappings
                        .FirstOrDefaultAsync(m =>
                            m.SchoolId == dto.SchoolId &&
                            m.ClassId == createdClass.Id &&
                            m.AcademicYearId == currentYear.Id);

                    if (existingMapping == null)
                    {
                        var mapping = new SchoolLevelMapping
                        {
                            SchoolId = dto.SchoolId,
                            ClassId = createdClass.Id,
                            RoboticsLevelId = dto.RoboticsLevelId.Value,
                            AcademicYearId = currentYear.Id,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.SchoolLevelMappings.Add(mapping);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            return await GetClassByIdWithLevelAsync(createdClass.Id) ?? MapToDto(createdClass);
        }

        // Helper to get class with level info
        private async Task<ClassResponseDto?> GetClassByIdWithLevelAsync(int id)
        {
            var @class = await _classRepository.GetByIdAsync(id);
            if (@class == null) return null;

            var dto = MapToDto(@class);

            // Get the level mapping
            var currentYear = await _context.AcademicYears.FirstOrDefaultAsync(a => a.IsCurrent);
            if (currentYear != null)
            {
                var mapping = await _context.SchoolLevelMappings
                    .Include(m => m.RoboticsLevel)
                    .FirstOrDefaultAsync(m =>
                        m.ClassId == id &&
                        m.AcademicYearId == currentYear.Id);

                if (mapping != null)
                {
                    dto.RoboticsLevelId = mapping.RoboticsLevelId;
                    dto.RoboticsLevelName = mapping.RoboticsLevel?.Name;
                    dto.RoboticsLevelNumber = mapping.RoboticsLevel?.LevelNumber;
                }
            }

            return dto;
        }

        public async Task<ClassResponseDto> UpdateClassAsync(int id, ClassCreateDto dto)
        {
            var @class = await _classRepository.GetByIdAsync(id);
            if (@class == null)
                throw new KeyNotFoundException("Class not found");

            // Check if new name already exists (excluding current class)
            if (@class.ClassName != dto.ClassName &&
                await _classRepository.ExistsByNameAndSchoolAsync(dto.ClassName, dto.SchoolId))
                throw new InvalidOperationException("Class with this name already exists in this school");

            @class.ClassName = dto.ClassName;
            @class.SchoolId = dto.SchoolId;

            var updatedClass = await _classRepository.UpdateAsync(@class);
            return MapToDto(updatedClass);
        }

        public async Task<bool> DeleteClassAsync(int id)
        {
            return await _classRepository.DeleteAsync(id);
        }

        private ClassResponseDto MapToDto(Class @class)
        {
            return new ClassResponseDto
            {
                Id = @class.Id,
                ClassName = @class.ClassName,
                SchoolId = @class.SchoolId,
                SchoolName = @class.School?.SchoolName ?? "Unknown",
                CreatedAt = @class.CreatedAt,
                StudentCount = @class.Students?.Count ?? 0
            };
        }
    }
}
