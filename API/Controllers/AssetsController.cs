using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prospera.Application.Features.Assets.Commands;
using Prospera.Application.Features.Assets.Queries;
using Prospera.Contracts.DTOs.Asset;

namespace Prospera.API.Controllers;

/// <summary>
/// Endpoints for managing user assets
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class AssetsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AssetsController> _logger;

    public AssetsController(IMediator mediator, ILogger<AssetsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Add a new asset to user's portfolio
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="request">Asset details (name, value, type)</param>
    /// <returns>The created asset</returns>
    /// <response code="201">Asset created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">User not found</response>
    [HttpPost("users/{userId}")]
    [ProducesResponseType(typeof(AssetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddAsset(Guid userId, AddAssetRequest request)
    {
        _logger.LogInformation("Adding asset for user: {UserId}, Type: {AssetType}", userId, request.Type);
        
        try
        {
            var command = new AddAssetCommand 
            { 
                UserId = userId, 
                Name = request.Name, 
                CurrentValue = request.CurrentValue,
                Type = request.Type
            };
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAssetById), new { userId, id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding asset for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get asset by ID
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="id">The asset ID</param>
    /// <returns>Asset details</returns>
    /// <response code="200">Asset found</response>
    /// <response code="404">Asset or user not found</response>
    [HttpGet("users/{userId}/assets/{id}")]
    [ProducesResponseType(typeof(AssetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAssetById(Guid userId, Guid id)
    {
        _logger.LogInformation("Fetching asset {AssetId} for user {UserId}", id, userId);

        try
        {
            var query = new GetAssetQuery { UserId = userId, AssetId = id };
            var result = await _mediator.Send(query);

            if (result == null)
            {
                _logger.LogWarning("Asset {AssetId} not found for user {UserId}", id, userId);
                return NotFound();
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching asset {AssetId}", id);
            throw;
        }
    }

    /// <summary>
    /// Get all assets for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>List of assets</returns>
    /// <response code="200">Assets retrieved</response>
    /// <response code="404">User not found</response>
    [HttpGet("users/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<AssetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserAssets(Guid userId)
    {
        _logger.LogInformation("Fetching assets for user: {UserId}", userId);

        try
        {
            var query = new GetUserAssetsQuery { UserId = userId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching assets for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get asset with projected valuations (1 year ahead)
    /// Automatically fetches rates based on asset type:
    /// - Cash/Stocks/Bonds/Crypto: Inflation adjustment from World Bank
    /// - Cars: Depreciation from Python financial-api
    /// - Real Estate: Appreciation from regional data
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="id">The asset ID</param>
    /// <returns>Asset valuation with 1-year projections and detailed breakdown</returns>
    /// <response code="200">Valuation calculated successfully</response>
    /// <response code="404">Asset or user not found</response>
    [HttpGet("users/{userId}/assets/{id}/valuation")]
    [ProducesResponseType(typeof(Prospera.Application.DTOs.AssetValuationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAssetValuation(Guid userId, Guid id)
    {
        _logger.LogInformation(
            "Getting asset valuation for {AssetId}, user {UserId}",
            id, userId);

        try
        {
            var query = new GetAssetValuationQuery
            {
                UserId = userId,
                AssetId = id
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Asset {AssetId} not found", id);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to asset {AssetId}", id);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting asset valuation for {AssetId}", id);
            throw;
        }
    }

    /// <summary>
    /// <param name="userId">The user ID</param>
    /// <param name="id">The asset ID</param>
    /// <param name="request">Updated asset information</param>
    /// <returns>Updated asset</returns>
    /// <response code="200">Asset updated</response>
    /// <response code="404">Asset or user not found</response>
    [HttpPut("users/{userId}/assets/{id}")]
    [ProducesResponseType(typeof(AssetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsset(Guid userId, Guid id, AddAssetRequest request)
    {
        _logger.LogInformation("Updating asset {AssetId} for user {UserId}", id, userId);

        try
        {
            var command = new UpdateAssetCommand 
            { 
                UserId = userId, 
                AssetId = id, 
                Name = request.Name, 
                CurrentValue = request.CurrentValue,
                Type = request.Type.ToString()
            };
            var result = await _mediator.Send(command);

            if (result == null)
            {
                _logger.LogWarning("Asset {AssetId} not found for user {UserId}", id, userId);
                return NotFound();
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating asset {AssetId}", id);
            throw;
        }
    }

    /// <summary>
    /// Delete an asset
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="id">The asset ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Asset deleted</response>
    /// <response code="404">Asset or user not found</response>
    [HttpDelete("users/{userId}/assets/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsset(Guid userId, Guid id)
    {
        _logger.LogInformation("Deleting asset {AssetId} for user {UserId}", id, userId);

        try
        {
            var command = new DeleteAssetCommand { UserId = userId, AssetId = id };
            var result = await _mediator.Send(command);

            if (!result)
            {
                _logger.LogWarning("Asset {AssetId} not found for user {UserId}", id, userId);
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting asset {AssetId}", id);
            throw;
        }
    }
}
