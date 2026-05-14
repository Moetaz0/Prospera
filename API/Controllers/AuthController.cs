using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prospera.Infrastructure.Identity;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Prospera.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IIdentityService identityService, ILogger<AuthController> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

        var result = await _identityService.RegisterAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.Age);

        _logger.LogInformation("Registration result - IsSuccess: {IsSuccess}, Error: {Error}", result.IsSuccess, result.Error);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Registration failed for {Email}: {Error}", request.Email, result.Error);
            return BadRequest(new ErrorResponse(result.Error));
        }

        var data = result.Data!;
        _logger.LogInformation("Registration successful for {Email}", request.Email);

        var response = new AuthResponse(data.Token, data.RefreshToken, data.UserId, data.Email, data.FullName, data.Role);
        _logger.LogInformation("Response object: Token={Token}, RefreshToken={RefreshToken}, UserId={UserId}, Email={Email}, FullName={FullName}, Role={Role}", 
            response.Token, response.RefreshToken, response.UserId, response.Email, response.FullName, response.Role);

        return Ok(response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for email: {Email}", request.Email);

        var result = await _identityService.LoginAsync(request.Email, request.Password);

        _logger.LogInformation("Login result - IsSuccess: {IsSuccess}, Error: {Error}", result.IsSuccess, result.Error);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Login failed for {Email}: {Error}", request.Email, result.Error);
            return Unauthorized(new ErrorResponse(result.Error));
        }

        var data = result.Data!;
        _logger.LogInformation("Login successful for {Email}", request.Email);

        var response = new AuthResponse(data.Token, data.RefreshToken, data.UserId, data.Email, data.FullName, data.Role);
        _logger.LogInformation("Response object: Token={Token}, RefreshToken={RefreshToken}, UserId={UserId}, Email={Email}, FullName={FullName}, Role={Role}", 
            response.Token, response.RefreshToken, response.UserId, response.Email, response.FullName, response.Role);

        return Ok(response);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ForgotPasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        _logger.LogInformation("Forgot password request for email: {Email}", request.Email);

        var result = await _identityService.ForgotPasswordAsync(request.Email);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Forgot password failed for {Email}: {Error}", request.Email, result.Error);
            return NotFound(new ErrorResponse(result.Error));
        }

        _logger.LogInformation("Password reset token generated for {Email}", request.Email);
        return Ok(new ForgotPasswordResponse("Password reset token sent to your email. Use this token to reset your password."));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        _logger.LogInformation("Reset password request for email: {Email}", request.Email);

        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return BadRequest(new ErrorResponse("Reset token is required"));
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(new ErrorResponse("New password is required"));
        }

        var result = await _identityService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Reset password failed for {Email}: {Error}", request.Email, result.Error);
            return BadRequest(new ErrorResponse(result.Error));
        }

        _logger.LogInformation("Password reset successful for {Email}", request.Email);
        return Ok(new MessageResponse("Password reset successful"));
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Invalid user ID claim");
            return Unauthorized(new ErrorResponse("Invalid user claims"));
        }

        _logger.LogInformation("Fetching current user: {UserId}", userId);

        var result = await _identityService.GetCurrentUserAsync(userId);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to fetch current user {UserId}: {Error}", userId, result.Error);
            return NotFound(new ErrorResponse(result.Error));
        }

        var data = result.Data!;
        var response = new AuthResponse(data.Token, data.RefreshToken, data.UserId, data.Email, data.FullName, data.Role);
        return Ok(response);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        _logger.LogInformation("Refresh token request received");

        var result = await _identityService.RefreshTokenAsync(request.RefreshToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Refresh token failed: {Error}", result.Error);
            return Unauthorized(new ErrorResponse(result.Error));
        }

        var data = result.Data!;
        _logger.LogInformation("Refresh token successful for user: {UserId}", data.UserId);

        var response = new AuthResponse(data.Token, data.RefreshToken, data.UserId, data.Email, data.FullName, data.Role);
        return Ok(response);
    }

    public record RegisterRequest(string Email, string Password, string FirstName, string LastName, int Age);
    public record LoginRequest(string Email, string Password);
    public record RefreshTokenRequest(string RefreshToken);
    public record ForgotPasswordRequest(string Email);
    public record ResetPasswordRequest(string Email, string Token, string NewPassword);

    public record AuthResponse(
        [property: JsonPropertyName("token")] string Token,
        [property: JsonPropertyName("refreshToken")] string RefreshToken,
        [property: JsonPropertyName("userId")] Guid UserId,
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("fullName")] string FullName,
        [property: JsonPropertyName("role")] string Role);

    public record ErrorResponse(
        [property: JsonPropertyName("error")] string? Error);

    public record ForgotPasswordResponse(
        [property: JsonPropertyName("message")] string Message);

    public record MessageResponse(
        [property: JsonPropertyName("message")] string Message);
}
