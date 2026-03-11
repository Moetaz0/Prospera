using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FluentValidation;

namespace Prospera.API.Filters;

/// <summary>
/// Global exception filter for handling errors across the API
/// </summary>
public class ApiExceptionFilter : IAsyncExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnExceptionAsync(ExceptionContext context)
    {
        var response = new ProblemDetails();

        switch (context.Exception)
        {
            case ValidationException validationException:
                response.Status = StatusCodes.Status400BadRequest;
                response.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                response.Title = "Validation Error";
                response.Detail = "One or more validation errors occurred.";
                response.Extensions["errors"] = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                _logger.LogWarning("Validation error: {@ValidationErrors}", validationException.Errors);
                break;

            case InvalidOperationException invalidOpException:
                response.Status = StatusCodes.Status400BadRequest;
                response.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                response.Title = "Invalid Operation";
                response.Detail = invalidOpException.Message;
                _logger.LogWarning("Invalid operation: {Message}", invalidOpException.Message);
                break;

            case KeyNotFoundException notFoundException:
                response.Status = StatusCodes.Status404NotFound;
                response.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
                response.Title = "Resource Not Found";
                response.Detail = notFoundException.Message;
                _logger.LogWarning("Resource not found: {Message}", notFoundException.Message);
                break;

            case UnauthorizedAccessException unauthorizedException:
                response.Status = StatusCodes.Status401Unauthorized;
                response.Type = "https://tools.ietf.org/html/rfc7235#section-3.1";
                response.Title = "Unauthorized";
                response.Detail = unauthorizedException.Message;
                _logger.LogWarning("Unauthorized access attempt: {Message}", unauthorizedException.Message);
                break;

            case Exception generalException:
                response.Status = StatusCodes.Status500InternalServerError;
                response.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
                response.Title = "An error occurred processing your request";
                response.Detail = "An unexpected error occurred. Please try again later.";
                _logger.LogError(generalException, "Unhandled exception: {Message}", generalException.Message);
                break;
        }

        context.Result = new ObjectResult(response);
        context.ExceptionHandled = true;

        await Task.CompletedTask;
    }
}
