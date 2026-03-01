using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.Models;
using System.Security.Cryptography;
using System.Text;

namespace MotionRobotics.LMS.API.Services
{
    /// <summary>
    /// Manages server-side user sessions backed by the UserSessions database table.
    /// </summary>
    public class SessionService : ISessionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SessionService> _logger;

        public SessionService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<SessionService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<UserSession> CreateSessionAsync(
            string userId,
            string role,
            string accessToken,
            string refreshToken,
            DateTime refreshTokenExpiresAt,
            int? schoolId,
            string? ipAddress,
            string? userAgent)
        {
            var sessionExpiryHours = GetSessionExpiryHours(role);

            var session = new UserSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Role = role,
                TokenHash = HashToken(accessToken),
                RefreshTokenHash = HashToken(refreshToken),
                RefreshTokenExpiresAt = refreshTokenExpiresAt,
                IpAddress = ipAddress,
                UserAgent = TruncateUserAgent(userAgent),
                SchoolId = schoolId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(sessionExpiryHours),
                LastActivityAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.UserSessions.Add(session);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Session created for user {UserId} with role {Role}, session {SessionId}, expires {ExpiresAt}",
                userId, role, session.Id, session.ExpiresAt);

            return session;
        }

        public async Task<UserSession?> ValidateSessionAsync(Guid sessionId, string accessToken)
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
            {
                _logger.LogWarning("Session {SessionId} not found", sessionId);
                return null;
            }

            if (!session.IsActive)
            {
                _logger.LogWarning("Session {SessionId} is not active (revoked/logged out)", sessionId);
                return null;
            }

            if (session.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Session {SessionId} has expired", sessionId);
                session.IsActive = false;
                session.LoggedOutAt = DateTime.UtcNow;
                session.LogoutReason = "Session expired";
                await _context.SaveChangesAsync();
                return null;
            }

            // Validate token hash matches
            var tokenHash = HashToken(accessToken);
            if (session.TokenHash != tokenHash)
            {
                _logger.LogWarning("Session {SessionId} token hash mismatch", sessionId);
                return null;
            }

            // Update last activity
            session.LastActivityAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return session;
        }

        public async Task<bool> RefreshSessionAsync(
            Guid sessionId,
            string newAccessToken,
            string newRefreshToken,
            DateTime newRefreshTokenExpiresAt)
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.IsActive);

            if (session == null)
                return false;

            session.TokenHash = HashToken(newAccessToken);
            session.RefreshTokenHash = HashToken(newRefreshToken);
            session.RefreshTokenExpiresAt = newRefreshTokenExpiresAt;
            session.LastActivityAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Session {SessionId} tokens refreshed", sessionId);
            return true;
        }

        public async Task<bool> EndSessionAsync(Guid sessionId, string reason = "User logout")
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
                return false;

            session.IsActive = false;
            session.LoggedOutAt = DateTime.UtcNow;
            session.LogoutReason = reason;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Session {SessionId} ended: {Reason}", sessionId, reason);
            return true;
        }

        public async Task<int> EndAllSessionsAsync(string userId, string reason = "All sessions revoked")
        {
            var activeSessions = await _context.UserSessions
                .Where(s => s.UserId == userId && s.IsActive)
                .ToListAsync();

            foreach (var session in activeSessions)
            {
                session.IsActive = false;
                session.LoggedOutAt = DateTime.UtcNow;
                session.LogoutReason = reason;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "All sessions ({Count}) ended for user {UserId}: {Reason}",
                activeSessions.Count, userId, reason);

            return activeSessions.Count;
        }

        public async Task<List<UserSession>> GetActiveSessionsAsync(string userId)
        {
            return await _context.UserSessions
                .Where(s => s.UserId == userId && s.IsActive && s.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(s => s.LastActivityAt)
                .ToListAsync();
        }

        public async Task<UserSession?> GetSessionAsync(Guid sessionId)
        {
            return await _context.UserSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId);
        }

        public async Task<int> CleanupExpiredSessionsAsync(int olderThanDays = 30)
        {
            var cutoff = DateTime.UtcNow.AddDays(-olderThanDays);

            var expiredSessions = await _context.UserSessions
                .Where(s => !s.IsActive && s.LoggedOutAt < cutoff
                         || s.ExpiresAt < cutoff)
                .ToListAsync();

            _context.UserSessions.RemoveRange(expiredSessions);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cleaned up {Count} expired sessions", expiredSessions.Count);
            return expiredSessions.Count;
        }

        // ── Helpers ──────────────────────────────────────────────────────

        /// <summary>
        /// SHA256 hash of a token string. We never store raw tokens.
        /// </summary>
        private static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Get session expiry hours based on role.
        /// Admins get shorter sessions, students get longer.
        /// </summary>
        private int GetSessionExpiryHours(string role)
        {
            return role switch
            {
                "SuperAdmin" => int.TryParse(_configuration["Session:SuperAdminExpiryHours"], out var sa) ? sa : 8,
                "SchoolAdmin" => int.TryParse(_configuration["Session:SchoolAdminExpiryHours"], out var sca) ? sca : 10,
                "Admin" => int.TryParse(_configuration["Session:AdminExpiryHours"], out var a) ? a : 8,
                "Teacher" => int.TryParse(_configuration["Session:TeacherExpiryHours"], out var t) ? t : 12,
                "Student" => int.TryParse(_configuration["Session:StudentExpiryHours"], out var s) ? s : 24,
                _ => 8
            };
        }

        /// <summary>
        /// Truncate user agent to prevent storing excessively long strings.
        /// </summary>
        private static string? TruncateUserAgent(string? userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return null;
            return userAgent.Length > 500 ? userAgent[..500] : userAgent;
        }
    }
}
