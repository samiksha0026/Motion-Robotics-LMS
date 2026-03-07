using Microsoft.AspNetCore.Identity;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MotionRobotics.LMS.API.Services.Admin
{
    public class AdminStudentService : IAdminStudentService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminStudentService(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<StudentResponseDto> CreateStudentAsync(StudentCreateDto dto)
        {
            // Validation: Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new InvalidOperationException("User with this email already exists");

            // Validation: Check if school exists
            var school = await _context.Schools.FindAsync(dto.SchoolId);
            if (school == null)
                throw new KeyNotFoundException("School not found");

            // Validation: Check if class exists and belongs to school
            var @class = await _context.Classes.FindAsync(dto.ClassId);
            if (@class == null || @class.SchoolId != dto.SchoolId)
                throw new KeyNotFoundException("Class not found in this school");

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

            // Assign Student role
            await _userManager.AddToRoleAsync(identityUser, "Student");

            // Get current academic year
            var currentYear = await _context.AcademicYears.FirstOrDefaultAsync(a => a.IsCurrent);

            // Create Student record
            var student = new Student
            {
                UserId = identityUser.Id,
                Email = dto.Email,
                FullName = dto.FullName,
                RollNo = dto.RollNo,
                ParentName = dto.ParentName,
                ParentPhone = dto.ParentPhone,
                ClassId = dto.ClassId,
                SchoolId = dto.SchoolId,
                CurrentAcademicYearId = currentYear?.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return await MapToDtoAsync(student, @class, school);
        }

        public async Task<StudentResponseDto?> GetStudentByIdAsync(int id)
        {
            var student = await _context.Students
                .Include(s => s.School)
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
                return null;

            return await MapToDtoAsync(student, student.Class, student.School);
        }

        public async Task<List<StudentResponseDto>> GetStudentsBySchoolAsync(int schoolId)
        {
            var students = await _context.Students
                .Where(s => s.SchoolId == schoolId)
                .Include(s => s.School)
                .Include(s => s.Class)
                .ToListAsync();

            var result = new List<StudentResponseDto>();
            foreach (var s in students)
            {
                result.Add(await MapToDtoAsync(s, s.Class, s.School));
            }
            return result;
        }

        public async Task<List<StudentResponseDto>> GetStudentsByClassAsync(int classId)
        {
            var students = await _context.Students
                .Where(s => s.ClassId == classId)
                .Include(s => s.School)
                .Include(s => s.Class)
                .ToListAsync();

            var result = new List<StudentResponseDto>();
            foreach (var s in students)
            {
                result.Add(await MapToDtoAsync(s, s.Class, s.School));
            }
            return result;
        }

        public async Task<List<StudentResponseDto>> GetAllStudentsAsync()
        {
            var students = await _context.Students
                .Include(s => s.School)
                .Include(s => s.Class)
                .ToListAsync();

            var result = new List<StudentResponseDto>();
            foreach (var s in students)
            {
                result.Add(await MapToDtoAsync(s, s.Class, s.School));
            }
            return result;
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
                return false;

            // Delete all related data first (cascade delete manually)
            // 1. Delete StudentProgress records
            var progressRecords = await _context.StudentProgress.Where(p => p.StudentId == id).ToListAsync();
            _context.StudentProgress.RemoveRange(progressRecords);

            // 2. Delete ExamResults
            var examResults = await _context.ExamResults.Where(e => e.StudentId == id).ToListAsync();
            _context.ExamResults.RemoveRange(examResults);

            // 4. Delete Certificates
            var certificates = await _context.Certificates.Where(c => c.StudentId == id).ToListAsync();
            _context.Certificates.RemoveRange(certificates);

            // 5. Delete associated IdentityUser
            var user = await _userManager.FindByEmailAsync(student.Email);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            // 6. Delete the student record
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<StudentResponseDto> UpdateStudentAsync(int id, StudentUpdateDto dto)
        {
            var student = await _context.Students
                .Include(s => s.School)
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
                throw new KeyNotFoundException("Student not found");

            // Validate class exists and belongs to school
            var @class = await _context.Classes.FindAsync(dto.ClassId);
            if (@class == null || @class.SchoolId != dto.SchoolId)
                throw new KeyNotFoundException("Class not found in this school");

            student.FullName = dto.FullName;
            student.RollNo = dto.RollNo;
            student.ClassId = dto.ClassId;
            student.ParentName = dto.ParentName;
            student.ParentPhone = dto.ParentPhone;

            await _context.SaveChangesAsync();

            await _context.Entry(student).Reference(s => s.School).LoadAsync();
            await _context.Entry(student).Reference(s => s.Class).LoadAsync();

            return await MapToDtoAsync(student, student.Class, student.School);
        }

        private async Task<StudentResponseDto> MapToDtoAsync(Student student, Class? @class, School? school)
        {
            // Get assigned robotics level from school level mapping
            RoboticsLevel? assignedLevel = null;
            var currentYear = await _context.AcademicYears.FirstOrDefaultAsync(a => a.IsCurrent);

            if (currentYear != null)
            {
                var mapping = await _context.SchoolLevelMappings
                    .Include(m => m.RoboticsLevel)
                    .FirstOrDefaultAsync(m =>
                        m.SchoolId == student.SchoolId &&
                        m.ClassId == student.ClassId &&
                        m.AcademicYearId == currentYear.Id);

                assignedLevel = mapping?.RoboticsLevel;
            }

            return new StudentResponseDto
            {
                Id = student.Id,
                Email = student.Email,
                FullName = student.FullName,
                RollNo = student.RollNo,
                ParentName = student.ParentName,
                ParentPhone = student.ParentPhone,
                ClassId = student.ClassId,
                ClassName = @class?.ClassName ?? "Unknown",
                SchoolId = student.SchoolId,
                SchoolName = school?.SchoolName ?? "Unknown",
                AssignedLevelId = assignedLevel?.Id,
                AssignedLevelName = assignedLevel?.Name,
                AssignedLevelNumber = assignedLevel?.LevelNumber,
                IsActive = student.IsActive,
                CreatedAt = student.CreatedAt
            };
        }
    }
}
