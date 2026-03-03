using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.Models;

namespace MotionRobotics.LMS.API.Seed
{
    /// <summary>
    /// Seeds all demo data required for client presentation:
    /// 1 School + SchoolAdmin (admin@demo.com / Admin@123)
    /// 1 Teacher (teacher@demo.com / Teacher@123)
    /// 2 Classes (Class 6A, Class 7B)
    /// 2 Students (student@demo.com / Student@123, student2@demo.com / Student@123)
    /// Electronics level assigned to both classes
    /// Experiment 6 unlocked for both classes
    /// </summary>
    public static class DemoDataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Check if fully seeded (school exists AND teacher exists AND students exist)
            var demoSchool = await context.Schools.FirstOrDefaultAsync(s => s.SchoolCode == "DEMO001");
            var demoTeacher = await context.Teachers.FirstOrDefaultAsync(t => t.Email == "teacher@demo.com");
            var demoStudent = await context.Students.FirstOrDefaultAsync(s => s.Email == "student@demo.com");

            if (demoSchool != null && demoTeacher != null && demoStudent != null)
            {
                // Verify passwords work by checking the Identity users exist with correct roles
                var adminCheck = await userManager.FindByEmailAsync("admin@demo.com");
                var teacherCheck = await userManager.FindByEmailAsync("teacher@demo.com");
                var studentCheck = await userManager.FindByEmailAsync("student@demo.com");

                if (adminCheck != null && teacherCheck != null && studentCheck != null)
                {
                    // Reset passwords to known values every startup (in case they got corrupted)
                    await ResetPasswordIfNeeded(userManager, adminCheck, "Admin@123");
                    await ResetPasswordIfNeeded(userManager, teacherCheck, "Teacher@123");
                    await ResetPasswordIfNeeded(userManager, studentCheck, "Student@123");

                    var student2Check = await userManager.FindByEmailAsync("student2@demo.com");
                    if (student2Check != null)
                        await ResetPasswordIfNeeded(userManager, student2Check, "Student@123");

                    Console.WriteLine("   ✅ Demo data already seeded. Passwords verified.");
                    return;
                }
            }

            Console.WriteLine("   🔧 Seeding demo data for client presentation...");

            // ─── 1. Ensure roles exist ───────────────────────────────────
            foreach (var role in new[] { "SchoolAdmin", "Teacher", "Student" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // ─── 2. Get existing AcademicYear and RoboticsLevel ──────────
            var academicYear = await context.AcademicYears.FirstOrDefaultAsync(a => a.IsCurrent);
            if (academicYear == null)
            {
                Console.WriteLine("   ❌ No current academic year found. Run AdminSeeder first.");
                return;
            }

            var electronicsLevel = await context.RoboticsLevels.FirstOrDefaultAsync(l => l.LevelNumber == 2);
            if (electronicsLevel == null)
            {
                Console.WriteLine("   ❌ Electronics level (LevelNumber=2) not found. Run AdminSeeder first.");
                return;
            }

            // ─── 3. Create or fix SchoolAdmin Identity User ──────────────
            var adminEmail = "admin@demo.com";
            var adminPassword = "Admin@123";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser != null)
            {
                // User exists — reset password to known value and ensure role
                await ResetPasswordIfNeeded(userManager, adminUser, adminPassword);
                var adminRoles = await userManager.GetRolesAsync(adminUser);
                if (!adminRoles.Contains("SchoolAdmin"))
                    await userManager.AddToRoleAsync(adminUser, "SchoolAdmin");
                Console.WriteLine("   ✅ SchoolAdmin user already exists, password reset to Admin@123");
            }
            else
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var adminResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (!adminResult.Succeeded)
                {
                    var errors = string.Join(", ", adminResult.Errors.Select(e => e.Description));
                    Console.WriteLine($"   ❌ Failed to create school admin user: {errors}");
                    return;
                }
                await userManager.AddToRoleAsync(adminUser, "SchoolAdmin");
                Console.WriteLine("   ✅ SchoolAdmin user created (admin@demo.com / Admin@123)");
            }

            // ─── 4. Create the Demo School (if not exists) ──────────────
            var school = await context.Schools.FirstOrDefaultAsync(s => s.SchoolCode == "DEMO001");
            if (school == null)
            {
                school = new School
                {
                    SchoolName = "Motion Robotics Demo School",
                    SchoolCode = "DEMO001",
                    Address = "123 Innovation Street, Tech Park",
                    City = "Mumbai",
                    State = "Maharashtra",
                    Pincode = "400001",
                    ContactEmail = adminEmail,
                    ContactPhone = "9876543210",
                    PrincipalName = "Dr. Rajesh Kumar",
                    SchoolAdminUserId = adminUser.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Schools.Add(school);
                await context.SaveChangesAsync();
                Console.WriteLine($"   ✅ Demo School created (ID: {school.Id})");
            }
            else
            {
                // Ensure school is linked to correct admin
                if (school.SchoolAdminUserId != adminUser.Id)
                {
                    school.SchoolAdminUserId = adminUser.Id;
                    await context.SaveChangesAsync();
                }
                Console.WriteLine($"   ✅ Demo School already exists (ID: {school.Id})");
            }

            // ─── 5. Create 2 Classes (if not exist) ─────────────────────
            var class6A = await context.Classes.FirstOrDefaultAsync(c => c.ClassName == "Class 6-A" && c.SchoolId == school.Id);
            if (class6A == null)
            {
                class6A = new Class
                {
                    ClassName = "Class 6-A",
                    SchoolId = school.Id,
                    CreatedAt = DateTime.UtcNow
                };
                context.Classes.Add(class6A);
            }

            var class7B = await context.Classes.FirstOrDefaultAsync(c => c.ClassName == "Class 7-B" && c.SchoolId == school.Id);
            if (class7B == null)
            {
                class7B = new Class
                {
                    ClassName = "Class 7-B",
                    SchoolId = school.Id,
                    CreatedAt = DateTime.UtcNow
                };
                context.Classes.Add(class7B);
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"   ✅ Classes: {class6A.ClassName} (ID: {class6A.Id}), {class7B.ClassName} (ID: {class7B.Id})");

            // ─── 6. Create Teacher Identity User + Teacher Record ────────
            var teacherEmail = "teacher@demo.com";
            var teacherPassword = "Teacher@123";

            var teacherUser = await userManager.FindByEmailAsync(teacherEmail);
            if (teacherUser == null)
            {
                teacherUser = new IdentityUser
                {
                    UserName = teacherEmail,
                    Email = teacherEmail,
                    EmailConfirmed = true
                };

                var teacherResult = await userManager.CreateAsync(teacherUser, teacherPassword);
                if (!teacherResult.Succeeded)
                {
                    var errors = string.Join(", ", teacherResult.Errors.Select(e => e.Description));
                    Console.WriteLine($"   ❌ Failed to create teacher user: {errors}");
                    return;
                }
                await userManager.AddToRoleAsync(teacherUser, "Teacher");
            }
            else
            {
                await ResetPasswordIfNeeded(userManager, teacherUser, teacherPassword);
            }

            var teacher = await context.Teachers.FirstOrDefaultAsync(t => t.Email == teacherEmail);
            if (teacher == null)
            {
                teacher = new Teacher
                {
                    UserId = teacherUser.Id,
                    Email = teacherEmail,
                    FullName = "Priya Sharma",
                    PhoneNumber = "9876543211",
                    SchoolId = school.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Teachers.Add(teacher);
                await context.SaveChangesAsync();
            }
            Console.WriteLine($"   ✅ Teacher: {teacher.FullName} (teacher@demo.com / Teacher@123)");

            // ─── 7. Assign Teacher to both Classes (if not already) ─────
            foreach (var classId in new[] { class6A.Id, class7B.Id })
            {
                if (!await context.TeacherClasses.AnyAsync(tc => tc.TeacherId == teacher.Id && tc.ClassId == classId))
                {
                    context.TeacherClasses.Add(new TeacherClass
                    {
                        TeacherId = teacher.Id,
                        ClassId = classId,
                        AssignedAt = DateTime.UtcNow
                    });
                }
            }
            await context.SaveChangesAsync();
            Console.WriteLine("   ✅ Teacher assigned to both classes");

            // ─── 8. Create 2 Student Identity Users + Student Records ────
            var studentData = new[]
            {
                new { Email = "student@demo.com", Password = "Student@123", FullName = "Arjun Patel", RollNo = "DEMO-001", ClassId = class6A.Id, ParentName = "Ramesh Patel", ParentPhone = "9876543212" },
                new { Email = "student2@demo.com", Password = "Student@123", FullName = "Ananya Singh", RollNo = "DEMO-002", ClassId = class7B.Id, ParentName = "Vikram Singh", ParentPhone = "9876543213" },
            };

            foreach (var sd in studentData)
            {
                var studentUser = await userManager.FindByEmailAsync(sd.Email);
                if (studentUser == null)
                {
                    studentUser = new IdentityUser
                    {
                        UserName = sd.Email,
                        Email = sd.Email,
                        EmailConfirmed = true
                    };

                    var studentResult = await userManager.CreateAsync(studentUser, sd.Password);
                    if (!studentResult.Succeeded)
                    {
                        var errors = string.Join(", ", studentResult.Errors.Select(e => e.Description));
                        Console.WriteLine($"   ❌ Failed to create student {sd.Email}: {errors}");
                        continue;
                    }
                    await userManager.AddToRoleAsync(studentUser, "Student");
                }
                else
                {
                    await ResetPasswordIfNeeded(userManager, studentUser, sd.Password);
                }

                if (!await context.Students.AnyAsync(s => s.Email == sd.Email))
                {
                    var student = new Student
                    {
                        UserId = studentUser.Id,
                        Email = sd.Email,
                        FullName = sd.FullName,
                        RollNo = sd.RollNo,
                        ParentName = sd.ParentName,
                        ParentPhone = sd.ParentPhone,
                        SchoolId = school.Id,
                        ClassId = sd.ClassId,
                        CurrentAcademicYearId = academicYear.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Students.Add(student);
                    await context.SaveChangesAsync();
                }
                Console.WriteLine($"   ✅ Student: {sd.FullName} ({sd.Email} / {sd.Password})");
            }

            // ─── 9. Assign Electronics Level to both Classes (if not already) ──
            foreach (var classId in new[] { class6A.Id, class7B.Id })
            {
                if (!await context.SchoolLevelMappings.AnyAsync(m => m.SchoolId == school.Id && m.ClassId == classId && m.AcademicYearId == academicYear.Id))
                {
                    context.SchoolLevelMappings.Add(new SchoolLevelMapping
                    {
                        SchoolId = school.Id,
                        ClassId = classId,
                        RoboticsLevelId = electronicsLevel.Id,
                        AcademicYearId = academicYear.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await context.SaveChangesAsync();
            Console.WriteLine($"   ✅ Electronics level (Level {electronicsLevel.LevelNumber}) assigned to both classes");

            // ─── 10. Unlock Experiment 6 for both Classes ────────────────
            var experiment6 = await context.Experiments
                .FirstOrDefaultAsync(e => e.RoboticsLevelId == electronicsLevel.Id && e.SequenceOrder == 6);

            if (experiment6 != null)
            {
                // Also unlock experiments 1-5 so students can see progression
                var experimentsToUnlock = await context.Experiments
                    .Where(e => e.RoboticsLevelId == electronicsLevel.Id && e.SequenceOrder <= 6)
                    .OrderBy(e => e.SequenceOrder)
                    .ToListAsync();

                foreach (var cls in new[] { class6A, class7B })
                {
                    foreach (var exp in experimentsToUnlock)
                    {
                        if (!await context.ClassExperimentUnlocks.AnyAsync(u => u.ClassId == cls.Id && u.ExperimentId == exp.Id))
                        {
                            context.ClassExperimentUnlocks.Add(new ClassExperimentUnlock
                            {
                                ClassId = cls.Id,
                                ExperimentId = exp.Id,
                                UnlockedByTeacherId = teacher.Id,
                                UnlockedAt = DateTime.UtcNow,
                                Instructions = exp.SequenceOrder == 6
                                    ? "Watch the demo video carefully and complete the buzzer circuit experiment."
                                    : null
                            });
                        }
                    }
                }

                await context.SaveChangesAsync();
                Console.WriteLine($"   ✅ Experiments 1-6 unlocked for both classes (Exp 6 has video)");
            }
            else
            {
                Console.WriteLine("   ⚠️  Experiment 6 not found for Electronics level. Run ExperimentSeeder first.");
            }

            Console.WriteLine("   🎉 Demo data seeding complete!");
            Console.WriteLine("   ──────────────────────────────────────────");
            Console.WriteLine("   School Admin: admin@demo.com / Admin@123");
            Console.WriteLine("   Teacher:      teacher@demo.com / Teacher@123");
            Console.WriteLine("   Student 1:    student@demo.com / Student@123");
            Console.WriteLine("   Student 2:    student2@demo.com / Student@123");
            Console.WriteLine("   ──────────────────────────────────────────");
        }

        /// <summary>
        /// Resets a user's password to the expected value using Identity's token-based flow.
        /// This ensures the password hash is always in the correct PBKDF2 format.
        /// </summary>
        private static async Task ResetPasswordIfNeeded(UserManager<IdentityUser> userManager, IdentityUser user, string expectedPassword)
        {
            // Try to verify current password first
            var passwordValid = await userManager.CheckPasswordAsync(user, expectedPassword);
            if (passwordValid)
                return; // Password is already correct

            // Reset password using token flow (proper PBKDF2 hash)
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, expectedPassword);
            if (result.Succeeded)
            {
                Console.WriteLine($"   🔑 Password reset for {user.Email}");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                Console.WriteLine($"   ⚠️  Failed to reset password for {user.Email}: {errors}");
            }
        }
    }
}
