using Microsoft.AspNetCore.Identity;

namespace MotionRobotics.LMS.API.Models
{
    /// <summary>
    /// Server-side session tracking for authenticated users.
    /// Each login creates a new session record. Sessions can be individually
    /// revoked (force-logout) or expired based on time.
    /// </summary>
    public class UserSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The user this session belongs to (FK → AspNetUsers)
        /// </summary>
        public string UserId { get; set; } = string.Empty;
        public virtual IdentityUser? User { get; set; }

        /// <summary>
        /// Role at the time of login (SuperAdmin, SchoolAdmin, Teacher, Student, etc.)
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// SHA256 hash of the JWT access token — used to validate the session is still active.
        /// We never store the raw token.
        /// </summary>
        public string TokenHash { get; set; } = string.Empty;

        /// <summary>
        /// The refresh token associated with this session (hashed)
        /// </summary>
        public string RefreshTokenHash { get; set; } = string.Empty;

        /// <summary>
        /// When the refresh token expires
        /// </summary>
        public DateTime RefreshTokenExpiresAt { get; set; }

        /// <summary>
        /// Client IP address at login
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Browser/device user-agent string
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// School ID for school-scoped sessions (SchoolAdmin, Teacher, Student)
        /// </summary>
        public int? SchoolId { get; set; }

        /// <summary>
        /// When the session was created (login time)
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Absolute session expiry — after this time the session is dead regardless of activity
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Last time an API call was made using this session (sliding window tracking)
        /// </summary>
        public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When set to false, the session is force-killed (admin logout, security revoke, etc.)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// When the user logged out or the session was revoked
        /// </summary>
        public DateTime? LoggedOutAt { get; set; }

        /// <summary>
        /// Reason the session was ended (e.g. "User logout", "Admin force logout", "Token reuse detected")
        /// </summary>
        public string? LogoutReason { get; set; }
    }
}
