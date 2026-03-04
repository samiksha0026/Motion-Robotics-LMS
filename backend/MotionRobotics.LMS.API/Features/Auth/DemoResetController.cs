using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.Seed;

namespace MotionRobotics.LMS.API.Features.Auth;

/// <summary>
/// Temporary diagnostic endpoint used to verify and recreate demo accounts.
/// Protected by a shared secret. Remove after client presentation.
/// </summary>
[ApiController]
[Route("api/demo")]
public class DemoResetController : ControllerBase
{
    private const string Secret = "MOTIONROBOTICS_DEMO_2025";

    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public DemoResetController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    /// <summary>
    /// GET /api/demo/status?secret=MOTIONROBOTICS_DEMO_2025
    /// Returns whether each demo user exists with correct role in DB.
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> Status([FromQuery] string? secret)
    {
        if (secret != Secret)
            return Unauthorized(new { message = "Invalid secret" });

        var emails = new[] { "admin@demo.com", "teacher@demo.com", "student@demo.com", "student2@demo.com" };
        var results = new List<object>();

        foreach (var email in emails)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                results.Add(new { email, exists = false, roles = Array.Empty<string>(), passwordHash = "N/A" });
                continue;
            }
            var roles = await _userManager.GetRolesAsync(user);
            // Show first 20 chars of hash to identify algo (BCrypt starts with $2a$, PBKDF2 is much longer)
            var hashPrefix = user.PasswordHash?[..Math.Min(20, user.PasswordHash.Length)] ?? "NULL";
            results.Add(new { email, exists = true, roles, hashPrefix });
        }

        return Ok(new { timestamp = DateTime.UtcNow, users = results });
    }

    /// <summary>
    /// POST /api/demo/reset?secret=MOTIONROBOTICS_DEMO_2025
    /// Wipes all demo accounts and re-seeds them from scratch.
    /// Bypasses the seeder's early-exit guard by deleting users first.
    /// </summary>
    [HttpPost("reset")]
    public async Task<IActionResult> Reset([FromQuery] string? secret)
    {
        if (secret != Secret)
            return Unauthorized(new { message = "Invalid secret" });

        var before = await GetUserSummary();

        // Force-delete demo users so the seeder's early-exit guard doesn't skip
        var emailsToDelete = new[] { "admin@demo.com", "teacher@demo.com", "student@demo.com", "student2@demo.com" };
        foreach (var email in emailsToDelete)
        {
            var existing = await _userManager.FindByEmailAsync(email);
            if (existing != null)
                await _userManager.DeleteAsync(existing);
        }

        // Now run the seeder (no users exist, so it will recreate them)
        var scope = HttpContext.RequestServices;
        await DemoDataSeeder.SeedAsync(scope);

        var after = await GetUserSummary();
        return Ok(new
        {
            message = "Reset completed",
            before,
            after
        });
    }

    private async Task<object> GetUserSummary()
    {
        var emails = new[] { "admin@demo.com", "teacher@demo.com", "student@demo.com", "student2@demo.com" };
        var list = new List<object>();
        foreach (var email in emails)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                list.Add(new { email, exists = false });
                continue;
            }
            var roles = await _userManager.GetRolesAsync(user);
            var hashPrefix = user.PasswordHash?[..Math.Min(20, user.PasswordHash.Length)] ?? "NULL";
            list.Add(new { email, exists = true, roles, hashPrefix });
        }
        return list;
    }
}
