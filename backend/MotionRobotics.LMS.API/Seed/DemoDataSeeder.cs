using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.Models;

namespace MotionRobotics.LMS.API.Seed
{
    public static class DemoDataSeeder
    {
        private static readonly string[] DemoEmails = { "admin@demo.com", "teacher@demo.com", "student@demo.com", "student2@demo.com" };

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            Console.WriteLine("[DEMO] === Starting Demo Data Seeder ===");

            // ─── QUICK CHECK: skip if all 4 users already have valid PBKDF2 hashes ─
            // This prevents the nuke-then-recreate cycle on every server restart.
            // BCrypt hashes start with "$2a$" or "$2b$"; PBKDF2 v3 starts with "AQAAAA".
            // NOTE: Run sequentially — EF Core DbContext does not allow parallel async operations.
            var earlyCheck = new List<IdentityUser?>();
            foreach (var email in DemoEmails)
                earlyCheck.Add(await userManager.FindByEmailAsync(email));

            bool allValid = earlyCheck.All(u =>
                u != null &&
                u.PasswordHash != null &&
                !u.PasswordHash.StartsWith("$2"));   // not BCrypt
            if (allValid)
            {
                Console.WriteLine("[DEMO] All demo users already exist with valid hashes — skipping seeder");
                return;
            }
            Console.WriteLine("[DEMO] One or more demo users missing or have invalid hash — running full seed");

            // ─── STEP 0: NUKE all existing demo data ─────────────────────
            await NukeAllDemoData(context);

            // ─── STEP 1: Ensure roles ─────────────────────────────────────
            try
            {
                foreach (var role in new[] { "SchoolAdmin", "Teacher", "Student" })
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
                }
                Console.WriteLine("[DEMO] Roles OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEMO] WARN Roles: {ex.Message}");
            }

            // ─── STEP 2: Create all 4 Identity users (CRITICAL — happens first) ──
            IdentityUser? adminUser = null, teacherUser = null, student1User = null, student2User = null;
            try
            {
                adminUser = await CreateFreshUser(userManager, "admin@demo.com", "Admin@123", "SchoolAdmin");
                teacherUser = await CreateFreshUser(userManager, "teacher@demo.com", "Teacher@123", "Teacher");
                student1User = await CreateFreshUser(userManager, "student@demo.com", "Student@123", "Student");
                student2User = await CreateFreshUser(userManager, "student2@demo.com", "Student@123", "Student");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEMO] FATAL creating users: {ex.Message}");
                Console.WriteLine($"[DEMO] Stack: {ex.StackTrace}");
                return;
            }

            if (adminUser == null || teacherUser == null || student1User == null || student2User == null)
            {
                Console.WriteLine("[DEMO] ABORT: One or more users failed to create (check errors above)");
                return;
            }
            Console.WriteLine("[DEMO] All 4 Identity users created successfully");

            // ─── STEP 3: School data (prereqs) ────────────────────────────
            try
            {
                // Get or create academic year
                var academicYear = await context.AcademicYears.FirstOrDefaultAsync(a => a.IsCurrent);
                if (academicYear == null)
                {
                    Console.WriteLine("[DEMO] WARN: No current academic year found — creating fallback");
                    academicYear = new AcademicYear
                    {
                        YearName = "2025-26",
                        IsCurrent = true,
                        StartDate = new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                        EndDate = new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc)
                    };
                    context.AcademicYears.Add(academicYear);
                    await context.SaveChangesAsync();
                }

                // Get electronics level (level 2)
                var electronicsLevel = await context.RoboticsLevels.FirstOrDefaultAsync(l => l.LevelNumber == 2);
                if (electronicsLevel == null)
                {
                    Console.WriteLine("[DEMO] WARN: No Electronics level found — creating fallback");
                    electronicsLevel = new RoboticsLevel
                    {
                        Name = "Electronics",
                        LevelNumber = 2,
                        Description = "Electronics and circuits module",
                        CreatedAt = DateTime.UtcNow
                    };
                    context.RoboticsLevels.Add(electronicsLevel);
                    await context.SaveChangesAsync();
                }

                Console.WriteLine($"[DEMO] Prereqs OK: year={academicYear.YearName}, level={electronicsLevel.Name}");

                // ─── STEP 4: School ─────────────────────────────────────
                var school = new School
                {
                    SchoolName = "Motion Robotics Demo School",
                    SchoolCode = "DEMO001",
                    Address = "123 Innovation Street, Tech Park",
                    City = "Mumbai",
                    State = "Maharashtra",
                    Pincode = "400001",
                    ContactEmail = "admin@demo.com",
                    ContactPhone = "9876543210",
                    PrincipalName = "Dr. Rajesh Kumar",
                    SchoolAdminUserId = adminUser.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Schools.Add(school);
                await context.SaveChangesAsync();
                Console.WriteLine($"[DEMO] School created ID={school.Id}");

                // ─── STEP 5: Classes ─────────────────────────────────────
                var class6A = new Class { ClassName = "Class 6-A", SchoolId = school.Id, CreatedAt = DateTime.UtcNow };
                var class7B = new Class { ClassName = "Class 7-B", SchoolId = school.Id, CreatedAt = DateTime.UtcNow };
                context.Classes.AddRange(class6A, class7B);
                await context.SaveChangesAsync();
                Console.WriteLine($"[DEMO] Classes created: {class6A.Id}, {class7B.Id}");

                // ─── STEP 6: Teacher record ───────────────────────────────
                var teacher = new Teacher
                {
                    UserId = teacherUser.Id,
                    Email = "teacher@demo.com",
                    FullName = "Priya Sharma",
                    PhoneNumber = "9876543211",
                    SchoolId = school.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Teachers.Add(teacher);
                await context.SaveChangesAsync();
                Console.WriteLine($"[DEMO] Teacher created ID={teacher.Id}");

                // ─── STEP 7: TeacherClasses ───────────────────────────────
                context.TeacherClasses.AddRange(
                    new TeacherClass { TeacherId = teacher.Id, ClassId = class6A.Id, AssignedAt = DateTime.UtcNow },
                    new TeacherClass { TeacherId = teacher.Id, ClassId = class7B.Id, AssignedAt = DateTime.UtcNow }
                );
                await context.SaveChangesAsync();
                Console.WriteLine("[DEMO] Teacher assigned to classes");

                // ─── STEP 8: Students ─────────────────────────────────────
                context.Students.AddRange(
                    new Student
                    {
                        UserId = student1User.Id,
                        Email = "student@demo.com",
                        FullName = "Arjun Patel",
                        RollNo = "DEMO-001",
                        ParentName = "Ramesh Patel",
                        ParentPhone = "9876543212",
                        SchoolId = school.Id,
                        ClassId = class6A.Id,
                        CurrentAcademicYearId = academicYear.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Student
                    {
                        UserId = student2User.Id,
                        Email = "student2@demo.com",
                        FullName = "Ananya Singh",
                        RollNo = "DEMO-002",
                        ParentName = "Vikram Singh",
                        ParentPhone = "9876543213",
                        SchoolId = school.Id,
                        ClassId = class7B.Id,
                        CurrentAcademicYearId = academicYear.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                );
                await context.SaveChangesAsync();
                Console.WriteLine("[DEMO] Students created");

                // ─── STEP 9: Level mapping ────────────────────────────────
                context.SchoolLevelMappings.AddRange(
                    new SchoolLevelMapping { SchoolId = school.Id, ClassId = class6A.Id, RoboticsLevelId = electronicsLevel.Id, AcademicYearId = academicYear.Id, CreatedAt = DateTime.UtcNow },
                    new SchoolLevelMapping { SchoolId = school.Id, ClassId = class7B.Id, RoboticsLevelId = electronicsLevel.Id, AcademicYearId = academicYear.Id, CreatedAt = DateTime.UtcNow }
                );
                await context.SaveChangesAsync();
                Console.WriteLine("[DEMO] Level mappings created");

                // ─── STEP 10: Unlock experiments 1-6 ─────────────────────
                var experiments = await context.Experiments
                    .Where(e => e.RoboticsLevelId == electronicsLevel.Id && e.SequenceOrder <= 6)
                    .OrderBy(e => e.SequenceOrder)
                    .ToListAsync();

                foreach (var cls in new[] { class6A, class7B })
                {
                    foreach (var exp in experiments)
                    {
                        context.ClassExperimentUnlocks.Add(new ClassExperimentUnlock
                        {
                            ClassId = cls.Id,
                            ExperimentId = exp.Id,
                            UnlockedByTeacherId = teacher.Id,
                            UnlockedAt = DateTime.UtcNow,
                            Instructions = exp.SequenceOrder == 6 ? "Watch the demo video carefully and complete the buzzer circuit experiment." : null
                        });
                    }
                }
                await context.SaveChangesAsync();
                Console.WriteLine($"[DEMO] {experiments.Count} experiments unlocked for both classes");

                Console.WriteLine("[DEMO] === SCHOOL DATA COMPLETE ===");
            }
            catch (Exception ex)
            {
                // School data creation failed, but users already exist — logins still work
                Console.WriteLine($"[DEMO] WARN school data failed: {ex.Message}");
                Console.WriteLine($"[DEMO] Stack: {ex.StackTrace}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[DEMO] Inner: {ex.InnerException.Message}");
                Console.WriteLine("[DEMO] Users were created; school data incomplete but logins should work");
            }

            Console.WriteLine("[DEMO] === SEEDER DONE ===");
            Console.WriteLine("[DEMO] admin@demo.com / Admin@123");
            Console.WriteLine("[DEMO] teacher@demo.com / Teacher@123");
            Console.WriteLine("[DEMO] student@demo.com / Student@123");
            Console.WriteLine("[DEMO] student2@demo.com / Student@123");
        }

        /// <summary>
        /// Deletes all demo data via individual raw SQL statements (one per call —
        /// Npgsql extended query protocol does not support multi-statement batches).
        /// Errors are swallowed so a missing table or no rows never blocks seeding.
        /// </summary>
        private static async Task NukeAllDemoData(ApplicationDbContext context)
        {
            Console.WriteLine("[DEMO] Nuking existing demo data...");

            var ne = string.Join(",", DemoEmails.Select(e => $"'{e.ToUpperInvariant()}'"));
            // subquery used in multiple places
            var userIdSub = $"SELECT \"Id\" FROM \"AspNetUsers\" WHERE \"NormalizedEmail\" IN ({ne})";
            var schoolIdSub = "SELECT \"Id\" FROM \"Schools\" WHERE \"SchoolCode\" = 'DEMO001'";
            var classIdSub = $"SELECT \"Id\" FROM \"Classes\" WHERE \"SchoolId\" IN ({schoolIdSub})";
            var studentIdSub = $"SELECT \"Id\" FROM \"Students\" WHERE \"SchoolId\" IN ({schoolIdSub})";
            var teacherIdSub = $"SELECT \"Id\" FROM \"Teachers\" WHERE \"SchoolId\" IN ({schoolIdSub})";

            // Each DELETE is its own ExecuteSqlRawAsync call — required for Npgsql
            var statements = new[]
            {
                // Auth session tables referencing AspNetUsers
                $"DELETE FROM \"UserSessions\"  WHERE \"UserId\" IN ({userIdSub})",
                $"DELETE FROM \"RefreshTokens\" WHERE \"UserId\" IN ({userIdSub})",

                // School-related leaf tables
                $"DELETE FROM \"ExamResults\"     WHERE \"StudentId\" IN ({studentIdSub})",
                $"DELETE FROM \"Certificates\"    WHERE \"StudentId\" IN ({studentIdSub})",
                $"DELETE FROM \"StudentProgress\" WHERE \"StudentId\" IN ({studentIdSub})",
                $"DELETE FROM \"ClassExperimentUnlocks\" WHERE \"ClassId\" IN ({classIdSub})",
                $"DELETE FROM \"TeacherClasses\"  WHERE \"TeacherId\" IN ({teacherIdSub})",
                $"DELETE FROM \"SchoolLevelMappings\" WHERE \"SchoolId\" IN ({schoolIdSub})",
                $"DELETE FROM \"Students\" WHERE \"SchoolId\" IN ({schoolIdSub})",
                $"DELETE FROM \"Teachers\" WHERE \"SchoolId\" IN ({schoolIdSub})",
                $"DELETE FROM \"Classes\"  WHERE \"SchoolId\" IN ({schoolIdSub})",
                $"DELETE FROM \"Schools\"  WHERE \"SchoolCode\" = 'DEMO001'",

                // Identity tables
                $"DELETE FROM \"AspNetUserRoles\"  WHERE \"UserId\" IN ({userIdSub})",
                $"DELETE FROM \"AspNetUserClaims\" WHERE \"UserId\" IN ({userIdSub})",
                $"DELETE FROM \"AspNetUserLogins\" WHERE \"UserId\" IN ({userIdSub})",
                $"DELETE FROM \"AspNetUserTokens\" WHERE \"UserId\" IN ({userIdSub})",
                $"DELETE FROM \"AspNetUsers\"      WHERE \"NormalizedEmail\" IN ({ne})",
            };

            foreach (var sql in statements)
            {
                try
                {
                    var rows = await context.Database.ExecuteSqlRawAsync(sql);
                    if (rows > 0)
                        Console.WriteLine($"[DEMO] Deleted {rows} row(s): {sql[..Math.Min(80, sql.Length)].Trim()}...");
                }
                catch (Exception ex)
                {
                    // Log but continue — table may not exist or other benign error
                    Console.WriteLine($"[DEMO] WARN nuke: {ex.Message.Split('\n')[0]} | SQL: {sql[..Math.Min(60, sql.Length)].Trim()}");
                }
            }

            Console.WriteLine("[DEMO] Nuke complete");
        }

        /// <summary>
        /// Creates a brand new Identity user with the given credentials and role.
        /// </summary>
        private static async Task<IdentityUser?> CreateFreshUser(UserManager<IdentityUser> userManager, string email, string password, string role)
        {
            var user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                Console.WriteLine($"[DEMO] FAILED to create {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                return null;
            }

            await userManager.AddToRoleAsync(user, role);
            Console.WriteLine($"[DEMO] Created user {email} with role {role}");
            return user;
        }
    }
}
