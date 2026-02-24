using MotionRobotics.LMS.API.Data;

namespace MotionRobotics.LMS.API.Seed
{
    public static class CertificateSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Certificates are issued dynamically when students pass exams
            // No need to seed template data here
            await Task.CompletedTask;
        }
    }
}
