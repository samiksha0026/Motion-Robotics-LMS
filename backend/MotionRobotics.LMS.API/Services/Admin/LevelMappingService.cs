using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Models;

namespace MotionRobotics.LMS.API.Services.Admin
{
    public interface ILevelMappingService
    {
        Task<List<LevelMappingDto>> GetAllMappingsAsync(int? schoolId = null, int? academicYearId = null);
        Task<LevelMappingDto?> GetMappingByIdAsync(int id);
        Task<SchoolLevelAssignmentsDto?> GetSchoolLevelAssignmentsAsync(int schoolId, int academicYearId);
        Task<LevelMappingDto> CreateMappingAsync(LevelMappingCreateDto dto);
        Task<List<LevelMappingDto>> CreateBulkMappingsAsync(BulkLevelMappingDto dto);
        Task<LevelMappingDto> UpdateMappingAsync(int id, LevelMappingUpdateDto dto);
        Task<bool> DeleteMappingAsync(int id);
        Task<bool> DeleteSchoolMappingsAsync(int schoolId, int academicYearId);
    }

    public class LevelMappingService : ILevelMappingService
    {
        private readonly ApplicationDbContext _context;

        public LevelMappingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<LevelMappingDto>> GetAllMappingsAsync(int? schoolId = null, int? academicYearId = null)
        {
            var query = _context.SchoolLevelMappings
                .Include(slm => slm.School)
                .Include(slm => slm.Class)
                .Include(slm => slm.RoboticsLevel)
                .Include(slm => slm.AcademicYear)
                .AsQueryable();

            if (schoolId.HasValue)
                query = query.Where(slm => slm.SchoolId == schoolId.Value);

            if (academicYearId.HasValue)
                query = query.Where(slm => slm.AcademicYearId == academicYearId.Value);

            return await query
                .OrderBy(slm => slm.School!.SchoolName)
                .ThenBy(slm => slm.Class!.ClassName)
                .Select(slm => new LevelMappingDto
                {
                    Id = slm.Id,
                    SchoolId = slm.SchoolId,
                    SchoolName = slm.School!.SchoolName,
                    ClassId = slm.ClassId,
                    ClassName = slm.Class!.ClassName,
                    RoboticsLevelId = slm.RoboticsLevelId,
                    LevelNumber = slm.RoboticsLevel!.LevelNumber,
                    LevelName = slm.RoboticsLevel.Name,
                    AcademicYearId = slm.AcademicYearId,
                    AcademicYearName = slm.AcademicYear!.YearName,
                    CreatedAt = slm.CreatedAt,
                    UpdatedAt = slm.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<LevelMappingDto?> GetMappingByIdAsync(int id)
        {
            return await _context.SchoolLevelMappings
                .Include(slm => slm.School)
                .Include(slm => slm.Class)
                .Include(slm => slm.RoboticsLevel)
                .Include(slm => slm.AcademicYear)
                .Where(slm => slm.Id == id)
                .Select(slm => new LevelMappingDto
                {
                    Id = slm.Id,
                    SchoolId = slm.SchoolId,
                    SchoolName = slm.School!.SchoolName,
                    ClassId = slm.ClassId,
                    ClassName = slm.Class!.ClassName,
                    RoboticsLevelId = slm.RoboticsLevelId,
                    LevelNumber = slm.RoboticsLevel!.LevelNumber,
                    LevelName = slm.RoboticsLevel.Name,
                    AcademicYearId = slm.AcademicYearId,
                    AcademicYearName = slm.AcademicYear!.YearName,
                    CreatedAt = slm.CreatedAt,
                    UpdatedAt = slm.UpdatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<SchoolLevelAssignmentsDto?> GetSchoolLevelAssignmentsAsync(int schoolId, int academicYearId)
        {
            var school = await _context.Schools.FindAsync(schoolId);
            if (school == null)
                return null;

            var academicYear = await _context.AcademicYears.FindAsync(academicYearId);
            if (academicYear == null)
                return null;

            // Get existing mappings for this school/year
            var mappings = await GetAllMappingsAsync(schoolId, academicYearId);

            // Get all classes for this school
            var mappedClassIds = mappings.Select(m => m.ClassId).ToHashSet();
            var availableClasses = await _context.Classes
                .Where(c => c.SchoolId == schoolId)
                .Select(c => new ClassSummaryDto
                {
                    Id = c.Id,
                    ClassName = c.ClassName,
                    StudentCount = c.Students.Count(s => s.IsActive),
                    HasMapping = mappedClassIds.Contains(c.Id)
                })
                .OrderBy(c => c.ClassName)
                .ToListAsync();

            return new SchoolLevelAssignmentsDto
            {
                SchoolId = schoolId,
                SchoolName = school.SchoolName,
                AcademicYearId = academicYearId,
                AcademicYearName = academicYear.YearName,
                Mappings = mappings,
                AvailableClasses = availableClasses
            };
        }

        public async Task<LevelMappingDto> CreateMappingAsync(LevelMappingCreateDto dto)
        {
            // Validate school exists
            var school = await _context.Schools.FindAsync(dto.SchoolId);
            if (school == null)
                throw new KeyNotFoundException("School not found");

            // Validate class exists and belongs to school
            var classEntity = await _context.Classes.FindAsync(dto.ClassId);
            if (classEntity == null)
                throw new KeyNotFoundException("Class not found");
            if (classEntity.SchoolId != dto.SchoolId)
                throw new InvalidOperationException("Class does not belong to the specified school");

            // Validate robotics level exists
            var level = await _context.RoboticsLevels.FindAsync(dto.RoboticsLevelId);
            if (level == null)
                throw new KeyNotFoundException("Robotics level not found");

            // Validate academic year exists
            var academicYear = await _context.AcademicYears.FindAsync(dto.AcademicYearId);
            if (academicYear == null)
                throw new KeyNotFoundException("Academic year not found");

            // Check if mapping already exists
            var existingMapping = await _context.SchoolLevelMappings
                .FirstOrDefaultAsync(slm =>
                    slm.SchoolId == dto.SchoolId &&
                    slm.ClassId == dto.ClassId &&
                    slm.AcademicYearId == dto.AcademicYearId);

            if (existingMapping != null)
                throw new InvalidOperationException($"A mapping already exists for this class in the selected academic year");

            var mapping = new SchoolLevelMapping
            {
                SchoolId = dto.SchoolId,
                ClassId = dto.ClassId,
                RoboticsLevelId = dto.RoboticsLevelId,
                AcademicYearId = dto.AcademicYearId,
                CreatedAt = DateTime.UtcNow
            };

            _context.SchoolLevelMappings.Add(mapping);
            await _context.SaveChangesAsync();

            return await GetMappingByIdAsync(mapping.Id) ?? throw new InvalidOperationException("Failed to retrieve created mapping");
        }

        public async Task<List<LevelMappingDto>> CreateBulkMappingsAsync(BulkLevelMappingDto dto)
        {
            // Validate school exists
            var school = await _context.Schools.FindAsync(dto.SchoolId);
            if (school == null)
                throw new KeyNotFoundException("School not found");

            // Validate academic year exists
            var academicYear = await _context.AcademicYears.FindAsync(dto.AcademicYearId);
            if (academicYear == null)
                throw new KeyNotFoundException("Academic year not found");

            // Get all classes for validation
            var schoolClassIds = await _context.Classes
                .Where(c => c.SchoolId == dto.SchoolId)
                .Select(c => c.Id)
                .ToListAsync();

            // Get all level IDs for validation
            var levelIds = await _context.RoboticsLevels
                .Select(rl => rl.Id)
                .ToListAsync();

            // Get existing mappings for this school/year
            var existingMappings = await _context.SchoolLevelMappings
                .Where(slm => slm.SchoolId == dto.SchoolId && slm.AcademicYearId == dto.AcademicYearId)
                .Select(slm => slm.ClassId)
                .ToListAsync();

            var createdMappings = new List<SchoolLevelMapping>();

            foreach (var pair in dto.Mappings)
            {
                // Validate class belongs to school
                if (!schoolClassIds.Contains(pair.ClassId))
                    throw new InvalidOperationException($"Class ID {pair.ClassId} does not belong to the school");

                // Validate level exists
                if (!levelIds.Contains(pair.RoboticsLevelId))
                    throw new InvalidOperationException($"Robotics level ID {pair.RoboticsLevelId} not found");

                // Skip if mapping already exists
                if (existingMappings.Contains(pair.ClassId))
                    continue;

                var mapping = new SchoolLevelMapping
                {
                    SchoolId = dto.SchoolId,
                    ClassId = pair.ClassId,
                    RoboticsLevelId = pair.RoboticsLevelId,
                    AcademicYearId = dto.AcademicYearId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.SchoolLevelMappings.Add(mapping);
                createdMappings.Add(mapping);
            }

            await _context.SaveChangesAsync();

            // Return all created mappings
            var createdIds = createdMappings.Select(m => m.Id).ToList();
            return await _context.SchoolLevelMappings
                .Include(slm => slm.School)
                .Include(slm => slm.Class)
                .Include(slm => slm.RoboticsLevel)
                .Include(slm => slm.AcademicYear)
                .Where(slm => createdIds.Contains(slm.Id))
                .Select(slm => new LevelMappingDto
                {
                    Id = slm.Id,
                    SchoolId = slm.SchoolId,
                    SchoolName = slm.School!.SchoolName,
                    ClassId = slm.ClassId,
                    ClassName = slm.Class!.ClassName,
                    RoboticsLevelId = slm.RoboticsLevelId,
                    LevelNumber = slm.RoboticsLevel!.LevelNumber,
                    LevelName = slm.RoboticsLevel.Name,
                    AcademicYearId = slm.AcademicYearId,
                    AcademicYearName = slm.AcademicYear!.YearName,
                    CreatedAt = slm.CreatedAt,
                    UpdatedAt = slm.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<LevelMappingDto> UpdateMappingAsync(int id, LevelMappingUpdateDto dto)
        {
            var mapping = await _context.SchoolLevelMappings.FindAsync(id);
            if (mapping == null)
                throw new KeyNotFoundException("Mapping not found");

            // Validate new level exists
            var level = await _context.RoboticsLevels.FindAsync(dto.RoboticsLevelId);
            if (level == null)
                throw new KeyNotFoundException("Robotics level not found");

            mapping.RoboticsLevelId = dto.RoboticsLevelId;
            mapping.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetMappingByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated mapping");
        }

        public async Task<bool> DeleteMappingAsync(int id)
        {
            var mapping = await _context.SchoolLevelMappings.FindAsync(id);
            if (mapping == null)
                return false;

            // Check if there's any student progress linked to this mapping
            var hasProgress = await _context.StudentProgress
                .Include(sp => sp.Student)
                .AnyAsync(sp =>
                    sp.Student!.ClassId == mapping.ClassId &&
                    sp.AcademicYearId == mapping.AcademicYearId);

            if (hasProgress)
                throw new InvalidOperationException("Cannot delete mapping with existing student progress");

            _context.SchoolLevelMappings.Remove(mapping);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteSchoolMappingsAsync(int schoolId, int academicYearId)
        {
            var mappings = await _context.SchoolLevelMappings
                .Where(slm => slm.SchoolId == schoolId && slm.AcademicYearId == academicYearId)
                .ToListAsync();

            if (!mappings.Any())
                return false;

            // Check for student progress
            var classIds = mappings.Select(m => m.ClassId).ToList();
            var hasProgress = await _context.StudentProgress
                .Include(sp => sp.Student)
                .AnyAsync(sp =>
                    classIds.Contains(sp.Student!.ClassId) &&
                    sp.AcademicYearId == academicYearId);

            if (hasProgress)
                throw new InvalidOperationException("Cannot delete mappings with existing student progress");

            _context.SchoolLevelMappings.RemoveRange(mappings);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
