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
            try
            {
                var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                Console.WriteLine("[DEMO] === Starting Demo Data Seeder ===");

                // ─── STEP 0: NUKE all existing demo data ─────────────────
                await NukeAllDemoData(context, userManager);

                // ─── STEP 1: Ensure roles ────────────────────────────────
                foreach (var role in new[] { "SchoolAdmin", "Teacher", "Student" })
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
                }
                Console.WriteLine("[DEMO] Roles OK");

                // ─── STEP 2: Get prereqs ─────────────────────────────────
                var academicYear = await context.AcademicYears.FirstOrDefaultAsync(a => a.IsCurrent);
                if (academicYear == null) { Console.WriteLine("[DEMO] ABORT: No current academic year"); return; }

                var electronicsLevel = await context.RoboticsLevels.FirstOrDefaultAsync(l => l.LevelNumber == 2);
                if (electronicsLevel == null) { Console.WriteLine("[DEMO] ABORT: No Electronics level"); return; }
                Console.WriteLine("[DEMO] Prereqs OK");

                // ─── STEP 3: Create all 4 Identity users fresh ───────────
                var adminUser = await CreateFreshUser(userManager, "admin@demo.com", "Admin@123", "SchoolAdmin");
                var teacherUser = await CreateFreshUser(userManager, "teacher@demo.com", "Teacher@123", "Teacher");
                var student1User = await CreateFreshUser(userManager, "student@demo.com", "Student@123", "Student");
                var student2User = await CreateFreshUser(userManager, "student2@demo.com", "Student@123", "Student");

                if (adminUser == null || teacherUser == null || student1User == null || student2User == null)
                {
                    Console.WriteLine("[DEMO] ABORT: Failed to create one or more users");
                    return;
                }

                // ─── STEP 4: School ──────────────────────────────────────
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

                // ─── STEP 6: Teacher record ──────────────────────────────
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

                // ─── STEP 7: TeacherClasses ──────────────────────────────
                context.TeacherClasses.AddRange(
                    new TeacherClass { TeacherId = teacher.Id, ClassId = class6A.Id, AssignedAt = DateTime.UtcNow },
                    new TeacherClass { TeacherId = teacher.Id, ClassId = class7B.Id, AssignedAt = DateTime.UtcNow }
                );
                await context.SaveChangesAsync();
                Console.WriteLine("[DEMO] Teacher assigned to classes");

                // ─── STEP 8: Students ────────────────────────────────────
                context.Students.AddRange(
                    new Student
                    {
                        UserId = student1User.Id, Email = "student@demo.com", FullName = "Arjun Patel",
                        RollNo = "DEMO-001", ParentName = "Ramesh Patel", ParentPhone = "9876543212",
                        SchoolId = school.Id, ClassId = class6A.Id, CurrentAcademicYearId = academicYear.Id,
                        IsActive = true, CreatedAt = DateTime.UtcNow
                    },
                    new Student
                    {
                        UserId = student2User.Id, Email = "student2@demo.com", FullName = "Ananya Singh",
                        RollNo = "DEMO-002", ParentName = "Vikram Singh", ParentPhone = "9876543213",
                        SchoolId = school.Id, ClassId = class7B.Id, CurrentAcademicYearId = academicYear.Id,
                        IsActive = true, CreatedAt = DateTime.UtcNow
                    }
                );
                await context.SaveChangesAsync();
                Console.WriteLine("[DEMO] Students created");

                // ─── STEP 9: Level mapping ───────────────────────────────
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

                Console.WriteLine("[DEMO] === COMPLETE ===");
                Console.WriteLine("[DEMO] admin@demo.com / Admin@123");
                Console.WriteLine("[DEMO] teacher@demo.com / Teacher@123");
                Console.WriteLine("[DEMO] student@demo.com / Student@123");
                Console.WriteLine("[DEMO] student2@demo.com / Student@123");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEMO] FATAL ERROR: {ex.Message}");
                Console.WriteLine($"[DEMO] Stack: {ex.StackTrace}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[DEMO] Inner: {ex.InnerException.Message}");
            }
        }

        /// <summary>
        /// Nuclear cleanup using RAW SQL for maximum reliability.
        /// Deletes in strict FK order to avoid constraint violations.
        /// </summary>
        private static async Task NukeAllDemoData(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            Console.WriteLine("[DEMO] Nuking existing demo data via raw SQL...");

            // Use raw SQL — bypasses all EF Core tracking issues and handles FK order perfectly
            var emailList = "'" + string.Join("','", DemoEmails) + "'";

            // 1. Delete auth-related tables referencing AspNetUsers
            var sql1 = $@"
                DELETE FROM ""UserSessions""  WHERE ""UserId"" IN (SELECT ""Id"" FROM ""AspNetUsers"" WHERE ""NormalizedEmail"" IN ({string.Join(",", DemoEmails.Select(e => $"'{e.ToUpperInvariant()}'"))}));
                DELETE FROM ""RefreshTokens"" WHERE ""UserId"" IN (SELECT ""Id"" FROM ""AspNetUsers"" WHERE ""NormalizedEmail"" IN ({string.Join(",", DemoEmails.Select(e => $"'{e.ToUpperInvariant()}'"))}));
            ";
            await context.Database.ExecuteSqlRawAsync(sql1);
            Console.WriteLine("[DEMO] Auth tables cleaned");

            // 2. Delete school-related data (by school code)
            var sql2 = @"
                -- ExamResults, Certificates, StudentProgress for students in demo school
                DELETE FROM ""ExamResults""     WHERE ""StudentId"" IN (SELECT ""Id"" FROM ""Students"" WHERE ""SchoolId"" IN (SELECT ""Id"" FROM ""Schools"" WHERE ""SchoolCode"" = 'DEMO001'));
                DELETE FROM ""Certificates""    WHERE ""StudentId"" IN (SELECT ""Id"" FROM ""Students"" WHERE ""SchoolId"" IN (SELECT ""Id"" FROM ""Schools"" WHERE ""SchoolCode"" = 'DEMO001'));
                DELETE FROM ""StudentProgress"" WHERE ""StudentId"" IN (SELECT ""Id"" FROM ""Students"" WHERE ""SchoolId"" IN (SELECT ""Id"" FROM ""Schools"" WHERE ""SchoolCode"" = 'DEMO001'));

                -- ClassExperimentUnlocks for classes in demo school
                DELETE FROM ""ClassExperimentUnlocks"" WHERE ""ClassId"" IN (SELECT ""Id"" FROM ""Classes"" WHERE ""SchoolId"" IN (SELECT ""Id"" FROM ""Schools"" WHERE ""SchoolCode"" = 'DEMO001'));

                -- TeacherClasses for teachers in demo school
                DELETE FROM ""TeacherClasses"" WHERE ""TeacherId"" IN (SELECT ""Id"" FROM ""Teachers"" WHERE ""SchoolId"" IN (SELECT ""Id"" FROM ""Schools"" WHERE ""SchoolCode"" = 'DEMO001'));

                -- SchoolLevelMappings
                DELETE FROM ""SchoolLevelMappings"" WHERE ""SchoolId"" IN (SELECT ""Id"" FROM ""Schools"" WHERE ""SchoolCode"" = 'DEMO001');

                -- Students, Teachers, Classes
                DELETE FROM ""Students"" WHERE ""SchoolId"" IN (SELECT ""Id"" FROM ""Schools"" WHERE ""SchoolCode"" = 'DEMO001');
                DELETE FROM ""Teachers"" WHERE ""SchoolId"" IN (SELECT ""Id"" FROM ""Schools"" WHERE ""SchoolCode"" = 'DEMO001');
                DELETE FROM ""Classes""  WHERE ""SchoolId"" IN (SELECT ""Id"" FROM ""Schools"" WHERE ""SchoolCode"" = 'DEMO001');

                -- School itself
                DELETE FROM ""Schools"" WHERE ""SchoolCode"" = 'DEMO001';
            ";
            await context.Database.ExecuteSqlRawAsync(sql2);
            Console.WriteLine("[DEMO] School data cleaned");

            // 3. Delete Identity user data (roles, claims, logins, tokens, then user)
            var normalizedEmails = string.Join(",", DemoEmails.Select(e => $"'{e.ToUpperInvariant()}'"));
            var sql3 = $@"
                DELETE FROM ""AspNetUserRoles""  WHERE ""UserId"" IN (SELECT ""Id"" FROM ""AspNetUsers"" WHERE ""NormalizedEmail"" IN ({normalizedEmails}));
                DELETE FROM ""AspNetUserClaims"" WHERE ""UserId"" IN (SELECT ""Id"" FROM ""AspNetUsers"" WHERE ""NormalizedEmail"" IN ({normalizedEmails}));
                DELETE FROM ""AspNetUserLogins"" WHERE ""UserId"" IN (SELECT ""Id"" FROM ""AspNetUsers"" WHERE ""NormalizedEmail"" IN ({normalizedEmails}));
                DELETE FROM ""AspNetUserTokens"" WHERE ""UserId"" IN (SELECT ""Id"" FROM ""AspNetUsers"" WHERE ""NormalizedEmail"" IN ({normalizedEmails}));
                DELETE FROM ""AspNetUsers""      WHERE ""NormalizedEmail"" IN ({normalizedEmails});
            ";
            await context.Database.ExecuteSqlRawAsync(sql3);
            Console.WriteLine("[DEMO] Identity users deleted");

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
