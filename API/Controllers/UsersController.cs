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

        var response = users.Select(u => new UserDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            RiskProfile = Enum.Parse<Prospera.Contracts.Enums.RiskProfile>(u.RiskProfile.ToString()),
            NetWorth = (u.Assets?.Sum(a => a.CurrentValue) ?? 0) - (u.Liabilities?.Sum(l => l.Amount) ?? 0),
            CreatedAt = DateTime.UtcNow
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
            // TODO: Send GetUserDashboardQuery via MediatR
            // var query = new GetUserDashboardQuery { UserId = id };
            // var result = await _mediator.Send(query);
            // return Ok(result);
            
            return StatusCode(StatusCodes.Status501NotImplemented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user with ID: {UserId}", id);
            throw;
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
            // TODO: Send UpdateUserCommand via MediatR
            // var command = new UpdateUserCommand { UserId = id, ...request properties };
            // var result = await _mediator.Send(command);
            // return Ok(result);
            
            return StatusCode(StatusCodes.Status501NotImplemented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID: {UserId}", id);
            throw;
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
            // TODO: Send DeleteUserCommand via MediatR
            // var command = new DeleteUserCommand { UserId = id };
            // await _mediator.Send(command);
            // return NoContent();
            
            return StatusCode(StatusCodes.Status501NotImplemented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
            throw;
        }
    }
}
