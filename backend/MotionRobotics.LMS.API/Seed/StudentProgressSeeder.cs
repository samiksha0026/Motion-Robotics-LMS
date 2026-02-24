using MotionRobotics.LMS.API.Data;

namespace MotionRobotics.LMS.API.Seed
{
    public static class StudentProgressSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Student progress is tracked as they complete experiments
            // No need to seed initial progress data
            await Task.CompletedTask;
        }
    }
}
