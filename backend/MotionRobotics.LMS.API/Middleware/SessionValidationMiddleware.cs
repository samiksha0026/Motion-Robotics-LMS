using MotionRobotics.LMS.API.Services;

namespace MotionRobotics.LMS.API.Middleware
{
    /// <summary>
    /// Middleware that validates every authenticated request against the UserSessions table.
    /// If the session is inactive, expired, or the token doesn't match, the request is rejected with 401.
    /// This runs AFTER the built-in JWT authentication middleware.
    /// </summary>
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SessionValidationMiddleware> _logger;

        // Paths that don't require session validation (login, refresh, public endpoints)
        private static readonly string[] SkipPaths = new[]
        {
            "/api/auth/login",
            "/api/auth/refresh",
            "/api/auth/register",
            "/swagger",
            "/favicon.ico"
        };

        public SessionValidationMiddleware(RequestDelegate next, ILogger<SessionValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // Skip session validation for public/auth endpoints and static files
            if (ShouldSkip(path))
            {
                await _next(context);
                return;
            }

            // Only validate if the user is authenticated (has a valid JWT)
            if (context.User.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            // Extract session ID from cookie
            var sessionIdStr = context.Request.Cookies["sessionId"];
            if (string.IsNullOrEmpty(sessionIdStr) || !Guid.TryParse(sessionIdStr, out var sessionId))
            {
                _logger.LogWarning("Authenticated request without sessionId cookie from {Path}", path);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Session not found. Please login again." });
                return;
            }

            // Extract the raw Bearer token from the Authorization header
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            var accessToken = authHeader["Bearer ".Length..].Trim();

            // Validate the session against the database
            using var scope = context.RequestServices.CreateScope();
            var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
            var session = await sessionService.ValidateSessionAsync(sessionId, accessToken);

            if (session == null)
            {
                _logger.LogWarning("Session validation failed for session {SessionId} on {Path}", sessionId, path);

                // Clear the session cookie
                context.Response.Cookies.Delete("sessionId", new CookieOptions
                {
                    Path = "/",
                    Secure = true,
                    SameSite = SameSiteMode.None
                });

                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Session expired or revoked. Please login again." });
                return;
            }

            // Store session info in HttpContext.Items for downstream use (controllers, etc.)
            context.Items["SessionId"] = session.Id;
            context.Items["SessionRole"] = session.Role;
            context.Items["SessionSchoolId"] = session.SchoolId;

            await _next(context);
        }

        private static bool ShouldSkip(string path)
        {
            // Skip static files
            if (path.Contains('.') && !path.StartsWith("/api"))
                return true;

            foreach (var skip in SkipPaths)
            {
                if (path.StartsWith(skip, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Extension method to register the session validation middleware.
    /// </summary>
    public static class SessionValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseSessionValidation(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SessionValidationMiddleware>();
        }
    }
}
