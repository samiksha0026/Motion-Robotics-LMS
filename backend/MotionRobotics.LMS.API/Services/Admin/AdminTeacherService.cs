using Microsoft.AspNetCore.Identity;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Models;
using MotionRobotics.LMS.API.Repositories.Admin;

namespace MotionRobotics.LMS.API.Services.Admin
{
    public class AdminTeacherService : IAdminTeacherService
    {
        private readonly ITeacherRepository _teacherRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminTeacherService(ITeacherRepository teacherRepository, UserManager<IdentityUser> userManager)
        {
            _teacherRepository = teacherRepository;
            _userManager = userManager;
        }

        public async Task<TeacherResponseDto> CreateTeacherAsync(TeacherCreateDto dto)
        {
            // Validation: Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new InvalidOperationException("User with this email already exists");

            if (await _teacherRepository.ExistsByEmailAsync(dto.Email))
                throw new InvalidOperationException("Teacher with this email already exists");

            // Create IdentityUser for authentication
            var identityUser = new IdentityUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                EmailConfirmed = true
            };

            var createUserResult = await _userManager.CreateAsync(identityUser, dto.Password);
            if (!createUserResult.Succeeded)
                throw new InvalidOperationException("Failed to create user: " +
                    string.Join(", ", createUserResult.Errors.Select(e => e.Description)));

            // Assign Teacher role
            await _userManager.AddToRoleAsync(identityUser, "Teacher");

            // Create Teacher record
            var teacher = new Teacher
            {
                UserId = identityUser.Id,  // Link to IdentityUser
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                SchoolId = dto.SchoolId,
                CreatedAt = DateTime.UtcNow
            };

            var createdTeacher = await _teacherRepository.CreateAsync(teacher);

            // Assign classes
            if (dto.ClassIds.Any())
            {
                foreach (var classId in dto.ClassIds)
                {
                    await _teacherRepository.AssignClassToTeacherAsync(createdTeacher.Id, classId);
                }
            }

            var fullTeacher = await _teacherRepository.GetByIdAsync(createdTeacher.Id);
            return MapToDto(fullTeacher!);
        }

        public async Task<TeacherResponseDto?> GetTeacherByIdAsync(int id)
        {
            var teacher = await _teacherRepository.GetByIdAsync(id);
            if (teacher == null)
                return null;

            return MapToDto(teacher);
        }

        public async Task<List<TeacherResponseDto>> GetTeachersBySchoolAsync(int schoolId)
        {
            var teachers = await _teacherRepository.GetBySchoolIdAsync(schoolId);
            return teachers.Select(MapToDto).ToList();
        }

        public async Task<List<TeacherResponseDto>> GetAllTeachersAsync()
        {
            var teachers = await _teacherRepository.GetAllAsync();
            return teachers.Select(MapToDto).ToList();
        }

        public async Task<TeacherResponseDto> UpdateTeacherAsync(int id, TeacherCreateDto dto)
        {
            var teacher = await _teacherRepository.GetByIdAsync(id);
            if (teacher == null)
                throw new KeyNotFoundException("Teacher not found");

            teacher.FullName = dto.FullName;
            teacher.PhoneNumber = dto.PhoneNumber;
            teacher.SchoolId = dto.SchoolId;

            var updatedTeacher = await _teacherRepository.UpdateAsync(teacher);
            return MapToDto(updatedTeacher);
        }

        public async Task<bool> DeleteTeacherAsync(int id)
        {
            var teacher = await _teacherRepository.GetByIdAsync(id);
            if (teacher == null)
                return false;

            // Delete associated IdentityUser
            var user = await _userManager.FindByEmailAsync(teacher.Email);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            return await _teacherRepository.DeleteAsync(id);
        }

        public async Task AssignClassToTeacherAsync(int teacherId, int classId)
        {
            await _teacherRepository.AssignClassToTeacherAsync(teacherId, classId);
        }

        public async Task RemoveClassFromTeacherAsync(int teacherId, int classId)
        {
            await _teacherRepository.RemoveClassFromTeacherAsync(teacherId, classId);
        }

        private TeacherResponseDto MapToDto(Teacher teacher)
        {
            return new TeacherResponseDto
            {
                Id = teacher.Id,
                Email = teacher.Email,
                FullName = teacher.FullName,
                PhoneNumber = teacher.PhoneNumber,
                SchoolId = teacher.SchoolId,
                SchoolName = teacher.School?.SchoolName ?? "Unknown",
                Classes = teacher.TeacherClasses?.Select(tc => new ClassResponseDto
                {
                    Id = tc.Class!.Id,
                    ClassName = tc.Class.ClassName,
                    SchoolId = tc.Class.SchoolId,
                    SchoolName = tc.Class.School?.SchoolName ?? "Unknown",
                    CreatedAt = tc.Class.CreatedAt,
                    StudentCount = tc.Class.Students?.Count ?? 0
                }).ToList() ?? new List<ClassResponseDto>(),
                CreatedAt = teacher.CreatedAt
            };
        }
    }
}
