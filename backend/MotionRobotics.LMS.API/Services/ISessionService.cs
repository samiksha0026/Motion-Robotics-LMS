using MotionRobotics.LMS.API.Models;

namespace MotionRobotics.LMS.API.Services
{
    /// <summary>
    /// Manages server-side user sessions: create on login, validate on every request,
    /// revoke on logout, and cleanup expired sessions.
    /// </summary>
    public interface ISessionService
    {
        /// <summary>
        /// Create a new session when a user logs in.
        /// Returns the session ID (stored in cookie on the client).
        /// </summary>
        Task<UserSession> CreateSessionAsync(
            string userId,
            string role,
            string accessToken,
            string refreshToken,
            DateTime refreshTokenExpiresAt,
            int? schoolId,
            string? ipAddress,
            string? userAgent);

        /// <summary>
        /// Validate that a session exists, is active, and has a matching token hash.
        /// Also updates LastActivityAt.
        /// </summary>
        Task<UserSession?> ValidateSessionAsync(Guid sessionId, string accessToken);

        /// <summary>
        /// Update the session when tokens are refreshed (new token hash + refresh token hash).
        /// </summary>
        Task<bool> RefreshSessionAsync(
            Guid sessionId,
            string newAccessToken,
            string newRefreshToken,
            DateTime newRefreshTokenExpiresAt);

        /// <summary>
        /// Deactivate a single session (user logout).
        /// </summary>
        Task<bool> EndSessionAsync(Guid sessionId, string reason = "User logout");

        /// <summary>
        /// Deactivate all sessions for a user (logout from all devices / security revoke).
        /// </summary>
        Task<int> EndAllSessionsAsync(string userId, string reason = "All sessions revoked");

        /// <summary>
        /// Get all active sessions for a user (for "active sessions" UI).
        /// </summary>
        Task<List<UserSession>> GetActiveSessionsAsync(string userId);

        /// <summary>
        /// Retrieve a session by its ID.
        /// </summary>
        Task<UserSession?> GetSessionAsync(Guid sessionId);

        /// <summary>
        /// Cleanup expired or inactive sessions older than the specified days.
        /// Called periodically (e.g. background service or on startup).
        /// </summary>
        Task<int> CleanupExpiredSessionsAsync(int olderThanDays = 30);
    }
}
