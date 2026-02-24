using MotionRobotics.LMS.API.Data;

namespace MotionRobotics.LMS.API.Seed
{
    public static class StudentSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Students are created through API in real-time by admin
            // No hard-coded demo data
            Console.WriteLine("ℹ Students will be created via /api/admin/students endpoint");
            await Task.CompletedTask;
        }
    }
}

