using System.Diagnostics;

namespace Prospera.API.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        var request = context.Request;
        _logger.LogInformation(
            "Incoming request: {Method} {Path} from {RemoteIP}",
            request.Method,
            request.Path,
            context.Connection.RemoteIpAddress
        );

        // Copy the original response stream to allow reading
        var originalBodyStream = context.Response.Body;
        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                stopwatch.Stop();

                _logger.LogInformation(
                    "Request completed: {Method} {Path} returned {StatusCode} in {ElapsedMilliseconds}ms",
                    request.Method,
                    request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds
                );

                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "Request failed: {Method} {Path} after {ElapsedMilliseconds}ms",
                    request.Method,
                    request.Path,
                    stopwatch.ElapsedMilliseconds
                );
                throw;
            }
        }
    }
}

/// <summary>
/// Extension methods for adding request logging middleware
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
