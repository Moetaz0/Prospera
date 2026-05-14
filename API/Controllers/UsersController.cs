using System;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prospera.Contracts.DTOs.User;
using Prospera.Application.Features.Users.Commands;
using Prospera.Domain.Interfaces;

namespace Prospera.API.Controllers;

/// <summary>
/// Endpoints for managing user accounts and financial profiles
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersController> _logger;
    private readonly IUserRepository _userRepository;

    public UsersController(
        IMediator mediator,
        ILogger<UsersController> logger,
        IUserRepository userRepository)
    {
        _mediator = mediator;
        _logger = logger;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    /// <returns>List of users</returns>
    /// <response code="200">Users retrieved successfully</response>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers()
    {
        _logger.LogInformation("Admin requesting all users");

        var users = await _userRepository.GetAllAsync();

        var response = users.Select(u =>
        {
            // Parse first and last names from full name
            var nameParts = (u.FullName ?? "").Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
            var firstName = nameParts.Length > 0 ? nameParts[0] : "";
            var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";

            return new Prospera.Contracts.DTOs.User.AdminUserDto
            {
                UserId = u.Id.ToString(),
                Email = u.Email,
                FirstName = firstName,
                LastName = lastName,
                IsAdmin = u.Role == "Admin",
                CreatedAt = DateTime.UtcNow,
                LastLogin = null
            };
        });

        return Ok(response);
    }

    /// <summary>
    /// Get user by ID with full dashboard information
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>User dashboard with assets, liabilities, and financial metrics</returns>
    /// <response code="200">User found</response>
    /// <response code="404">User not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        _logger.LogInformation("Fetching user dashboard for ID: {UserId}", id);

        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", id);
                return NotFound(new { message = "User not found" });
            }

            var response = new UserDashboardDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                RiskProfile = Enum.Parse<Prospera.Contracts.Enums.RiskProfile>(user.RiskProfile.ToString()),
                NetWorth = user.CalculateNetWorth(),
                CreatedAt = DateTime.UtcNow
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user with ID: {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Failed to retrieve user", error = ex.Message });
        }
    }

    /// <summary>
    /// Update user profile information
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="request">User update request</param>
    /// <returns>Updated user information</returns>
    /// <response code="200">User updated successfully</response>
    /// <response code="404">User not found</response>
    /// <response code="400">Invalid request data</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUser(Guid id, UpdateUserRequest request)
    {
        _logger.LogInformation("Updating user with ID: {UserId}", id);

        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", id);
                return NotFound(new { message = "User not found" });
            }

            // Build full name from first and last name if provided, otherwise use FullName
            string? fullName = null;
            if (!string.IsNullOrWhiteSpace(request.FirstName) && !string.IsNullOrWhiteSpace(request.LastName))
            {
                fullName = $"{request.FirstName} {request.LastName}";
            }
            else if (!string.IsNullOrWhiteSpace(request.FullName))
            {
                fullName = request.FullName;
            }

            // Convert RiskProfile enum if needed
            Prospera.Domain.Enums.RiskProfile? domainRiskProfile = null;
            if (request.RiskProfile.HasValue)
            {
                domainRiskProfile = Enum.Parse<Prospera.Domain.Enums.RiskProfile>(request.RiskProfile.Value.ToString());
            }

            // Update user profile
            user.UpdateProfile(fullName, request.Email, domainRiskProfile);

            // Update admin status if requested
            if (request.IsAdmin.HasValue)
            {
                var newRole = request.IsAdmin.Value ? "Admin" : "User";
                user.SetRole(newRole);
            }

            await _userRepository.UpdateAsync(user);

            var response = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                RiskProfile = Enum.Parse<Prospera.Contracts.Enums.RiskProfile>(user.RiskProfile.ToString()),
                NetWorth = user.CalculateNetWorth(),
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Successfully updated user with ID: {UserId}", id);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID: {UserId}", id);
            return BadRequest(new { message = "Failed to update user", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a user account
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>No content</returns>
    /// <response code="204">User deleted successfully</response>
    /// <response code="404">User not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        _logger.LogInformation("Deleting user with ID: {UserId}", id);

        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", id);
                return NotFound(new { message = "User not found" });
            }

            await _userRepository.DeleteAsync(id);
            _logger.LogInformation("Successfully deleted user with ID: {UserId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
            return BadRequest(new { message = "Failed to delete user", error = ex.Message });
        }
    }
}
