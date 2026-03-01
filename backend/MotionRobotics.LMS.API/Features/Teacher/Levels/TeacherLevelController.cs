using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.Models;
using System.Security.Claims;

namespace MotionRobotics.LMS.API.Features.Teacher.Levels
{
    /// <summary>
    /// Teacher endpoints for assigning robotics levels to classes.
    /// </summary>
    [ApiController]
    [Route("api/teacher/levels")]
    [Authorize(Roles = "Teacher")]
    public class TeacherLevelController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TeacherLevelController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Get all robotics levels available for assignment
        /// </summary>
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableLevels()
        {
            var levels = await _context.RoboticsLevels
                .OrderBy(l => l.LevelNumber)
                .Select(l => new
                {
                    l.Id,
                    l.LevelNumber,
                    l.Name,
                    l.Description,
                    l.SyllabusUrl,
                    ExperimentCount = l.Experiments.Count
                })
                .ToListAsync();

            return Ok(levels);
        }

        /// <summary>
        /// Get teacher's assigned classes with their current level mappings
        /// </summary>
        [HttpGet("my-classes")]
        public async Task<IActionResult> GetMyClassesWithLevels()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
                return NotFound(new { message = "Teacher not found" });

            // Get current academic year
            var currentYear = await _context.AcademicYears
                .FirstOrDefaultAsync(ay => ay.IsCurrent);

            if (currentYear == null)
                return BadRequest(new { message = "No current academic year set" });

            // Get classes assigned to this teacher
            var teacherClasses = await _context.TeacherClasses
                .Where(tc => tc.TeacherId == teacher.Id)
                .Include(tc => tc.Class)
                    .ThenInclude(c => c.School)
                .Select(tc => tc.Class)
                .ToListAsync();

            var result = new List<object>();
            foreach (var cls in teacherClasses)
            {
                // Get existing level mapping for this class
                var mapping = await _context.SchoolLevelMappings
                    .Include(m => m.RoboticsLevel)
                    .FirstOrDefaultAsync(m =>
                        m.ClassId == cls.Id &&
                        m.AcademicYearId == currentYear.Id);

                result.Add(new
                {
                    classId = cls.Id,
                    className = cls.ClassName,
                    schoolId = cls.SchoolId,
                    schoolName = cls.School?.SchoolName,
                    studentCount = await _context.Students.CountAsync(s => s.ClassId == cls.Id),
                    assignedLevel = mapping != null ? new
                    {
                        mappingId = mapping.Id,
                        levelId = mapping.RoboticsLevelId,
                        levelNumber = mapping.RoboticsLevel?.LevelNumber,
                        levelName = mapping.RoboticsLevel?.Name
                    } : null
                });
            }

            return Ok(new
            {
                academicYear = new { currentYear.Id, currentYear.YearName },
                classes = result
            });
        }

        /// <summary>
        /// Assign a robotics level to a class
        /// </summary>
        [HttpPost("assign")]
        public async Task<IActionResult> AssignLevelToClass([FromBody] AssignLevelDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
                return NotFound(new { message = "Teacher not found" });

            // Verify teacher is assigned to this class
            var isAssigned = await _context.TeacherClasses
                .AnyAsync(tc => tc.TeacherId == teacher.Id && tc.ClassId == dto.ClassId);

            if (!isAssigned)
                return Forbid("You are not assigned to this class");

            // Verify level exists
            var level = await _context.RoboticsLevels.FindAsync(dto.RoboticsLevelId);
            if (level == null)
                return NotFound(new { message = "Robotics level not found" });

            // Get class details
            var cls = await _context.Classes
                .Include(c => c.School)
                .FirstOrDefaultAsync(c => c.Id == dto.ClassId);

            if (cls == null)
                return NotFound(new { message = "Class not found" });

            // Get current academic year
            var currentYear = await _context.AcademicYears
                .FirstOrDefaultAsync(ay => ay.IsCurrent);

            if (currentYear == null)
                return BadRequest(new { message = "No current academic year set" });

            // Check if mapping already exists
            var existingMapping = await _context.SchoolLevelMappings
                .FirstOrDefaultAsync(m =>
                    m.ClassId == dto.ClassId &&
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
                    ClassId = dto.ClassId,
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
                classId = dto.ClassId,
                className = cls.ClassName,
                levelId = dto.RoboticsLevelId,
                levelName = level.Name
            });
        }

        /// <summary>
        /// Remove level assignment from a class
        /// </summary>
        [HttpDelete("assign/{classId}")]
        public async Task<IActionResult> RemoveLevelFromClass(int classId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
                return NotFound(new { message = "Teacher not found" });

            // Verify teacher is assigned to this class
            var isAssigned = await _context.TeacherClasses
                .AnyAsync(tc => tc.TeacherId == teacher.Id && tc.ClassId == classId);

            if (!isAssigned)
                return Forbid("You are not assigned to this class");

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

        /// <summary>
        /// Get all experiments for a class's assigned level with unlock status
        /// </summary>
        [HttpGet("classes/{classId}/experiments")]
        public async Task<IActionResult> GetClassExperiments(int classId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
                return NotFound(new { message = "Teacher not found" });

            // Verify teacher is assigned to this class
            var isAssigned = await _context.TeacherClasses
                .AnyAsync(tc => tc.TeacherId == teacher.Id && tc.ClassId == classId);

            if (!isAssigned)
                return Forbid("You are not assigned to this class");

            // Get class with school
            var cls = await _context.Classes
                .Include(c => c.School)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (cls == null)
                return NotFound(new { message = "Class not found" });

            // Get current academic year
            var currentYear = await _context.AcademicYears
                .FirstOrDefaultAsync(ay => ay.IsCurrent);

            if (currentYear == null)
                return BadRequest(new { message = "No current academic year set" });

            // Get level mapping for this class
            var mapping = await _context.SchoolLevelMappings
                .Include(m => m.RoboticsLevel)
                    .ThenInclude(l => l!.Experiments)
                .FirstOrDefaultAsync(m =>
                    m.ClassId == classId &&
                    m.AcademicYearId == currentYear.Id);

            if (mapping?.RoboticsLevel == null)
                return Ok(new
                {
                    classId = classId,
                    className = cls.ClassName,
                    level = (object?)null,
                    experiments = new List<object>(),
                    message = "No robotics level assigned to this class"
                });

            var level = mapping.RoboticsLevel;
            var experiments = level.Experiments?.OrderBy(e => e.SequenceOrder).ToList() ?? new List<Experiment>();

            // Get unlocked experiments for this class
            var unlockedExperiments = await _context.ClassExperimentUnlocks
                .Where(u => u.ClassId == classId)
                .ToListAsync();

            var experimentDtos = experiments.Select(exp =>
            {
                var unlock = unlockedExperiments.FirstOrDefault(u => u.ExperimentId == exp.Id);
                return new
                {
                    experimentId = exp.Id,
                    sequenceOrder = exp.SequenceOrder,
                    title = exp.Title,
                    description = exp.Description,
                    objective = exp.Objective,
                    components = exp.Components,
                    procedure = exp.Procedure,
                    wiringDiagram = exp.WiringDiagram,
                    circuitDiagram = exp.CircuitDiagram,
                    codeSnippet = exp.CodeSnippet,
                    demoVideoUrl = exp.DemoVideoUrl,
                    safetyNotes = exp.SafetyNotes,
                    estimatedMinutes = exp.EstimatedMinutes,
                    isUnlocked = unlock != null,
                    unlockInfo = unlock != null ? new
                    {
                        unlockId = unlock.Id,
                        unlockedAt = unlock.UnlockedAt,
                        deadline = unlock.Deadline,
                        instructions = unlock.Instructions
                    } : null
                };
            }).ToList();

            return Ok(new
            {
                classId = classId,
                className = cls.ClassName,
                level = new
                {
                    levelId = level.Id,
                    levelNumber = level.LevelNumber,
                    levelName = level.Name,
                    description = level.Description,
                    syllabusUrl = level.SyllabusUrl
                },
                totalExperiments = experiments.Count,
                unlockedCount = unlockedExperiments.Count,
                experiments = experimentDtos
            });
        }

        /// <summary>
        /// Unlock an experiment for a class (makes it visible to students)
        /// </summary>
        [HttpPost("experiments/unlock")]
        public async Task<IActionResult> UnlockExperiment([FromBody] UnlockExperimentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
                return NotFound(new { message = "Teacher not found" });

            // Verify teacher is assigned to this class
            var isAssigned = await _context.TeacherClasses
                .AnyAsync(tc => tc.TeacherId == teacher.Id && tc.ClassId == dto.ClassId);

            if (!isAssigned)
                return Forbid("You are not assigned to this class");

            // Verify experiment exists
            var experiment = await _context.Experiments.FindAsync(dto.ExperimentId);
            if (experiment == null)
                return NotFound(new { message = "Experiment not found" });

            // Verify the experiment belongs to the class's assigned level
            var currentYear = await _context.AcademicYears.FirstOrDefaultAsync(ay => ay.IsCurrent);
            if (currentYear == null)
                return BadRequest(new { message = "No current academic year set" });

            var mapping = await _context.SchoolLevelMappings
                .FirstOrDefaultAsync(m =>
                    m.ClassId == dto.ClassId &&
                    m.AcademicYearId == currentYear.Id);

            if (mapping == null || mapping.RoboticsLevelId != experiment.RoboticsLevelId)
                return BadRequest(new { message = "Experiment does not belong to the class's assigned level" });

            // Check if already unlocked
            var existingUnlock = await _context.ClassExperimentUnlocks
                .FirstOrDefaultAsync(u => u.ClassId == dto.ClassId && u.ExperimentId == dto.ExperimentId);

            if (existingUnlock != null)
            {
                // Update existing unlock
                existingUnlock.Deadline = dto.Deadline;
                existingUnlock.Instructions = dto.Instructions;
                _context.ClassExperimentUnlocks.Update(existingUnlock);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Experiment unlock updated",
                    unlockId = existingUnlock.Id,
                    experimentId = dto.ExperimentId,
                    classId = dto.ClassId
                });
            }

            // Create new unlock
            var unlock = new ClassExperimentUnlock
            {
                ClassId = dto.ClassId,
                ExperimentId = dto.ExperimentId,
                UnlockedByTeacherId = teacher.Id,
                UnlockedAt = DateTime.UtcNow,
                Deadline = dto.Deadline,
                Instructions = dto.Instructions
            };

            _context.ClassExperimentUnlocks.Add(unlock);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Experiment unlocked for class",
                unlockId = unlock.Id,
                experimentId = dto.ExperimentId,
                experimentTitle = experiment.Title,
                classId = dto.ClassId
            });
        }

        /// <summary>
        /// Lock an experiment (remove unlock) for a class
        /// </summary>
        [HttpDelete("experiments/unlock/{unlockId}")]
        public async Task<IActionResult> LockExperiment(int unlockId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
                return NotFound(new { message = "Teacher not found" });

            var unlock = await _context.ClassExperimentUnlocks
                .Include(u => u.Experiment)
                .FirstOrDefaultAsync(u => u.Id == unlockId);

            if (unlock == null)
                return NotFound(new { message = "Unlock not found" });

            // Verify teacher is assigned to this class
            var isAssigned = await _context.TeacherClasses
                .AnyAsync(tc => tc.TeacherId == teacher.Id && tc.ClassId == unlock.ClassId);

            if (!isAssigned)
                return Forbid("You are not assigned to this class");

            _context.ClassExperimentUnlocks.Remove(unlock);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Experiment locked for class",
                experimentId = unlock.ExperimentId,
                experimentTitle = unlock.Experiment?.Title,
                classId = unlock.ClassId
            });
        }

        /// <summary>
        /// Bulk unlock multiple experiments for a class
        /// </summary>
        [HttpPost("experiments/unlock-bulk")]
        public async Task<IActionResult> BulkUnlockExperiments([FromBody] BulkUnlockDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
                return NotFound(new { message = "Teacher not found" });

            // Verify teacher is assigned to this class
            var isAssigned = await _context.TeacherClasses
                .AnyAsync(tc => tc.TeacherId == teacher.Id && tc.ClassId == dto.ClassId);

            if (!isAssigned)
                return Forbid("You are not assigned to this class");

            // Get current academic year
            var currentYear = await _context.AcademicYears.FirstOrDefaultAsync(ay => ay.IsCurrent);
            if (currentYear == null)
                return BadRequest(new { message = "No current academic year set" });

            var mapping = await _context.SchoolLevelMappings
                .FirstOrDefaultAsync(m =>
                    m.ClassId == dto.ClassId &&
                    m.AcademicYearId == currentYear.Id);

            if (mapping == null)
                return BadRequest(new { message = "Class has no assigned level" });

            // Get experiments that belong to this level
            var validExperiments = await _context.Experiments
                .Where(e => dto.ExperimentIds.Contains(e.Id) && e.RoboticsLevelId == mapping.RoboticsLevelId)
                .ToListAsync();

            // Get already unlocked experiments
            var existingUnlocks = await _context.ClassExperimentUnlocks
                .Where(u => u.ClassId == dto.ClassId && dto.ExperimentIds.Contains(u.ExperimentId))
                .ToListAsync();

            var newUnlocks = new List<ClassExperimentUnlock>();
            foreach (var exp in validExperiments)
            {
                if (!existingUnlocks.Any(u => u.ExperimentId == exp.Id))
                {
                    newUnlocks.Add(new ClassExperimentUnlock
                    {
                        ClassId = dto.ClassId,
                        ExperimentId = exp.Id,
                        UnlockedByTeacherId = teacher.Id,
                        UnlockedAt = DateTime.UtcNow
                    });
                }
            }

            if (newUnlocks.Any())
            {
                _context.ClassExperimentUnlocks.AddRange(newUnlocks);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = $"{newUnlocks.Count} experiments unlocked for class",
                unlockedCount = newUnlocks.Count,
                alreadyUnlockedCount = existingUnlocks.Count,
                classId = dto.ClassId
            });
        }

        /// <summary>
        /// Get student progress for all experiments in a class
        /// </summary>
        [HttpGet("classes/{classId}/student-progress")]
        public async Task<IActionResult> GetStudentProgress(int classId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
                return NotFound(new { message = "Teacher not found" });

            // Verify teacher is assigned to this class
            var isAssigned = await _context.TeacherClasses
                .AnyAsync(tc => tc.TeacherId == teacher.Id && tc.ClassId == classId);

            if (!isAssigned)
                return Forbid("You are not assigned to this class");

            // Get students in this class
            var students = await _context.Students
                .Where(s => s.ClassId == classId && s.IsActive)
                .Include(s => s.StudentProgresses)
                .ToListAsync();

            // Get unlocked experiments for this class
            var unlockedExperimentIds = await _context.ClassExperimentUnlocks
                .Where(u => u.ClassId == classId)
                .Select(u => u.ExperimentId)
                .ToListAsync();

            var studentProgress = students.Select(student =>
            {
                var progresses = student.StudentProgresses
                    .Where(p => unlockedExperimentIds.Contains(p.ExperimentId))
                    .ToList();

                return new
                {
                    studentId = student.Id,
                    studentName = student.FullName,
                    rollNo = student.RollNo,
                    email = student.Email,
                    totalUnlocked = unlockedExperimentIds.Count,
                    completed = progresses.Count(p => p.Completed),
                    approved = progresses.Count(p => p.IsApprovedByTeacher),
                    pendingApproval = progresses.Count(p => p.Completed && !p.IsApprovedByTeacher),
                    progressPercentage = unlockedExperimentIds.Count > 0
                        ? Math.Round((decimal)progresses.Count(p => p.IsApprovedByTeacher) / unlockedExperimentIds.Count * 100, 1)
                        : 0,
                    experiments = progresses.Select(p => new
                    {
                        experimentId = p.ExperimentId,
                        completed = p.Completed,
                        completedAt = p.CompletedAt,
                        isApproved = p.IsApprovedByTeacher,
                        approvedAt = p.ApprovedAt
                    }).ToList()
                };
            }).OrderBy(s => s.studentName).ToList();

            return Ok(new
            {
                classId = classId,
                totalStudents = students.Count,
                totalUnlockedExperiments = unlockedExperimentIds.Count,
                students = studentProgress
            });
        }

        /// <summary>
        /// Get progress for a specific experiment in a class
        /// </summary>
        [HttpGet("classes/{classId}/experiments/{experimentId}/progress")]
        public async Task<IActionResult> GetExperimentProgress(int classId, int experimentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
                return NotFound(new { message = "Teacher not found" });

            // Verify teacher is assigned to this class
            var isAssigned = await _context.TeacherClasses
                .AnyAsync(tc => tc.TeacherId == teacher.Id && tc.ClassId == classId);

            if (!isAssigned)
                return Forbid("You are not assigned to this class");

            // Get experiment info
            var experiment = await _context.Experiments.FindAsync(experimentId);
            if (experiment == null)
                return NotFound(new { message = "Experiment not found" });

            // Check if experiment is unlocked
            var unlock = await _context.ClassExperimentUnlocks
                .FirstOrDefaultAsync(u => u.ClassId == classId && u.ExperimentId == experimentId);

            if (unlock == null)
                return BadRequest(new { message = "Experiment is not unlocked for this class" });

            // Get students in this class with their progress for this experiment
            var students = await _context.Students
                .Where(s => s.ClassId == classId && s.IsActive)
                .Include(s => s.StudentProgresses.Where(p => p.ExperimentId == experimentId))
                .ToListAsync();

            var studentProgress = students.Select(student =>
            {
                var progress = student.StudentProgresses.FirstOrDefault();
                return new
                {
                    studentId = student.Id,
                    studentName = student.FullName,
                    rollNo = student.RollNo,
                    email = student.Email,
                    status = progress == null ? "not_started"
                           : progress.IsApprovedByTeacher ? "approved"
                           : progress.Completed ? "pending_approval"
                           : "in_progress",
                    completed = progress?.Completed ?? false,
                    completedAt = progress?.CompletedAt,
                    submissionNotes = progress?.SubmissionNotes,
                    submissionImageUrl = progress?.SubmissionImageUrl,
                    isApproved = progress?.IsApprovedByTeacher ?? false,
                    approvedAt = progress?.ApprovedAt,
                    teacherRemarks = progress?.TeacherRemarks
                };
            }).OrderBy(s => s.studentName).ToList();

            var completedCount = studentProgress.Count(s => s.completed);
            var approvedCount = studentProgress.Count(s => s.isApproved);
            var pendingCount = studentProgress.Count(s => s.status == "pending_approval");

            return Ok(new
            {
                classId = classId,
                experimentId = experimentId,
                experimentTitle = experiment.Title,
                unlockedAt = unlock.UnlockedAt,
                deadline = unlock.Deadline,
                instructions = unlock.Instructions,
                totalStudents = students.Count,
                completedCount,
                approvedCount,
                pendingApprovalCount = pendingCount,
                notStartedCount = students.Count - completedCount,
                students = studentProgress
            });
        }
    }

    public class AssignLevelDto
    {
        public int ClassId { get; set; }
        public int RoboticsLevelId { get; set; }
    }

    public class UnlockExperimentDto
    {
        public int ClassId { get; set; }
        public int ExperimentId { get; set; }
        public DateTime? Deadline { get; set; }
        public string? Instructions { get; set; }
    }

    public class BulkUnlockDto
    {
        public int ClassId { get; set; }
        public List<int> ExperimentIds { get; set; } = new();
    }
}
