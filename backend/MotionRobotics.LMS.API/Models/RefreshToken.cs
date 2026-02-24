using Microsoft.AspNetCore.Identity;

namespace MotionRobotics.LMS.API.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        /// <summary>
        /// The actual refresh token string (hashed for security)
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// JWT Id (jti) - links to the access token
        /// </summary>
        public string JwtId { get; set; } = string.Empty;

        /// <summary>
        /// User this token belongs to
        /// </summary>
        public string UserId { get; set; } = string.Empty;
        public virtual IdentityUser? User { get; set; }

        /// <summary>
        /// When this refresh token was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When this refresh token expires
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// IP address of the client that created this token
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent/device info
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Whether this token has been used (for rotation)
        /// </summary>
        public bool IsUsed { get; set; } = false;

        /// <summary>
        /// Whether this token has been revoked (logout, security breach)
        /// </summary>
        public bool IsRevoked { get; set; } = false;

        /// <summary>
        /// If revoked, when it was revoked
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// Reason for revocation
        /// </summary>
        public string? RevokedReason { get; set; }

        /// <summary>
        /// Token that replaced this one (for token rotation tracking)
        /// </summary>
        public string? ReplacedByToken { get; set; }

        /// <summary>
        /// Check if token is active (not expired, not used, not revoked)
        /// </summary>
        public bool IsActive => !IsRevoked && !IsUsed && ExpiresAt > DateTime.UtcNow;
    }
}
