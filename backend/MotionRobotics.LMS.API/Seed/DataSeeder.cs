using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.Seed;

public static class DataSeeder
{
    public static async Task SeedDatabaseAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        await RoleAndAdminSeeder.SeedRolesAndAdminAsync(services);
        await BookSeeder.SeedAsync(context);
        await ExamSeeder.SeedAsync(context);
        await ExperimentSeeder.SeedAsync(context);
        await CertificateSeeder.SeedAsync(context);
        await StudentSeeder.SeedAsync(context);
        await StudentProgressSeeder.SeedAsync(context);
    }
}
