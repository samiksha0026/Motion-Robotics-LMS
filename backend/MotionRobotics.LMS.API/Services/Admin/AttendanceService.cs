using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Models;
using MotionRobotics.LMS.API.Repositories.Admin;

namespace MotionRobotics.LMS.API.Services.Admin
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly ApplicationDbContext _context;

        public AttendanceService(IAttendanceRepository attendanceRepository, ApplicationDbContext context)
        {
            _attendanceRepository = attendanceRepository;
            _context = context;
        }

        public async Task<AttendanceResponseDto?> GetAttendanceByIdAsync(int id)
        {
            var attendance = await _attendanceRepository.GetByIdAsync(id);
            if (attendance == null)
                return null;

            return MapToDto(attendance);
        }

        public async Task<List<AttendanceResponseDto>> GetStudentAttendanceAsync(int studentId)
        {
            var attendances = await _attendanceRepository.GetByStudentAsync(studentId);
            return attendances.Select(MapToDto).ToList();
        }

        public async Task<List<AttendanceResponseDto>> GetClassAttendanceAsync(int classId)
        {
            var attendances = await _attendanceRepository.GetByClassAsync(classId);
            return attendances.Select(MapToDto).ToList();
        }

        public async Task<List<AttendanceResponseDto>> GetAttendanceByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var attendances = await _attendanceRepository.GetByDateRangeAsync(startDate, endDate);
            return attendances.Select(MapToDto).ToList();
        }

        public async Task<List<AttendanceResponseDto>> GetClassAttendanceByDateAsync(int classId, DateTime date)
        {
            var attendances = await _attendanceRepository.GetByClassAndDateAsync(classId, date);
            return attendances.Select(MapToDto).ToList();
        }

        public async Task<AttendanceResponseDto> RecordAttendanceAsync(int teacherId, AttendanceCreateDto dto)
        {
            // Check if attendance already exists
            var exists = await _attendanceRepository.ExistsAsync(dto.StudentId, dto.ClassId, dto.AttendanceDate);
            if (exists)
                throw new InvalidOperationException("Attendance already recorded for this student on this date");

            var attendance = new Attendance
            {
                StudentId = dto.StudentId,
                ClassId = dto.ClassId,
                TeacherId = teacherId,
                AttendanceDate = dto.AttendanceDate,
                IsPresent = dto.IsPresent,
                Remarks = dto.Remarks,
                RecordedAt = DateTime.UtcNow
            };

            var createdAttendance = await _attendanceRepository.CreateAsync(attendance);

            // Reload with navigation properties
            var result = await _attendanceRepository.GetByIdAsync(createdAttendance.Id);
            return MapToDto(result!);
        }

        public async Task<AttendanceResponseDto> UpdateAttendanceAsync(int id, AttendanceCreateDto dto)
        {
            var attendance = await _attendanceRepository.GetByIdAsync(id);
            if (attendance == null)
                throw new KeyNotFoundException("Attendance record not found");

            attendance.AttendanceDate = dto.AttendanceDate;
            attendance.IsPresent = dto.IsPresent;
            attendance.Remarks = dto.Remarks;

            var updatedAttendance = await _attendanceRepository.UpdateAsync(attendance);
            return MapToDto(updatedAttendance);
        }

        public async Task<bool> DeleteAttendanceAsync(int id)
        {
            return await _attendanceRepository.DeleteAsync(id);
        }

        public async Task<BulkAttendanceResponseDto> RecordBulkAttendanceAsync(int teacherId, BulkAttendanceCreateDto dto)
        {
            var classInfo = await _context.Classes
                .Include(c => c.School)
                .FirstOrDefaultAsync(c => c.Id == dto.ClassId);

            if (classInfo == null)
                throw new KeyNotFoundException("Class not found");

            var teacher = await _context.Teachers.FindAsync(teacherId);
            if (teacher == null)
                throw new KeyNotFoundException("Teacher not found");

            var attendances = new List<Attendance>();
            foreach (var studentAttendance in dto.Attendances)
            {
                // Skip if already exists
                var exists = await _attendanceRepository.ExistsAsync(studentAttendance.StudentId, dto.ClassId, dto.AttendanceDate);
                if (exists) continue;

                attendances.Add(new Attendance
                {
                    StudentId = studentAttendance.StudentId,
                    ClassId = dto.ClassId,
                    TeacherId = teacherId,
                    AttendanceDate = dto.AttendanceDate,
                    IsPresent = studentAttendance.IsPresent,
                    Remarks = studentAttendance.Remarks,
                    RecordedAt = DateTime.UtcNow
                });
            }

            if (attendances.Any())
                await _attendanceRepository.CreateBulkAsync(attendances);

            return new BulkAttendanceResponseDto
            {
                ClassId = dto.ClassId,
                ClassName = classInfo.ClassName,
                AttendanceDate = dto.AttendanceDate,
                TotalStudents = dto.Attendances.Count,
                PresentCount = dto.Attendances.Count(a => a.IsPresent),
                AbsentCount = dto.Attendances.Count(a => !a.IsPresent),
                RecordedBy = teacher.FullName,
                RecordedAt = DateTime.UtcNow
            };
        }

        public async Task<List<AttendanceResponseDto>> GetAllAttendanceAsync(int? schoolId = null, int? classId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var attendances = await _attendanceRepository.GetAllAsync(schoolId, classId, startDate, endDate);
            return attendances.Select(MapToDto).ToList();
        }

        public async Task<AttendanceSummaryDto> GetAttendanceSummaryAsync(int? schoolId = null, int? classId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var attendances = await _attendanceRepository.GetAllAsync(schoolId, classId, startDate, endDate);

            var dailyBreakdown = attendances
                .GroupBy(a => a.AttendanceDate.Date)
                .Select(g => new DailyAttendanceSummaryDto
                {
                    Date = g.Key,
                    PresentCount = g.Count(a => a.IsPresent),
                    AbsentCount = g.Count(a => !a.IsPresent),
                    AttendancePercentage = g.Count() > 0 ? Math.Round((double)g.Count(a => a.IsPresent) / g.Count() * 100, 2) : 0
                })
                .OrderByDescending(d => d.Date)
                .ToList();

            var totalPresent = attendances.Count(a => a.IsPresent);
            var totalAbsent = attendances.Count(a => !a.IsPresent);
            var total = attendances.Count;

            return new AttendanceSummaryDto
            {
                TotalClasses = attendances.Select(a => a.ClassId).Distinct().Count(),
                TotalAttendanceRecords = total,
                TotalPresent = totalPresent,
                TotalAbsent = totalAbsent,
                OverallAttendancePercentage = total > 0 ? Math.Round((double)totalPresent / total * 100, 2) : 0,
                DailyBreakdown = dailyBreakdown
            };
        }

        public async Task<StudentAttendanceReportDto> GetStudentAttendanceReportAsync(int studentId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var student = await _context.Students
                .Include(s => s.School)
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
                throw new KeyNotFoundException("Student not found");

            List<Attendance> attendances;
            if (startDate.HasValue && endDate.HasValue)
                attendances = await _attendanceRepository.GetByStudentAndDateRangeAsync(studentId, startDate.Value, endDate.Value);
            else
                attendances = await _attendanceRepository.GetByStudentAsync(studentId);

            var presentDays = attendances.Count(a => a.IsPresent);
            var absentDays = attendances.Count(a => !a.IsPresent);
            var totalDays = attendances.Count;

            return new StudentAttendanceReportDto
            {
                StudentId = student.Id,
                StudentName = student.FullName,
                RollNumber = student.RollNo,
                ClassName = student.Class?.ClassName ?? "Not Assigned",
                SchoolName = student.School?.SchoolName ?? "Unknown",
                TotalDays = totalDays,
                PresentDays = presentDays,
                AbsentDays = absentDays,
                AttendancePercentage = totalDays > 0 ? Math.Round((double)presentDays / totalDays * 100, 2) : 0,
                AttendanceHistory = attendances.Select(a => new AttendanceRecordDto
                {
                    Date = a.AttendanceDate,
                    IsPresent = a.IsPresent,
                    Remarks = a.Remarks,
                    RecordedBy = a.Teacher?.FullName ?? "Unknown"
                }).ToList()
            };
        }

        public async Task<ClassAttendanceReportDto> GetClassAttendanceReportAsync(int classId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var classInfo = await _context.Classes
                .Include(c => c.School)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classInfo == null)
                throw new KeyNotFoundException("Class not found");

            var students = await _context.Students
                .Where(s => s.ClassId == classId && s.IsActive)
                .ToListAsync();

            List<Attendance> attendances;
            if (startDate.HasValue && endDate.HasValue)
                attendances = await _attendanceRepository.GetByClassAndDateRangeAsync(classId, startDate.Value, endDate.Value);
            else
                attendances = await _attendanceRepository.GetByClassAsync(classId);

            var totalDays = attendances.Select(a => a.AttendanceDate.Date).Distinct().Count();

            var studentAttendances = students.Select(student =>
            {
                var studentRecords = attendances.Where(a => a.StudentId == student.Id).ToList();
                var present = studentRecords.Count(a => a.IsPresent);
                var absent = studentRecords.Count(a => !a.IsPresent);
                var total = studentRecords.Count;

                return new StudentAttendanceSummaryDto
                {
                    StudentId = student.Id,
                    StudentName = student.FullName,
                    RollNumber = student.RollNo,
                    PresentDays = present,
                    AbsentDays = absent,
                    AttendancePercentage = total > 0 ? Math.Round((double)present / total * 100, 2) : 0
                };
            }).OrderBy(s => s.StudentName).ToList();

            var avgAttendance = studentAttendances.Any() ? Math.Round(studentAttendances.Average(s => s.AttendancePercentage), 2) : 0;

            return new ClassAttendanceReportDto
            {
                ClassId = classInfo.Id,
                ClassName = classInfo.ClassName,
                SchoolName = classInfo.School?.SchoolName ?? "Unknown",
                LevelName = "N/A",  // Level determined by school mapping
                TotalStudents = students.Count,
                TotalDays = totalDays,
                AverageAttendancePercentage = avgAttendance,
                StudentAttendances = studentAttendances
            };
        }

        public async Task<SchoolAttendanceReportDto> GetSchoolAttendanceReportAsync(int schoolId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var school = await _context.Schools.FirstOrDefaultAsync(s => s.Id == schoolId);
            if (school == null)
                throw new KeyNotFoundException("School not found");

            var classes = await _context.Classes
                .Where(c => c.SchoolId == schoolId)
                .ToListAsync();

            var students = await _context.Students
                .Where(s => s.SchoolId == schoolId && s.IsActive)
                .CountAsync();

            List<Attendance> attendances;
            if (startDate.HasValue && endDate.HasValue)
                attendances = await _attendanceRepository.GetBySchoolAndDateRangeAsync(schoolId, startDate.Value, endDate.Value);
            else
                attendances = await _attendanceRepository.GetBySchoolAsync(schoolId);

            var totalPresent = attendances.Count(a => a.IsPresent);
            var totalRecords = attendances.Count;
            var avgPercentage = totalRecords > 0 ? Math.Round((double)totalPresent / totalRecords * 100, 2) : 0;

            var classAttendances = new List<ClassAttendanceSummaryDto>();
            foreach (var cls in classes)
            {
                var classRecords = attendances.Where(a => a.ClassId == cls.Id).ToList();
                var classStudents = await _context.Students.CountAsync(s => s.ClassId == cls.Id && s.IsActive);
                var classPresent = classRecords.Count(a => a.IsPresent);
                var classTotal = classRecords.Count;

                classAttendances.Add(new ClassAttendanceSummaryDto
                {
                    ClassId = cls.Id,
                    ClassName = cls.ClassName,
                    TotalStudents = classStudents,
                    AttendancePercentage = classTotal > 0 ? Math.Round((double)classPresent / classTotal * 100, 2) : 0
                });
            }

            return new SchoolAttendanceReportDto
            {
                SchoolId = school.Id,
                SchoolName = school.SchoolName,
                TotalStudents = students,
                TotalClasses = classes.Count,
                TotalAttendanceRecords = totalRecords,
                AverageAttendancePercentage = avgPercentage,
                ClassAttendances = classAttendances.OrderBy(c => c.ClassName).ToList()
            };
        }

        private AttendanceResponseDto MapToDto(Attendance attendance)
        {
            return new AttendanceResponseDto
            {
                Id = attendance.Id,
                StudentId = attendance.StudentId,
                StudentName = attendance.Student?.FullName ?? "Unknown",
                ClassId = attendance.ClassId,
                ClassName = attendance.Class?.ClassName ?? "Unknown",
                TeacherId = attendance.TeacherId,
                TeacherName = attendance.Teacher?.FullName ?? "Unknown",
                AttendanceDate = attendance.AttendanceDate,
                IsPresent = attendance.IsPresent,
                Remarks = attendance.Remarks,
                RecordedAt = attendance.RecordedAt
            };
        }
    }
}
