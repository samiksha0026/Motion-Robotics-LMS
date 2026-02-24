using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MotionRobotics.LMS.API.DTOs;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.Models;

namespace MotionRobotics.LMS.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AuthController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            // Try to find user by email first, then by username
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                user = await _userManager.FindByNameAsync(request.Email);

            if (user == null)
                return Unauthorized(new { message = "Invalid credentials" });

            var result = await _signInManager.CheckPasswordSignInAsync(
                user, request.Password, false);

            if (!result.Succeeded)
                return Unauthorized(new { message = "Invalid credentials" });

            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? "Unknown";

            // Get role-specific data
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
                    .Select(t => new
                    {
                        t.Id,
                        t.FullName,
                        SchoolId = t.SchoolId,
                        SchoolName = t.School!.SchoolName
                    })
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

            // Generate tokens
            var (accessToken, jwtId) = GenerateAccessToken(user, roles);
            var refreshToken = await GenerateRefreshTokenAsync(user.Id, jwtId);

            return Ok(new
            {
                token = accessToken,
                refreshToken = refreshToken.Token,
                role = primaryRole,
                roles = roles,
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

            // Find the refresh token
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (storedToken == null)
                return Unauthorized(new { message = "Invalid refresh token" });

            if (!storedToken.IsActive)
            {
                // Token reuse detected - revoke all tokens for this user
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

            // Mark old token as used
            storedToken.IsUsed = true;

            // Generate new tokens (token rotation)
            var (accessToken, jwtId) = GenerateAccessToken(user, roles);
            var newRefreshToken = await GenerateRefreshTokenAsync(user.Id, jwtId);

            // Link old token to new one
            storedToken.ReplacedByToken = newRefreshToken.Token;
            await _context.SaveChangesAsync();

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
                // Try to find user by email
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(email))
                {
                    var user = await _userManager.FindByEmailAsync(email);
                    userId = user?.Id;
                }
            }

            if (string.IsNullOrEmpty(userId))
                return BadRequest(new { message = "User not found" });

            if (!string.IsNullOrEmpty(request?.RefreshToken))
            {
                // Revoke specific refresh token
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

            await RevokeAllUserTokensAsync(user.Id, "User logged out from all devices");

            return Ok(new { message = "All sessions revoked successfully" });
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

            var sessions = await _context.RefreshTokens
                .Where(rt => rt.UserId == user.Id && !rt.IsRevoked && !rt.IsUsed && rt.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(rt => rt.CreatedAt)
                .Select(rt => new
                {
                    rt.Id,
                    rt.CreatedAt,
                    rt.ExpiresAt,
                    rt.IpAddress,
                    rt.UserAgent,
                    IsCurrent = rt.Token == Request.Headers["X-Refresh-Token"].ToString()
                })
                .ToListAsync();

            return Ok(sessions);
        }

        [HttpPost("revoke-session/{sessionId}")]
        [Authorize]
        public async Task<IActionResult> RevokeSession(int sessionId)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { message = "User not found" });

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return BadRequest(new { message = "User not found" });

            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Id == sessionId && rt.UserId == user.Id);

            if (token == null)
                return NotFound(new { message = "Session not found" });

            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = "Revoked by user";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Session revoked successfully" });
        }

        // ─── Private Helper Methods ─────────────────────────────────────

        private (string token, string jwtId) GenerateAccessToken(IdentityUser user, IList<string> roles)
        {
            var jwtId = Guid.NewGuid().ToString();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, jwtId),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes()),
                signingCredentials: creds
            );

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
            // Check for forwarded IP (behind proxy/load balancer)
            var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        private int GetAccessTokenExpiryMinutes()
        {
            return int.TryParse(_configuration["Jwt:AccessTokenExpiryMinutes"], out var minutes)
                ? minutes
                : 15; // Default 15 minutes
        }

        private int GetRefreshTokenExpiryDays()
        {
            return int.TryParse(_configuration["Jwt:RefreshTokenExpiryDays"], out var days)
                ? days
                : 7; // Default 7 days
        }
    }

    // ─── DTOs ─────────────────────────────────────────────────────────

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class LogoutRequest
    {
        public string? RefreshToken { get; set; }
    }
}
