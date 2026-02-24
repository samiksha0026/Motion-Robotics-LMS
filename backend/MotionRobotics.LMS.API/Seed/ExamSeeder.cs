using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.Models;

namespace MotionRobotics.LMS.API.Seed
{
    /// <summary>
    /// Seeds exams for each robotics level.
    /// Each level has one final exam that unlocks after all experiments are completed.
    /// </summary>
    public static class ExamSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Only seed if no exams exist and robotics levels are seeded
            if (context.Exams.Any())
            {
                Console.WriteLine("   - Exams already seeded.");
                return;
            }

            // Check if RoboticsLevels exist
            var levels = await context.RoboticsLevels.ToListAsync();
            if (!levels.Any())
            {
                Console.WriteLine("   ! Cannot seed exams - RoboticsLevels not found. Run AdminSeeder first.");
                return;
            }

            var exams = new List<Exam>();

            foreach (var level in levels)
            {
                exams.Add(new Exam
                {
                    RoboticsLevelId = level.Id,
                    Title = $"{level.Name} Final Assessment",
                    Description = $"Final assessment for {level.Name} level. Complete all experiments to unlock.",
                    DurationMinutes = 30,
                    TotalQuestions = 20,
                    TotalMarks = 100,
                    PassingPercentage = 40,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            context.Exams.AddRange(exams);
            await context.SaveChangesAsync();

            Console.WriteLine($"   + Created {exams.Count} exams (one per level).");
        }
    }
}
