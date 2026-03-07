using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Models;

namespace MotionRobotics.LMS.API.Services.Admin;

public class LabService : ILabService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public LabService(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<LabInfoDto?> GetLabInfoAsync(int schoolId)
    {
        var school = await _context.Schools
            .Include(s => s.LabPhotos)
            .FirstOrDefaultAsync(s => s.Id == schoolId);

        return school == null ? null : MapToLabInfoDto(school);
    }

    public async Task<LabInfoDto> UpdateLabInfoAsync(int schoolId, UpdateLabInfoDto dto)
    {
        var school = await _context.Schools
            .Include(s => s.LabPhotos)
            .FirstOrDefaultAsync(s => s.Id == schoolId)
            ?? throw new KeyNotFoundException($"School with ID {schoolId} not found.");

        school.LabDescription = dto.LabDescription;
        school.LabArea = dto.LabArea;
        school.LabCapacity = dto.LabCapacity;
        school.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToLabInfoDto(school);
    }

    public async Task<LabPhotoDto> UploadPhotoAsync(int schoolId, IFormFile file, string? caption)
    {
        _ = await _context.Schools.FindAsync(schoolId)
            ?? throw new KeyNotFoundException($"School with ID {schoolId} not found.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException("Only JPG, PNG, and WebP images are allowed.");

        if (file.Length > MaxFileSizeBytes)
            throw new InvalidOperationException("File size must not exceed 10 MB.");

        var maxOrder = await _context.LabPhotos
            .Where(p => p.SchoolId == schoolId)
            .MaxAsync(p => (int?)p.DisplayOrder) ?? 0;

        var filename = $"{Guid.NewGuid()}{ext}";
        var schoolFolder = Path.Combine(_env.WebRootPath, "lab-photos", schoolId.ToString());
        Directory.CreateDirectory(schoolFolder);
        var fullPath = Path.Combine(schoolFolder, filename);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
            await file.CopyToAsync(stream);

        var photo = new LabPhoto
        {
            SchoolId = schoolId,
            FilePath = $"/lab-photos/{schoolId}/{filename}",
            Caption = caption,
            DisplayOrder = maxOrder + 1,
            UploadedAt = DateTime.UtcNow
        };

        _context.LabPhotos.Add(photo);
        await _context.SaveChangesAsync();

        return MapToPhotoDto(photo);
    }

    public async Task<bool> DeletePhotoAsync(int photoId, int schoolId)
    {
        var photo = await _context.LabPhotos
            .FirstOrDefaultAsync(p => p.Id == photoId && p.SchoolId == schoolId);

        if (photo == null) return false;

        // Delete file from disk
        var relativePath = photo.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_env.WebRootPath, relativePath);
        if (File.Exists(fullPath)) File.Delete(fullPath);

        _context.LabPhotos.Remove(photo);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<SchoolLabSummaryDto>> GetAllSchoolsLabSummaryAsync()
    {
        return await _context.Schools
            .OrderBy(s => s.SchoolName)
            .Select(s => new SchoolLabSummaryDto
            {
                SchoolId = s.Id,
                SchoolName = s.SchoolName,
                SchoolCode = s.SchoolCode,
                PhotoCount = s.LabPhotos.Count,
                HasLabInfo = s.LabDescription != null || s.LabArea != null || s.LabCapacity != null
            })
            .ToListAsync();
    }

    // ── Mapping helpers ──────────────────────────────────────────────────────

    private static LabInfoDto MapToLabInfoDto(School school) => new()
    {
        SchoolId = school.Id,
        SchoolName = school.SchoolName,
        LabDescription = school.LabDescription,
        LabArea = school.LabArea,
        LabCapacity = school.LabCapacity,
        Photos = school.LabPhotos
            .OrderBy(p => p.DisplayOrder)
            .Select(MapToPhotoDto)
            .ToList()
    };

    private static LabPhotoDto MapToPhotoDto(LabPhoto photo) => new()
    {
        Id = photo.Id,
        Url = photo.FilePath,
        Caption = photo.Caption,
        DisplayOrder = photo.DisplayOrder,
        UploadedAt = photo.UploadedAt
    };
}
