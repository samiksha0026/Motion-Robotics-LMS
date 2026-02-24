using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace MotionRobotics.LMS.API.Middleware
{
    /// <summary>
    /// Global exception handling middleware.
    /// Catches all unhandled exceptions and returns a consistent error response.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Log the exception
            _logger.LogError(exception,
                "Unhandled exception occurred. Path: {Path}, Method: {Method}, TraceId: {TraceId}",
                context.Request.Path,
                context.Request.Method,
                context.TraceIdentifier);

            // Determine status code and error details based on exception type
            var (statusCode, errorCode, message) = MapExceptionToResponse(exception);

            var response = new ApiErrorResponse
            {
                StatusCode = statusCode,
                ErrorCode = errorCode,
                Message = message,
                Path = context.Request.Path,
                Timestamp = DateTime.UtcNow
            };

            // Include stack trace only in Development
            if (_env.IsDevelopment())
            {
                response.StackTrace = exception.ToString();
            }

            // Set response properties
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }

        private static (int StatusCode, string ErrorCode, string Message) MapExceptionToResponse(Exception exception)
        {
            return exception switch
            {
                // Not Found
                KeyNotFoundException ex => (
                    (int)HttpStatusCode.NotFound,
                    ErrorCodes.NotFound,
                    ex.Message),

                // Validation / Business Rule
                InvalidOperationException ex => (
                    (int)HttpStatusCode.BadRequest,
                    ErrorCodes.BusinessRuleViolation,
                    ex.Message),

                // Argument errors
                ArgumentNullException ex => (
                    (int)HttpStatusCode.BadRequest,
                    ErrorCodes.InvalidInput,
                    $"Required parameter missing: {ex.ParamName}"),

                ArgumentException ex => (
                    (int)HttpStatusCode.BadRequest,
                    ErrorCodes.InvalidInput,
                    ex.Message),

                // Unauthorized
                UnauthorizedAccessException ex => (
                    (int)HttpStatusCode.Unauthorized,
                    ErrorCodes.Unauthorized,
                    ex.Message.Length > 0 ? ex.Message : "You are not authorized to perform this action"),

                // Database errors
                DbUpdateException ex => (
                    (int)HttpStatusCode.Conflict,
                    ErrorCodes.DatabaseError,
                    GetDbErrorMessage(ex)),

                // Timeout
                TimeoutException => (
                    (int)HttpStatusCode.GatewayTimeout,
                    ErrorCodes.ServiceUnavailable,
                    "The request timed out. Please try again."),

                // Operation cancelled
                OperationCanceledException => (
                    (int)HttpStatusCode.BadRequest,
                    ErrorCodes.OperationNotAllowed,
                    "The operation was cancelled."),

                // Default - Internal Server Error
                _ => (
                    (int)HttpStatusCode.InternalServerError,
                    ErrorCodes.InternalError,
                    "An unexpected error occurred. Please try again later.")
            };
        }

        private static string GetDbErrorMessage(DbUpdateException ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;

            // Check for common database constraint violations
            if (innerMessage.Contains("UNIQUE constraint", StringComparison.OrdinalIgnoreCase) ||
                innerMessage.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
            {
                return "A record with this information already exists.";
            }

            if (innerMessage.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase))
            {
                return "This operation references a record that doesn't exist.";
            }

            if (innerMessage.Contains("DELETE", StringComparison.OrdinalIgnoreCase) &&
                innerMessage.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase))
            {
                return "Cannot delete this record because it is referenced by other records.";
            }

            return "A database error occurred. Please try again.";
        }
    }

    /// <summary>
    /// Extension method to easily add the middleware
    /// </summary>
    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}
