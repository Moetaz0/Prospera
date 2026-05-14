using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prospera.Application.Features.Liabilities.Commands;
using Prospera.Application.Features.Liabilities.Queries;
using Prospera.Contracts.DTOs.Liability;

namespace Prospera.API.Controllers;

/// <summary>
/// Endpoints for managing user liabilities (debts)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class LiabilitiesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<LiabilitiesController> _logger;

    public LiabilitiesController(IMediator mediator, ILogger<LiabilitiesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Add a new liability to user's portfolio
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="request">Liability details (name, amount, type)</param>
    /// <returns>The created liability</returns>
    /// <response code="201">Liability created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">User not found</response>
    [HttpPost("users/{userId}")]
    [ProducesResponseType(typeof(LiabilityDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddLiability(Guid userId, AddLiabilityRequest request)
    {
        _logger.LogInformation("Adding liability for user: {UserId}, Type: {LiabilityType}", userId, request.Type);

        try
        {
            var command = new AddLiabilityCommand 
            { 
                UserId = userId, 
                Name = request.Name ?? string.Empty, 
                Amount = request.Amount,
                Type = request.Type
            };
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetLiabilityById), new { userId, id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding liability for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get liability by ID
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="id">The liability ID</param>
    /// <returns>Liability details</returns>
    /// <response code="200">Liability found</response>
    /// <response code="404">Liability or user not found</response>
    [HttpGet("users/{userId}/liabilities/{id}")]
    [ProducesResponseType(typeof(LiabilityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLiabilityById(Guid userId, Guid id)
    {
        _logger.LogInformation("Fetching liability {LiabilityId} for user {UserId}", id, userId);

        try
        {
            var query = new GetLiabilityQuery { UserId = userId, LiabilityId = id };
            var result = await _mediator.Send(query);

            if (result == null)
            {
                _logger.LogWarning("Liability {LiabilityId} not found for user {UserId}", id, userId);
                return NotFound();
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching liability {LiabilityId}", id);
            throw;
        }
    }

    /// <summary>
    /// Get all liabilities for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>List of liabilities</returns>
    /// <response code="200">Liabilities retrieved</response>
    /// <response code="404">User not found</response>
    [HttpGet("users/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<LiabilityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserLiabilities(Guid userId)
    {
        _logger.LogInformation("Fetching liabilities for user: {UserId}", userId);

        try
        {
            var query = new GetUserLiabilitiesQuery { UserId = userId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching liabilities for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Update liability details
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="id">The liability ID</param>
    /// <param name="request">Updated liability information</param>
    /// <returns>Updated liability</returns>
    /// <response code="200">Liability updated</response>
    /// <response code="404">Liability or user not found</response>
    [HttpPut("users/{userId}/liabilities/{id}")]
    [ProducesResponseType(typeof(LiabilityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLiability(Guid userId, Guid id, AddLiabilityRequest request)
    {
        _logger.LogInformation("Updating liability {LiabilityId} for user {UserId}", id, userId);

        try
        {
            var command = new UpdateLiabilityCommand 
            { 
                UserId = userId, 
                LiabilityId = id, 
                Name = request.Name ?? string.Empty, 
                Amount = request.Amount,
                Type = request.Type.ToString()
            };
            var result = await _mediator.Send(command);

            if (result == null)
            {
                _logger.LogWarning("Liability {LiabilityId} not found for user {UserId}", id, userId);
                return NotFound();
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating liability {LiabilityId}", id);
            throw;
        }
    }

    /// <summary>
    /// Delete a liability
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="id">The liability ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Liability deleted</response>
    /// <response code="404">Liability or user not found</response>
    [HttpDelete("users/{userId}/liabilities/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLiability(Guid userId, Guid id)
    {
        _logger.LogInformation("Deleting liability {LiabilityId} for user {UserId}", id, userId);

        try
        {
            var command = new DeleteLiabilityCommand { UserId = userId, LiabilityId = id };
            var result = await _mediator.Send(command);

            if (!result)
            {
                _logger.LogWarning("Liability {LiabilityId} not found for user {UserId}", id, userId);
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting liability {LiabilityId}", id);
            throw;
        }
    }
}
