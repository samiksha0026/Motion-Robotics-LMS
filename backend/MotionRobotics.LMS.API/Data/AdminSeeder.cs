using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MotionRobotics.LMS.API.Models;

namespace MotionRobotics.LMS.API.Data
{
    public static class AdminSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            Console.WriteLine("\n========== SEEDING DATABASE ==========\n");

            // 1. Seed Roles
            await SeedRolesAsync(roleManager);

            // 2. Seed SuperAdmin User
            var configuration = serviceProvider.GetService<IConfiguration>();
            await SeedSuperAdminAsync(userManager, configuration);

            // 3. Seed Robotics Levels
            await SeedRoboticsLevelsAsync(context);

            // 4. Seed Academic Year
            await SeedAcademicYearAsync(context);

            Console.WriteLine("\n========== SEEDING COMPLETE ==========\n");
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            Console.WriteLine("1. Seeding Roles...");

            // Role hierarchy: SuperAdmin > SchoolAdmin > Teacher > Student
            string[] roles = { "SuperAdmin", "SchoolAdmin", "Teacher", "Student" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    Console.WriteLine($"   + Created role: {role}");
                }
                else
                {
                    Console.WriteLine($"   - Role exists: {role}");
                }
            }
        }

        private static async Task SeedSuperAdminAsync(UserManager<IdentityUser> userManager, IConfiguration? configuration = null)
        {
            Console.WriteLine("\n2. Seeding SuperAdmin User...");

            // IConfiguration maps SuperAdmin__Email → SuperAdmin:Email (works for env vars, user-secrets, appsettings)
            string superAdminEmail = configuration?["SuperAdmin:Email"] ?? string.Empty;
            string superAdminPassword = configuration?["SuperAdmin:Password"] ?? string.Empty;

            // Credentials must be explicitly provided — never fall back to defaults
            if (string.IsNullOrEmpty(superAdminEmail) || string.IsNullOrEmpty(superAdminPassword))
            {
                throw new InvalidOperationException(
                    "SuperAdmin credentials missing. Set SuperAdmin__Email and SuperAdmin__Password environment variables.");
            }

            if (superAdminPassword.Length < 8)
            {
                throw new InvalidOperationException(
                    "SuperAdmin password is too short. Minimum 8 characters required.");
            }

            var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);

            if (superAdminUser == null)
            {
                superAdminUser = new IdentityUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(superAdminUser, superAdminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
                    Console.WriteLine("   + SuperAdmin account created successfully!");
                    Console.WriteLine($"     Email: {superAdminEmail}");
                    // Password intentionally NOT logged for security
                }
                else
                {
                    Console.WriteLine("   ! Failed to create SuperAdmin:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"     - {error.Description}");
                    }
                }
            }
            else
            {
                // Ensure SuperAdmin role is assigned
                var isInRole = await userManager.IsInRoleAsync(superAdminUser, "SuperAdmin");
                if (!isInRole)
                {
                    await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
                    Console.WriteLine("   + Added SuperAdmin role to existing user.");
                }
                else
                {
                    Console.WriteLine("   - SuperAdmin account already exists.");
                }
                Console.WriteLine($"     Email: {superAdminEmail}");
            }
        }

        private static async Task SeedRoboticsLevelsAsync(ApplicationDbContext context)
        {
            Console.WriteLine("\n3. Seeding Robotics Levels...");

            // Check if levels already exist
            if (await context.RoboticsLevels.AnyAsync())
            {
                Console.WriteLine("   - Robotics levels already seeded.");
                return;
            }

            // The 6 Fixed Robotics Levels
            var levels = new List<RoboticsLevel>
            {
                new RoboticsLevel
                {
                    LevelNumber = 1,
                    Name = "Mech Tech",
                    Description = "Introduction to mechanical engineering concepts and basic robotics mechanisms.",
                    SyllabusUrl = "/syllabus/Mech Tech Level-1.pdf",
                    IsActive = true
                },
                new RoboticsLevel
                {
                    LevelNumber = 2,
                    Name = "Electronics",
                    Description = "Fundamentals of electronics, circuits, and electronic components.",
                    SyllabusUrl = "/syllabus/Electronics Level 2.pdf",
                    IsActive = true
                },
                new RoboticsLevel
                {
                    LevelNumber = 3,
                    Name = "Electro Mechanical",
                    Description = "Integration of electronic and mechanical systems in robotics.",
                    SyllabusUrl = "/syllabus/Electro Mechanical Level-3.pdf",
                    IsActive = true
                },
                new RoboticsLevel
                {
                    LevelNumber = 4,
                    Name = "Digi-Coding",
                    Description = "Programming fundamentals and digital logic for robotics.",
                    SyllabusUrl = "/syllabus/Digi-Tech Coding Level 4.pdf",
                    IsActive = true
                },
                new RoboticsLevel
                {
                    LevelNumber = 5,
                    Name = "Digi-Sense",
                    Description = "Sensors, data acquisition, and environmental awareness in robotics.",
                    SyllabusUrl = "/syllabus/Digi sense Level 5.pdf",
                    IsActive = true
                },
                new RoboticsLevel
                {
                    LevelNumber = 6,
                    Name = "Wireless & IOT",
                    Description = "Wireless communication, Internet of Things, and connected robotics.",
                    SyllabusUrl = "/syllabus/WIRELESS IOT pdf.pdf",
                    IsActive = true
                }
            };

            await context.RoboticsLevels.AddRangeAsync(levels);
            await context.SaveChangesAsync();

            Console.WriteLine("   + Added 6 Robotics Levels:");
            foreach (var level in levels)
            {
                Console.WriteLine($"     Level {level.LevelNumber}: {level.Name}");
            }
        }

        private static async Task SeedAcademicYearAsync(ApplicationDbContext context)
        {
            Console.WriteLine("\n4. Seeding Academic Year...");

            // Check if academic years already exist
            if (await context.AcademicYears.AnyAsync())
            {
                Console.WriteLine("   - Academic years already seeded.");
                return;
            }

            // Create current academic year (2025-2026)
            var currentYear = new AcademicYear
            {
                YearName = "2025-2026",
                StartDate = new DateTime(2025, 6, 1),  // June 1, 2025
                EndDate = new DateTime(2026, 5, 31),    // May 31, 2026
                IsCurrent = true
            };

            await context.AcademicYears.AddAsync(currentYear);
            await context.SaveChangesAsync();

            Console.WriteLine($"   + Created Academic Year: {currentYear.YearName} (Current)");
        }

        // Legacy method for backward compatibility
        public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
        {
            await SeedAsync(serviceProvider);
        }
    }
}
