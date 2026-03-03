using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.DTOs;
using MotionRobotics.LMS.API.Models;
using MotionRobotics.LMS.API.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MotionRobotics.LMS.API.Features.Auth;

[ApiController]
[Route("api/auth")]
[Tags("Auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration configuration,
        ApplicationDbContext context,
        ISessionService sessionService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
        _sessionService = sessionService;
    }

    [HttpPost("login")]
    [EnableRateLimiting("AuthEndpoints")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
                ?? await _userManager.FindByNameAsync(request.Email);

        if (user == null)
            return Unauthorized(new { message = "Invalid credentials" });

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
            return Unauthorized(new { message = "Invalid credentials" });

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? "Unknown";

        object? roleData = null;

        if (roles.Contains("SchoolAdmin"))
        {
            var school = await _context.Schools
                .Where(s => s.SchoolAdminUserId == user.Id)
                .Select(s => new { s.Id, s.SchoolName, s.SchoolCode, s.LogoUrl, s.IsActive })
                .FirstOrDefaultAsync();

            if (school != null && !school.IsActive)
                return Unauthorized(new { message = "School account is deactivated. Contact administrator." });

            roleData = new { school };
        }
        else if (roles.Contains("Teacher"))
        {
            var teacher = await _context.Teachers
                .Include(t => t.School)
                .Where(t => t.UserId == user.Id)
                .Select(t => new { t.Id, t.FullName, SchoolId = t.SchoolId, SchoolName = t.School!.SchoolName })
                .FirstOrDefaultAsync();
            roleData = new { teacher };
        }
        else if (roles.Contains("Student"))
        {
            var student = await _context.Students
                .Include(s => s.School)
                .Include(s => s.Class)
                .Where(s => s.UserId == user.Id)
                .Select(s => new
                {
                    s.Id,
                    s.FullName,
                    SchoolId = s.SchoolId,
                    SchoolName = s.School!.SchoolName,
                    ClassId = s.ClassId,
                    ClassName = s.Class!.ClassName
                })
                .FirstOrDefaultAsync();
            roleData = new { student };
        }

        var (accessToken, jwtId) = GenerateAccessToken(user, roles);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id, jwtId);

        // Determine school ID for session scoping
        int? schoolId = null;
        if (roles.Contains("SchoolAdmin"))
        {
            schoolId = await _context.Schools
                .Where(s => s.SchoolAdminUserId == user.Id)
                .Select(s => (int?)s.Id)
                .FirstOrDefaultAsync();
        }
        else if (roles.Contains("Teacher"))
        {
            schoolId = await _context.Teachers
                .Where(t => t.UserId == user.Id)
                .Select(t => (int?)t.SchoolId)
                .FirstOrDefaultAsync();
        }
        else if (roles.Contains("Student"))
        {
            schoolId = await _context.Students
                .Where(s => s.UserId == user.Id)
                .Select(s => (int?)s.SchoolId)
                .FirstOrDefaultAsync();
        }

        // Create server-side session
        var session = await _sessionService.CreateSessionAsync(
            userId: user.Id,
            role: primaryRole,
            accessToken: accessToken,
            refreshToken: refreshToken.Token,
            refreshTokenExpiresAt: refreshToken.ExpiresAt,
            schoolId: schoolId,
            ipAddress: GetClientIpAddress(),
            userAgent: Request.Headers["User-Agent"].ToString());

        // Set httpOnly session cookie
        SetSessionCookie(session.Id);

        return Ok(new
        {
            token = accessToken,
            refreshToken = refreshToken.Token,
            sessionId = session.Id,
            role = primaryRole,
            roles,
            user = new { user.Email, user.UserName },
            data = roleData,
            expires = DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes()),
            refreshTokenExpires = refreshToken.ExpiresAt
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
            return BadRequest(new { message = "Refresh token is required" });

        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (storedToken == null)
            return Unauthorized(new { message = "Invalid refresh token" });

        if (!storedToken.IsActive)
        {
            if (storedToken.IsUsed)
            {
                await RevokeAllUserTokensAsync(storedToken.UserId, "Token reuse detected - possible theft");
                return Unauthorized(new { message = "Token reuse detected. All sessions revoked for security." });
            }
            return Unauthorized(new { message = "Refresh token expired or revoked" });
        }

        var user = storedToken.User;
        if (user == null)
            return Unauthorized(new { message = "User not found" });

        var roles = await _userManager.GetRolesAsync(user);
        storedToken.IsUsed = true;

        var (accessToken, jwtId) = GenerateAccessToken(user, roles);
        var newRefreshToken = await GenerateRefreshTokenAsync(user.Id, jwtId);

        storedToken.ReplacedByToken = newRefreshToken.Token;
        await _context.SaveChangesAsync();

        // Update the server-side session with new token hashes
        var sessionIdStr = Request.Cookies["sessionId"];
        if (!string.IsNullOrEmpty(sessionIdStr) && Guid.TryParse(sessionIdStr, out var sessionId))
        {
            await _sessionService.RefreshSessionAsync(
                sessionId, accessToken, newRefreshToken.Token, newRefreshToken.ExpiresAt);
        }

        return Ok(new
        {
            token = accessToken,
            refreshToken = newRefreshToken.Token,
            role = roles,
            expires = DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes()),
            refreshTokenExpires = newRefreshToken.ExpiresAt
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest? request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(email))
            {
                var u = await _userManager.FindByEmailAsync(email);
                userId = u?.Id;
            }
        }

        if (string.IsNullOrEmpty(userId))
            return BadRequest(new { message = "User not found" });

        // Revoke refresh token in DB
        if (!string.IsNullOrEmpty(request?.RefreshToken))
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.UserId == userId);

            if (token != null)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedReason = "User logout";
                await _context.SaveChangesAsync();
            }
        }

        // End server-side session
        var sessionIdStr = Request.Cookies["sessionId"];
        if (!string.IsNullOrEmpty(sessionIdStr) && Guid.TryParse(sessionIdStr, out var sessionId))
        {
            await _sessionService.EndSessionAsync(sessionId, "User logout");
        }

        // Clear session cookie
        ClearSessionCookie();

        return Ok(new { message = "Logged out successfully" });
    }

    [HttpPost("logout-all")]
    [Authorize]
    public async Task<IActionResult> LogoutAll()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return BadRequest(new { message = "User not found" });

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return BadRequest(new { message = "User not found" });

        // Revoke all refresh tokens
        await RevokeAllUserTokensAsync(user.Id, "User logged out from all devices");

        // End all server-side sessions
        var revokedCount = await _sessionService.EndAllSessionsAsync(user.Id, "User logged out from all devices");

        // Clear current session cookie
        ClearSessionCookie();

        return Ok(new { message = $"All sessions revoked successfully ({revokedCount} sessions ended)" });
    }

    [HttpGet("sessions")]
    [Authorize]
    public async Task<IActionResult> GetActiveSessions()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return BadRequest(new { message = "User not found" });

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return BadRequest(new { message = "User not found" });

        // Get current session ID from cookie
        var currentSessionIdStr = Request.Cookies["sessionId"];
        Guid.TryParse(currentSessionIdStr, out var currentSessionId);

        // Return sessions from the UserSessions table
        var sessions = await _sessionService.GetActiveSessionsAsync(user.Id);
        var result = sessions.Select(s => new
        {
            s.Id,
            s.Role,
            s.CreatedAt,
            s.ExpiresAt,
            s.LastActivityAt,
            s.IpAddress,
            s.UserAgent,
            IsCurrent = s.Id == currentSessionId
        });

        return Ok(result);
    }

    [HttpPost("revoke-session/{sessionId:guid}")]
    [Authorize]
    public async Task<IActionResult> RevokeSession(Guid sessionId)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return BadRequest(new { message = "User not found" });

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return BadRequest(new { message = "User not found" });

        // Verify the session belongs to this user
        var session = await _sessionService.GetSessionAsync(sessionId);
        if (session == null || session.UserId != user.Id)
            return NotFound(new { message = "Session not found" });

        await _sessionService.EndSessionAsync(sessionId, "Revoked by user");

        return Ok(new { message = "Session revoked successfully" });
    }

    /// <summary>
    /// Returns the current user's session info (role, school, etc.) — used by the frontend
    /// to get session state without relying on localStorage.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentSession()
    {
        var sessionIdStr = Request.Cookies["sessionId"];
        if (string.IsNullOrEmpty(sessionIdStr) || !Guid.TryParse(sessionIdStr, out var sessionId))
            return Unauthorized(new { message = "No active session" });

        var session = await _sessionService.GetSessionAsync(sessionId);
        if (session == null || !session.IsActive)
            return Unauthorized(new { message = "Session expired" });

        var user = await _userManager.FindByIdAsync(session.UserId);
        if (user == null)
            return Unauthorized(new { message = "User not found" });

        var roles = await _userManager.GetRolesAsync(user);

        // Get role-specific data
        object? roleData = null;
        if (session.Role == "SchoolAdmin" && session.SchoolId.HasValue)
        {
            var school = await _context.Schools
                .Where(s => s.Id == session.SchoolId.Value)
                .Select(s => new { s.Id, s.SchoolName, s.SchoolCode, s.LogoUrl })
                .FirstOrDefaultAsync();
            roleData = new { school };
        }
        else if (session.Role == "Teacher")
        {
            var teacher = await _context.Teachers
                .Include(t => t.School)
                .Where(t => t.UserId == user.Id)
                .Select(t => new { t.Id, t.FullName, SchoolId = t.SchoolId, SchoolName = t.School!.SchoolName })
                .FirstOrDefaultAsync();
            roleData = new { teacher };
        }
        else if (session.Role == "Student")
        {
            var student = await _context.Students
                .Include(s => s.School)
                .Include(s => s.Class)
                .Where(s => s.UserId == user.Id)
                .Select(s => new { s.Id, s.FullName, SchoolId = s.SchoolId, SchoolName = s.School!.SchoolName, ClassId = s.ClassId, ClassName = s.Class!.ClassName })
                .FirstOrDefaultAsync();
            roleData = new { student };
        }

        return Ok(new
        {
            sessionId = session.Id,
            role = session.Role,
            roles,
            user = new { user.Email, user.UserName },
            data = roleData,
            schoolId = session.SchoolId,
            expiresAt = session.ExpiresAt
        });
    }

    // ── Private helpers ─────────────────────────────────────────────────────

    private (string token, string jwtId) GenerateAccessToken(IdentityUser user, IList<string> roles)
    {
        var jwtId = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, jwtId),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email!)
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes()),
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), jwtId);
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string jwtId)
    {
        var refreshToken = new RefreshToken
        {
            Token = GenerateSecureToken(),
            JwtId = jwtId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays()),
            IpAddress = GetClientIpAddress(),
            UserAgent = Request.Headers["User-Agent"].ToString()
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private async Task RevokeAllUserTokensAsync(string userId, string reason)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = reason;
        }

        await _context.SaveChangesAsync();
    }

    private string? GetClientIpAddress()
    {
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        return !string.IsNullOrEmpty(forwardedFor)
            ? forwardedFor.Split(',')[0].Trim()
            : HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    private int GetAccessTokenExpiryMinutes()
        => int.TryParse(_configuration["Jwt:AccessTokenExpiryMinutes"], out var m) ? m : 15;

    private int GetRefreshTokenExpiryDays()
        => int.TryParse(_configuration["Jwt:RefreshTokenExpiryDays"], out var d) ? d : 7;

    // ── Cookie helpers ──────────────────────────────────────────────────

    private void SetSessionCookie(Guid sessionId)
    {
        Response.Cookies.Append("sessionId", sessionId.ToString(), new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(GetRefreshTokenExpiryDays())
        });
    }

    private void ClearSessionCookie()
    {
        Response.Cookies.Delete("sessionId", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/"
        });
    }
}

// ── Inline request DTOs (auth-feature-specific) ─────────────────────────────
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class LogoutRequest
{
    public string? RefreshToken { get; set; }
}
