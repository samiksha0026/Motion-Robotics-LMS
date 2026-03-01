using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Models;


namespace MotionRobotics.LMS.API.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options
        ) : base(options)
        {
        }

        // Core entities
        public DbSet<School> Schools { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<TeacherClass> TeacherClasses { get; set; }
        public DbSet<Attendance> Attendances { get; set; }

        // Robotics Level System
        public DbSet<RoboticsLevel> RoboticsLevels { get; set; }
        public DbSet<AcademicYear> AcademicYears { get; set; }
        public DbSet<SchoolLevelMapping> SchoolLevelMappings { get; set; }

        // Learning content
        public DbSet<Experiment> Experiments { get; set; }
        public DbSet<StudentProgress> StudentProgress { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<ClassExperimentUnlock> ClassExperimentUnlocks { get; set; }

        // Assessment
        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamResult> ExamResults { get; set; }
        public DbSet<Certificate> Certificates { get; set; }

        // Auth
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============ SCHOOL RELATIONSHIPS ============

            // School - Class relationship
            modelBuilder.Entity<Class>()
                .HasOne(c => c.School)
                .WithMany(s => s.Classes)
                .HasForeignKey(c => c.SchoolId)
                .OnDelete(DeleteBehavior.Cascade);

            // School - Student relationship
            modelBuilder.Entity<Student>()
                .HasOne(s => s.School)
                .WithMany(s => s.Students)
                .HasForeignKey(s => s.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            // Class - Student relationship
            modelBuilder.Entity<Student>()
                .HasOne(s => s.Class)
                .WithMany(c => c.Students)
                .HasForeignKey(s => s.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            // School - Teacher relationship
            modelBuilder.Entity<Teacher>()
                .HasOne(t => t.School)
                .WithMany(s => s.Teachers)
                .HasForeignKey(t => t.SchoolId)
                .OnDelete(DeleteBehavior.Cascade);

            // School - SchoolAdmin User relationship
            modelBuilder.Entity<School>()
                .HasOne(s => s.SchoolAdminUser)
                .WithMany()
                .HasForeignKey(s => s.SchoolAdminUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // ============ TEACHER RELATIONSHIPS ============

            // Teacher - Class relationship (many-to-many)
            modelBuilder.Entity<TeacherClass>()
                .HasOne(tc => tc.Teacher)
                .WithMany(t => t.TeacherClasses)
                .HasForeignKey(tc => tc.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeacherClass>()
                .HasOne(tc => tc.Class)
                .WithMany()
                .HasForeignKey(tc => tc.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============ ROBOTICS LEVEL SYSTEM ============

            // RoboticsLevel - Experiment relationship
            modelBuilder.Entity<Experiment>()
                .HasOne(e => e.RoboticsLevel)
                .WithMany(rl => rl.Experiments)
                .HasForeignKey(e => e.RoboticsLevelId)
                .OnDelete(DeleteBehavior.Cascade);

            // Experiment ordering index
            modelBuilder.Entity<Experiment>()
                .HasIndex(e => new { e.RoboticsLevelId, e.SequenceOrder })
                .IsUnique();

            // SchoolLevelMapping - Unique constraint (one mapping per school/class/year)
            modelBuilder.Entity<SchoolLevelMapping>()
                .HasIndex(slm => new { slm.SchoolId, slm.ClassId, slm.AcademicYearId })
                .IsUnique();

            modelBuilder.Entity<SchoolLevelMapping>()
                .HasOne(slm => slm.School)
                .WithMany(s => s.SchoolLevelMappings)
                .HasForeignKey(slm => slm.SchoolId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SchoolLevelMapping>()
                .HasOne(slm => slm.Class)
                .WithMany()
                .HasForeignKey(slm => slm.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SchoolLevelMapping>()
                .HasOne(slm => slm.RoboticsLevel)
                .WithMany(rl => rl.SchoolLevelMappings)
                .HasForeignKey(slm => slm.RoboticsLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SchoolLevelMapping>()
                .HasOne(slm => slm.AcademicYear)
                .WithMany(ay => ay.SchoolLevelMappings)
                .HasForeignKey(slm => slm.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============ STUDENT PROGRESS & APPROVAL ============

            modelBuilder.Entity<StudentProgress>()
                .HasOne(sp => sp.Student)
                .WithMany(s => s.StudentProgresses)
                .HasForeignKey(sp => sp.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentProgress>()
                .HasOne(sp => sp.Experiment)
                .WithMany(e => e.StudentProgresses)
                .HasForeignKey(sp => sp.ExperimentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentProgress>()
                .HasOne(sp => sp.ApprovedByTeacher)
                .WithMany()
                .HasForeignKey(sp => sp.ApprovedByTeacherId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<StudentProgress>()
                .HasOne(sp => sp.AcademicYear)
                .WithMany(ay => ay.StudentProgresses)
                .HasForeignKey(sp => sp.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint: One progress per student/experiment/year
            modelBuilder.Entity<StudentProgress>()
                .HasIndex(sp => new { sp.StudentId, sp.ExperimentId, sp.AcademicYearId })
                .IsUnique();

            // ============ CLASS EXPERIMENT UNLOCK ============
            // Teacher unlocks experiments for the class one by one

            modelBuilder.Entity<ClassExperimentUnlock>()
                .HasOne(ceu => ceu.Class)
                .WithMany()
                .HasForeignKey(ceu => ceu.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClassExperimentUnlock>()
                .HasOne(ceu => ceu.Experiment)
                .WithMany()
                .HasForeignKey(ceu => ceu.ExperimentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClassExperimentUnlock>()
                .HasOne(ceu => ceu.UnlockedByTeacher)
                .WithMany()
                .HasForeignKey(ceu => ceu.UnlockedByTeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint: One unlock record per class/experiment
            modelBuilder.Entity<ClassExperimentUnlock>()
                .HasIndex(ceu => new { ceu.ClassId, ceu.ExperimentId })
                .IsUnique();

            // ============ EXAM & RESULTS ============

            modelBuilder.Entity<Exam>()
                .HasOne(e => e.RoboticsLevel)
                .WithMany()
                .HasForeignKey(e => e.RoboticsLevelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExamResult>()
                .HasOne(er => er.Student)
                .WithMany()
                .HasForeignKey(er => er.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExamResult>()
                .HasOne(er => er.Exam)
                .WithMany(e => e.ExamResults)
                .HasForeignKey(er => er.ExamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExamResult>()
                .HasOne(er => er.AcademicYear)
                .WithMany()
                .HasForeignKey(er => er.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExamResult>()
                .HasOne(er => er.Certificate)
                .WithMany()
                .HasForeignKey(er => er.CertificateId)
                .OnDelete(DeleteBehavior.SetNull);

            // ============ CERTIFICATE ============

            modelBuilder.Entity<Certificate>()
                .HasOne(c => c.Student)
                .WithMany()
                .HasForeignKey(c => c.StudentId)
                .OnDelete(DeleteBehavior.Restrict);  // Changed to Restrict to avoid cascade cycles

            modelBuilder.Entity<Certificate>()
                .HasOne(c => c.School)
                .WithMany()
                .HasForeignKey(c => c.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Certificate>()
                .HasOne(c => c.RoboticsLevel)
                .WithMany()
                .HasForeignKey(c => c.RoboticsLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Certificate>()
                .HasOne(c => c.AcademicYear)
                .WithMany()
                .HasForeignKey(c => c.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Certificate>()
                .HasIndex(c => c.CertificateNumber)
                .IsUnique();

            // ============ ATTENDANCE ============

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Student)
                .WithMany(s => s.Attendances)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Class)
                .WithMany()
                .HasForeignKey(a => a.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Teacher)
                .WithMany(t => t.Attendances)
                .HasForeignKey(a => a.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============ STUDENT - ACADEMIC YEAR ============

            modelBuilder.Entity<Student>()
                .HasOne(s => s.CurrentAcademicYear)
                .WithMany()
                .HasForeignKey(s => s.CurrentAcademicYearId)
                .OnDelete(DeleteBehavior.SetNull);

            // ============ REFRESH TOKEN ============

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.UserId);

            // ============ USER SESSIONS ============

            modelBuilder.Entity<UserSession>()
                .HasKey(us => us.Id);

            modelBuilder.Entity<UserSession>()
                .HasOne(us => us.User)
                .WithMany()
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserSession>()
                .HasIndex(us => us.UserId);

            modelBuilder.Entity<UserSession>()
                .HasIndex(us => us.TokenHash);

            modelBuilder.Entity<UserSession>()
                .HasIndex(us => new { us.UserId, us.IsActive });

            // ============ INDEXES FOR PERFORMANCE ============

            modelBuilder.Entity<Student>()
                .HasIndex(s => s.Email)
                .IsUnique();

            modelBuilder.Entity<Teacher>()
                .HasIndex(t => t.Email)
                .IsUnique();

            modelBuilder.Entity<School>()
                .HasIndex(s => s.SchoolCode)
                .IsUnique();

            modelBuilder.Entity<RoboticsLevel>()
                .HasIndex(rl => rl.LevelNumber)
                .IsUnique();

            // ============ DECIMAL PRECISION ============

            modelBuilder.Entity<Certificate>()
                .Property(c => c.ExamScore)
                .HasPrecision(5, 2);  // e.g., 100.00

            modelBuilder.Entity<Certificate>()
                .Property(c => c.PassingScore)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Exam>()
                .Property(e => e.TotalMarks)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Exam>()
                .Property(e => e.PassingPercentage)
                .HasPrecision(5, 2);

            modelBuilder.Entity<ExamResult>()
                .Property(er => er.ScoreObtained)
                .HasPrecision(5, 2);

            modelBuilder.Entity<ExamResult>()
                .Property(er => er.TotalMarks)
                .HasPrecision(5, 2);

            modelBuilder.Entity<ExamResult>()
                .Property(er => er.Percentage)
                .HasPrecision(5, 2);
        }
    }
}


