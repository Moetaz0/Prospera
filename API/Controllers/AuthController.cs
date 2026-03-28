using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prospera.Infrastructure.Identity;
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

        var response = new AuthResponse(data.Token, data.UserId, data.Email, data.FullName, data.Role);
        _logger.LogInformation("Response object: Token={Token}, UserId={UserId}, Email={Email}, FullName={FullName}, Role={Role}", 
            response.Token, response.UserId, response.Email, response.FullName, response.Role);

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

        var response = new AuthResponse(data.Token, data.UserId, data.Email, data.FullName, data.Role);
        _logger.LogInformation("Response object: Token={Token}, UserId={UserId}, Email={Email}, FullName={FullName}, Role={Role}", 
            response.Token, response.UserId, response.Email, response.FullName, response.Role);

        return Ok(response);
    }

    public record RegisterRequest(string Email, string Password, string FirstName, string LastName, int Age);
    public record LoginRequest(string Email, string Password);

    public record AuthResponse(
        [property: JsonPropertyName("token")] string Token,
        [property: JsonPropertyName("userId")] Guid UserId,
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("fullName")] string FullName,
        [property: JsonPropertyName("role")] string Role);

    public record ErrorResponse(
        [property: JsonPropertyName("error")] string? Error);
}
