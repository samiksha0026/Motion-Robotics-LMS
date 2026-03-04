// ============================================================
// Program.cs
// Sets up: ASP.NET Identity + JWT Auth + CORS for Next.js
// Replace your existing Program.cs with this
// ============================================================
// NuGet packages needed (install all of these):
//   Microsoft.AspNetCore.Identity.EntityFrameworkCore
//   Microsoft.AspNetCore.Authentication.JwtBearer
//   Microsoft.IdentityModel.Tokens
//   Npgsql.EntityFrameworkCore.PostgreSQL
//   BCrypt.Net-Next
// ============================================================

using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.Middleware;
using MotionRobotics.LMS.API.Repositories.Admin;
using MotionRobotics.LMS.API.Seed;
using MotionRobotics.LMS.API.Services;
using MotionRobotics.LMS.API.Services.Admin;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ─── Render PORT binding (Render injects PORT env var) ────────
// Falls back to 8080 for local Docker or other platforms
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ─── 0. Configuration Validation ─────────────────────────────
ValidateConfiguration(builder.Configuration, builder.Environment);

// ─── 1. Read config ──────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ─── MediatR (CQRS) ──────────────────────────────────────────
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

// ─── 2. Database connection ──────────────────────────────────
// Supports both Npgsql key-value format AND postgresql:// URI format (Render default)
var rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
var connectionString = ConvertPostgresUriToNpgsql(rawConnectionString);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// ─── 3. ASP.NET Identity setup ───────────────────────────────
// This uses your existing AspNetUsers table
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ─── 4. CORS — allows Next.js to call this API ──────────────
// Development : always http://localhost:3000 (hardcoded, never AllowAnyOrigin)
// Production  : read Cors:AllowedOrigins from config/env vars — fail fast if empty
//               Set on Render: Cors__AllowedOrigins__0 = https://your-app.vercel.app
string[] allowedOrigins;

if (builder.Environment.IsDevelopment())
{
    allowedOrigins = ["http://localhost:3000"];
}
else
{
    allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? [];

    if (allowedOrigins.Length == 0)
    {
        throw new InvalidOperationException(
            "Cors:AllowedOrigins must contain at least one origin in Production. " +
            "Set via Cors__AllowedOrigins__0 environment variable on Render.");
    }
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJs", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ─── 4.5. Rate Limiting ──────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Global rate limit: 100 requests per minute per IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Strict rate limit for auth endpoints: 10 requests per minute
    options.AddFixedWindowLimiter("AuthEndpoints", opts =>
    {
        opts.PermitLimit = 10;
        opts.Window = TimeSpan.FromMinutes(1);
        opts.AutoReplenishment = true;
    });
});

// ─── 5. JWT setup ────────────────────────────────────────────
// IConfiguration automatically maps Jwt__SecretKey env var → Jwt:SecretKey
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];

if (string.IsNullOrEmpty(jwtSecretKey))
{
    if (builder.Environment.IsDevelopment())
    {
        jwtSecretKey = "DevOnlyKey-ChangeInProduction-Min32Chars!!";
    }
    else
    {
        throw new InvalidOperationException("Jwt:SecretKey is required in Production. Set via Jwt__SecretKey environment variable.");
    }
}

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
            Encoding.UTF8.GetBytes(jwtSecretKey)
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

// ─── 9. Forwarded Headers (reverse proxy support) ────────────
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// ─── Build ───────────────────────────────────────────────────
var app = builder.Build();

// ─── Global Exception Handler (must be first) ────────────────
app.UseGlobalExceptionHandler();

// ─── Forwarded Headers (must be early for correct scheme/IP) ─
app.UseForwardedHeaders();

// ─── CORS (must be before HTTPS redirect so OPTIONS preflights aren't redirected) ──
app.UseCors("AllowNextJs");

// ─── HSTS & HTTPS Redirect (production only) ────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ─── Security Headers ────────────────────────────────────────
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

    if (!context.Request.Path.StartsWithSegments("/swagger"))
    {
        context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline';");
    }

    await next();
});

// ─── Rate Limiting ───────────────────────────────────────────
app.UseRateLimiter();

// Configure static file serving with proper MIME types for PDFs and videos
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Set proper headers for PDF files to enable viewing in browser
        if (ctx.File.Name.EndsWith(".pdf"))
        {
            ctx.Context.Response.Headers["Content-Type"] = "application/pdf";
            ctx.Context.Response.Headers["Content-Disposition"] = "inline";
            // Remove X-Frame-Options so the PDF can be embedded cross-origin in the viewer
            ctx.Context.Response.Headers.Remove("X-Frame-Options");
            ctx.Context.Response.Headers.Remove("Content-Security-Policy");
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

        // Apply pending migrations on every startup (safe — EF migrations are idempotent)
        // In Development: migrates local/Neon DB
        // In Production (Render): migrates Neon DB on first deploy and after schema changes
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        // ✅ PROFESSIONAL: Seed Admin at startup (ONE-TIME ONLY)
        await AdminSeeder.SeedAdminAsync(services);

        // Seed other data (books, experiments, exams, etc.)
        await DataSeeder.SeedDatabaseAsync(services);

        // Seed demo data for client presentation
        await DemoDataSeeder.SeedAsync(services);

        Console.WriteLine("✅ Database seeding completed!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ An error occurred while seeding the database: {ex.Message}");
    }
}

app.Run();

// ─── Configuration Validation ────────────────────────────────
static void ValidateConfiguration(IConfiguration config, IWebHostEnvironment env)
{
    var errors = new List<string>();

    // Check required configuration keys
    // IConfiguration automatically maps Jwt__SecretKey env var → Jwt:SecretKey
    var jwtKey = config["Jwt:SecretKey"];
    if (string.IsNullOrEmpty(jwtKey))
    {
        if (!env.IsDevelopment())
        {
            errors.Add("JWT:SecretKey is required. Set via JWT__SecretKey environment variable.");
        }
        else
        {
            // Use a development-only key (not for production!)
            Console.WriteLine("⚠️  WARNING: Using default JWT key for development. Set JWT__SecretKey in production!");
        }
    }
    else if (jwtKey.Length < 32)
    {
        errors.Add("JWT:SecretKey must be at least 32 characters long.");
    }

    // Validate connection string
    var connectionString = config.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        errors.Add("ConnectionStrings:DefaultConnection is required.");
    }

    // Validate JWT issuer and audience
    if (string.IsNullOrEmpty(config["Jwt:Issuer"]))
        errors.Add("Jwt:Issuer is required.");
    if (string.IsNullOrEmpty(config["Jwt:Audience"]))
        errors.Add("Jwt:Audience is required.");

    // In production, require SuperAdmin credentials
    // IConfiguration maps SuperAdmin__Email → SuperAdmin:Email, SuperAdmin__Password → SuperAdmin:Password
    if (!env.IsDevelopment())
    {
        var superAdminEmail = config["SuperAdmin:Email"];
        var superAdminPassword = config["SuperAdmin:Password"];

        if (string.IsNullOrEmpty(superAdminEmail) || string.IsNullOrEmpty(superAdminPassword))
        {
            throw new InvalidOperationException("SuperAdmin credentials missing in Production environment.");
        }
    }

    if (errors.Any())
    {
        throw new InvalidOperationException(
            $"Configuration validation failed:\\n{string.Join("\\n", errors.Select(e => $"  - {e}"))}");
    }
}

// Converts postgresql://user:pass@host/db?sslmode=require  →  Npgsql key-value format
// Returns the input unchanged if it is already in key-value format
static string ConvertPostgresUriToNpgsql(string connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
        return connectionString;

    if (!connectionString.StartsWith("postgresql://") && !connectionString.StartsWith("postgres://"))
        return connectionString; // already key-value format

    var uri = new Uri(connectionString);
    var userInfo = uri.UserInfo.Split(':', 2);
    var username = Uri.UnescapeDataString(userInfo[0]);
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
    var host = uri.Host;
    var port = uri.IsDefaultPort ? 5432 : uri.Port;
    var database = uri.AbsolutePath.TrimStart('/');

    // Parse query string for sslmode
    var query = uri.Query.TrimStart('?');
    var sslMode = "Require";
    foreach (var param in query.Split('&'))
    {
        var kv = param.Split('=', 2);
        if (kv[0].Equals("sslmode", StringComparison.OrdinalIgnoreCase) && kv.Length > 1)
            sslMode = kv[1] switch { "require" => "Require", "disable" => "Disable", _ => "Require" };
    }

    return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode={sslMode};Trust Server Certificate=true";
}