// ============================================================
// Program.cs
// Sets up: ASP.NET Identity + JWT Auth + CORS for Next.js
// Replace your existing Program.cs with this
// ============================================================
// NuGet packages needed (install all of these):
//   Microsoft.AspNetCore.Identity.EntityFrameworkCore
//   Microsoft.AspNetCore.Authentication.JwtBearer
//   Microsoft.IdentityModel.Tokens
//   Microsoft.EntityFrameworkCore.SqlServer
//   BCrypt.Net-Next
// ============================================================

using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.Middleware;
using MotionRobotics.LMS.API.Repositories.Admin;
using MotionRobotics.LMS.API.Services;
using MotionRobotics.LMS.API.Services.Admin;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ─── 1. Read config ──────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ─── MediatR (CQRS) ──────────────────────────────────────────
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

// ─── 2. Database connection ──────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ─── 3. ASP.NET Identity setup ───────────────────────────────
// This uses your existing AspNetUsers table
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ─── 4. CORS — allows Next.js to call this API ──────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJs", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000", "http://localhost:3001")   // your Next.js local dev URL
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ─── 5. JWT setup ────────────────────────────────────────────
builder.Services.AddAuthentication(options =>
{
    // Override Identity's default cookie auth → use JWT instead
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? "")
        )
    };
});

builder.Services.AddAuthorization();

// ─── 6. Session Service ──────────────────────────────────────
builder.Services.AddScoped<ISessionService, SessionService>();

// ─── 7. Admin Module Services ────────────────────────────────
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();
builder.Services.AddScoped<ISchoolService, SchoolService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IAdminStudentService, AdminStudentService>();
builder.Services.AddScoped<IAdminTeacherService, AdminTeacherService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<TeacherAuthService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<IReportService, ReportService>();

// ─── 7. Level Mapping Services ───────────────────────────────
builder.Services.AddScoped<IRoboticsLevelService, RoboticsLevelService>();
builder.Services.AddScoped<IAcademicYearService, AcademicYearService>();
builder.Services.AddScoped<ILevelMappingService, LevelMappingService>();

// ─── 8. Admin Module Repositories ────────────────────────────
builder.Services.AddScoped<ISchoolRepository, SchoolRepository>();
builder.Services.AddScoped<IClassRepository, ClassRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();

// ─── Build ───────────────────────────────────────────────────
var app = builder.Build();

// ─── Global Exception Handler (must be first) ────────────────
app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowNextJs");

// Configure static file serving with proper MIME types for PDFs and videos
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Set proper headers for PDF files to enable viewing in browser
        if (ctx.File.Name.EndsWith(".pdf"))
        {
            ctx.Context.Response.Headers.Append("Content-Type", "application/pdf");
            ctx.Context.Response.Headers.Append("Content-Disposition", "inline");
        }
        // Set proper headers for video files
        else if (ctx.File.Name.EndsWith(".mp4"))
        {
            ctx.Context.Response.Headers.Append("Content-Type", "video/mp4");
            ctx.Context.Response.Headers.Append("Accept-Ranges", "bytes");
        }
        else if (ctx.File.Name.EndsWith(".webm"))
        {
            ctx.Context.Response.Headers.Append("Content-Type", "video/webm");
            ctx.Context.Response.Headers.Append("Accept-Ranges", "bytes");
        }
    }
});

app.UseAuthentication();
app.UseAuthorization();

// ─── Session Validation (after auth, before controllers) ─────
app.UseSessionValidation();

app.MapControllers();

// ─── Comprehensive Data Seeding on startup ─────────────────
using (var scope = app.Services.CreateScope())
{
    try
    {
        var services = scope.ServiceProvider;

        // Apply any pending migrations
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        // ✅ PROFESSIONAL: Seed Admin at startup (ONE-TIME ONLY)
        await AdminSeeder.SeedAdminAsync(services);

        // Seed other data (books, experiments, exams, etc.)
        await DataSeeder.SeedDatabaseAsync(services);

        Console.WriteLine("✅ Database seeding completed!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ An error occurred while seeding the database: {ex.Message}");
    }
}

app.Run();