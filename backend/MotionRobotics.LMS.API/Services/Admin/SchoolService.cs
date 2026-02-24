using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Models;
using MotionRobotics.LMS.API.Repositories.Admin;

namespace MotionRobotics.LMS.API.Services.Admin
{
    public class SchoolService : ISchoolService
    {
        private readonly ISchoolRepository _schoolRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _environment;

        public SchoolService(
            ISchoolRepository schoolRepository,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment environment)
        {
            _schoolRepository = schoolRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _environment = environment;
        }

        public async Task<SchoolResponseDto?> GetSchoolByIdAsync(int id)
        {
            var school = await _schoolRepository.GetByIdAsync(id);
            if (school == null)
                return null;

            return await MapToDtoAsync(school);
        }

        public async Task<List<SchoolResponseDto>> GetAllSchoolsAsync()
        {
            var schools = await _schoolRepository.GetAllAsync();
            var dtos = new List<SchoolResponseDto>();
            foreach (var school in schools)
            {
                dtos.Add(await MapToDtoAsync(school));
            }
            return dtos;
        }

        public async Task<SchoolResponseDto> CreateSchoolAsync(SchoolCreateDto dto)
        {
            // Validation
            if (await _schoolRepository.ExistsByNameAsync(dto.SchoolName))
                throw new InvalidOperationException("School with this name already exists");

            // Check if school code already exists
            if (await _schoolRepository.ExistsByCodeAsync(dto.SchoolCode))
                throw new InvalidOperationException("School with this code already exists");

            // Create school login credentials
            var username = $"school_{dto.SchoolCode.ToLower()}";
            var password = GenerateRandomPassword();

            // Check if SchoolAdmin role exists, create if not
            if (!await _roleManager.RoleExistsAsync("SchoolAdmin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("SchoolAdmin"));
            }

            var user = new IdentityUser
            {
                UserName = username,
                Email = dto.ContactEmail,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create school admin login: {errors}");
            }

            await _userManager.AddToRoleAsync(user, "SchoolAdmin");

            var school = new School
            {
                SchoolName = dto.SchoolName,
                SchoolCode = dto.SchoolCode,
                Address = dto.Address,
                City = dto.City,
                State = dto.State,
                Pincode = dto.Pincode,
                ContactEmail = dto.ContactEmail,
                ContactPhone = dto.ContactPhone,
                PrincipalName = dto.PrincipalName ?? string.Empty,
                SchoolAdminUserId = user.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createdSchool = await _schoolRepository.CreateAsync(school);

            var response = await MapToDtoAsync(createdSchool);
            response.LoginUsername = username;
            response.LoginPassword = password;  // Return password on creation
            return response;
        }

        public async Task<SchoolResponseDto> UpdateSchoolAsync(int id, SchoolCreateDto dto)
        {
            var school = await _schoolRepository.GetByIdAsync(id);
            if (school == null)
                throw new KeyNotFoundException("School not found");

            // Check if new name already exists (excluding current school)
            if (school.SchoolName != dto.SchoolName &&
                await _schoolRepository.ExistsByNameAsync(dto.SchoolName))
                throw new InvalidOperationException("School with this name already exists");

            school.SchoolName = dto.SchoolName;
            school.SchoolCode = dto.SchoolCode;
            school.Address = dto.Address;
            school.City = dto.City;
            school.State = dto.State;
            school.Pincode = dto.Pincode;
            school.ContactEmail = dto.ContactEmail;
            school.ContactPhone = dto.ContactPhone;
            school.PrincipalName = dto.PrincipalName ?? string.Empty;
            school.UpdatedAt = DateTime.UtcNow;

            var updatedSchool = await _schoolRepository.UpdateAsync(school);
            return await MapToDtoAsync(updatedSchool);
        }

        public async Task<bool> DeleteSchoolAsync(int id)
        {
            var school = await _schoolRepository.GetByIdAsync(id);
            if (school != null && !string.IsNullOrEmpty(school.SchoolAdminUserId))
            {
                var user = await _userManager.FindByIdAsync(school.SchoolAdminUserId);
                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }
            }
            return await _schoolRepository.DeleteAsync(id);
        }

        private async Task<SchoolResponseDto> MapToDtoAsync(School school)
        {
            string? loginUsername = null;
            if (!string.IsNullOrEmpty(school.SchoolAdminUserId))
            {
                var user = await _userManager.FindByIdAsync(school.SchoolAdminUserId);
                loginUsername = user?.UserName;
            }

            return new SchoolResponseDto
            {
                Id = school.Id,
                SchoolName = school.SchoolName,
                SchoolCode = school.SchoolCode,
                Address = school.Address,
                City = school.City ?? string.Empty,
                State = school.State ?? string.Empty,
                Pincode = school.Pincode ?? string.Empty,
                ContactEmail = school.ContactEmail,
                ContactPhone = school.ContactPhone,
                LogoUrl = school.LogoUrl,
                PrincipalName = school.PrincipalName,
                LoginUsername = loginUsername,
                IsActive = school.IsActive,
                CreatedAt = school.CreatedAt,
                StudentCount = school.Students?.Count ?? 0,
                ClassCount = school.Classes?.Count ?? 0,
                TeacherCount = school.Teachers?.Count ?? 0
            };
        }

        private static string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
            var random = new Random();
            var password = new char[12];
            for (int i = 0; i < password.Length; i++)
            {
                password[i] = chars[random.Next(chars.Length)];
            }
            // Ensure password meets requirements
            return $"{new string(password)}@1Aa";
        }

        public async Task<string> UploadLogoAsync(int id, IFormFile file)
        {
            var school = await _schoolRepository.GetByIdAsync(id);
            if (school == null)
                throw new KeyNotFoundException("School not found");

            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "logos");
            Directory.CreateDirectory(uploadsPath);

            // Delete old logo if exists
            if (!string.IsNullOrEmpty(school.LogoUrl))
            {
                var oldPath = Path.Combine(_environment.ContentRootPath, "wwwroot", school.LogoUrl.TrimStart('/'));
                if (File.Exists(oldPath))
                {
                    File.Delete(oldPath);
                }
            }

            // Generate unique filename
            var extension = Path.GetExtension(file.FileName);
            var fileName = $"school_{school.SchoolCode.ToLower()}_{DateTime.UtcNow.Ticks}{extension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Update school with logo URL
            school.LogoUrl = $"/uploads/logos/{fileName}";
            school.UpdatedAt = DateTime.UtcNow;
            await _schoolRepository.UpdateAsync(school);

            return school.LogoUrl;
        }

        public async Task<bool> ToggleSchoolStatusAsync(int id)
        {
            var school = await _schoolRepository.GetByIdAsync(id);
            if (school == null)
                throw new KeyNotFoundException("School not found");

            school.IsActive = !school.IsActive;
            school.UpdatedAt = DateTime.UtcNow;
            await _schoolRepository.UpdateAsync(school);

            return school.IsActive;
        }

        public async Task<string> ResetSchoolAdminPasswordAsync(int id)
        {
            var school = await _schoolRepository.GetByIdAsync(id);
            if (school == null)
                throw new KeyNotFoundException("School not found");

            if (string.IsNullOrEmpty(school.SchoolAdminUserId))
                throw new InvalidOperationException("School has no admin user");

            var user = await _userManager.FindByIdAsync(school.SchoolAdminUserId);
            if (user == null)
                throw new KeyNotFoundException("School admin user not found");

            var newPassword = GenerateRandomPassword();
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to reset password: {errors}");
            }

            return newPassword;
        }
    }
}
