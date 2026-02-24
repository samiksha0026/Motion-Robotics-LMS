using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Models;

namespace MotionRobotics.LMS.API.Services.Admin
{
    public interface IAcademicYearService
    {
        Task<List<AcademicYearDto>> GetAllAcademicYearsAsync();
        Task<AcademicYearDto?> GetAcademicYearByIdAsync(int id);
        Task<AcademicYearDto?> GetCurrentAcademicYearAsync();
        Task<AcademicYearDto> CreateAcademicYearAsync(AcademicYearCreateDto dto);
        Task<AcademicYearDto> UpdateAcademicYearAsync(int id, AcademicYearCreateDto dto);
        Task<bool> SetCurrentAcademicYearAsync(int id);
        Task<bool> DeleteAcademicYearAsync(int id);
    }

    public class AcademicYearService : IAcademicYearService
    {
        private readonly ApplicationDbContext _context;

        public AcademicYearService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AcademicYearDto>> GetAllAcademicYearsAsync()
        {
            return await _context.AcademicYears
                .OrderByDescending(ay => ay.StartDate)
                .Select(ay => new AcademicYearDto
                {
                    Id = ay.Id,
                    YearName = ay.YearName,
                    StartDate = ay.StartDate,
                    EndDate = ay.EndDate,
                    IsCurrent = ay.IsCurrent,
                    TotalMappings = ay.SchoolLevelMappings.Count,
                    TotalStudentProgress = ay.StudentProgresses.Count
                })
                .ToListAsync();
        }

        public async Task<AcademicYearDto?> GetAcademicYearByIdAsync(int id)
        {
            return await _context.AcademicYears
                .Where(ay => ay.Id == id)
                .Select(ay => new AcademicYearDto
                {
                    Id = ay.Id,
                    YearName = ay.YearName,
                    StartDate = ay.StartDate,
                    EndDate = ay.EndDate,
                    IsCurrent = ay.IsCurrent,
                    TotalMappings = ay.SchoolLevelMappings.Count,
                    TotalStudentProgress = ay.StudentProgresses.Count
                })
                .FirstOrDefaultAsync();
        }

        public async Task<AcademicYearDto?> GetCurrentAcademicYearAsync()
        {
            return await _context.AcademicYears
                .Where(ay => ay.IsCurrent)
                .Select(ay => new AcademicYearDto
                {
                    Id = ay.Id,
                    YearName = ay.YearName,
                    StartDate = ay.StartDate,
                    EndDate = ay.EndDate,
                    IsCurrent = ay.IsCurrent,
                    TotalMappings = ay.SchoolLevelMappings.Count,
                    TotalStudentProgress = ay.StudentProgresses.Count
                })
                .FirstOrDefaultAsync();
        }

        public async Task<AcademicYearDto> CreateAcademicYearAsync(AcademicYearCreateDto dto)
        {
            // Check if year name already exists
            if (await _context.AcademicYears.AnyAsync(ay => ay.YearName == dto.YearName))
                throw new InvalidOperationException($"Academic year '{dto.YearName}' already exists");

            // Validate dates
            if (dto.EndDate <= dto.StartDate)
                throw new InvalidOperationException("End date must be after start date");

            var academicYear = new AcademicYear
            {
                YearName = dto.YearName,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsCurrent = dto.SetAsCurrent
            };

            // If setting as current, unset all others
            if (dto.SetAsCurrent)
            {
                await _context.AcademicYears
                    .Where(ay => ay.IsCurrent)
                    .ExecuteUpdateAsync(s => s.SetProperty(ay => ay.IsCurrent, false));
            }

            _context.AcademicYears.Add(academicYear);
            await _context.SaveChangesAsync();

            return new AcademicYearDto
            {
                Id = academicYear.Id,
                YearName = academicYear.YearName,
                StartDate = academicYear.StartDate,
                EndDate = academicYear.EndDate,
                IsCurrent = academicYear.IsCurrent,
                TotalMappings = 0,
                TotalStudentProgress = 0
            };
        }

        public async Task<AcademicYearDto> UpdateAcademicYearAsync(int id, AcademicYearCreateDto dto)
        {
            var academicYear = await _context.AcademicYears.FindAsync(id);
            if (academicYear == null)
                throw new KeyNotFoundException("Academic year not found");

            // Check if year name already exists (for different record)
            if (await _context.AcademicYears.AnyAsync(ay => ay.YearName == dto.YearName && ay.Id != id))
                throw new InvalidOperationException($"Academic year '{dto.YearName}' already exists");

            // Validate dates
            if (dto.EndDate <= dto.StartDate)
                throw new InvalidOperationException("End date must be after start date");

            academicYear.YearName = dto.YearName;
            academicYear.StartDate = dto.StartDate;
            academicYear.EndDate = dto.EndDate;

            if (dto.SetAsCurrent && !academicYear.IsCurrent)
            {
                await _context.AcademicYears
                    .Where(ay => ay.IsCurrent && ay.Id != id)
                    .ExecuteUpdateAsync(s => s.SetProperty(ay => ay.IsCurrent, false));
                academicYear.IsCurrent = true;
            }

            await _context.SaveChangesAsync();

            return await GetAcademicYearByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated year");
        }

        public async Task<bool> SetCurrentAcademicYearAsync(int id)
        {
            var academicYear = await _context.AcademicYears.FindAsync(id);
            if (academicYear == null)
                return false;

            // Unset all current years
            await _context.AcademicYears
                .Where(ay => ay.IsCurrent)
                .ExecuteUpdateAsync(s => s.SetProperty(ay => ay.IsCurrent, false));

            // Set this one as current
            academicYear.IsCurrent = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAcademicYearAsync(int id)
        {
            var academicYear = await _context.AcademicYears
                .Include(ay => ay.SchoolLevelMappings)
                .Include(ay => ay.StudentProgresses)
                .FirstOrDefaultAsync(ay => ay.Id == id);

            if (academicYear == null)
                return false;

            // Don't allow deletion if there's data
            if (academicYear.SchoolLevelMappings.Any() || academicYear.StudentProgresses.Any())
                throw new InvalidOperationException("Cannot delete academic year with existing mappings or student progress");

            // Don't allow deletion of current year
            if (academicYear.IsCurrent)
                throw new InvalidOperationException("Cannot delete the current academic year");

            _context.AcademicYears.Remove(academicYear);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
