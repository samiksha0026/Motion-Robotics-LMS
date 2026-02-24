using System.Text.Json.Serialization;

namespace MotionRobotics.LMS.API.Middleware
{
    /// <summary>
    /// Standardized API error response format.
    /// All API errors return this consistent structure.
    /// </summary>
    public class ApiErrorResponse
    {
        /// <summary>
        /// Indicates the request was unsuccessful
        /// </summary>
        public bool Success { get; set; } = false;

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Human-readable error message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Machine-readable error code for client handling
        /// </summary>
        public string ErrorCode { get; set; } = string.Empty;

        /// <summary>
        /// Request path where the error occurred
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the error occurred
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Detailed errors (e.g., validation errors)
        /// Only included when there are multiple error details
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ErrorDetail>? Errors { get; set; }

        /// <summary>
        /// Stack trace - only included in Development environment
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? StackTrace { get; set; }
    }

    /// <summary>
    /// Individual error detail (for validation errors, etc.)
    /// </summary>
    public class ErrorDetail
    {
        public string Field { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Standard error codes for client handling
    /// </summary>
    public static class ErrorCodes
    {
        // Authentication & Authorization
        public const string Unauthorized = "AUTH_UNAUTHORIZED";
        public const string Forbidden = "AUTH_FORBIDDEN";
        public const string InvalidCredentials = "AUTH_INVALID_CREDENTIALS";
        public const string TokenExpired = "AUTH_TOKEN_EXPIRED";

        // Validation
        public const string ValidationFailed = "VALIDATION_FAILED";
        public const string InvalidInput = "INVALID_INPUT";

        // Resources
        public const string NotFound = "RESOURCE_NOT_FOUND";
        public const string AlreadyExists = "RESOURCE_ALREADY_EXISTS";
        public const string Conflict = "RESOURCE_CONFLICT";

        // Server
        public const string InternalError = "INTERNAL_SERVER_ERROR";
        public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
        public const string DatabaseError = "DATABASE_ERROR";

        // Business Logic
        public const string BusinessRuleViolation = "BUSINESS_RULE_VIOLATION";
        public const string OperationNotAllowed = "OPERATION_NOT_ALLOWED";
    }
}
