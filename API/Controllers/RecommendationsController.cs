using MediatR;
using Microsoft.AspNetCore.Mvc;
using Prospera.Contracts.DTOs.Recommendations;
using Prospera.Application.Features.Recommendations.Commands;

namespace Prospera.API.Controllers;

/// <summary>
/// Endpoints for AI-powered investment recommendations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RecommendationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RecommendationsController> _logger;

    public RecommendationsController(IMediator mediator, ILogger<RecommendationsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Generate AI-powered investment recommendations for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="request">Optional analysis context for recommendation generation</param>
    /// <returns>Generated investment recommendation with allocation strategy</returns>
    /// <response code="201">Recommendation generated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">User not found</response>
    [HttpPost("users/{userId}")]
    [ProducesResponseType(typeof(InvestmentRecommendationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateRecommendation(Guid userId, GenerateInvestmentRecommendationRequest request)
    {
        _logger.LogInformation("Generating investment recommendation for user: {UserId}", userId);
        
        try
        {
            var command = new GenerateRecommendationCommand 
            { 
                UserId = userId, 
                AnalysisContext = request.AnalysisContext 
            };
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetRecommendationById), new { userId, id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recommendation for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get a specific recommendation by ID
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="id">The recommendation ID</param>
    /// <returns>Recommendation details</returns>
    /// <response code="200">Recommendation found</response>
    /// <response code="404">Recommendation or user not found</response>
    [HttpGet("users/{userId}/recommendations/{id}")]
    [ProducesResponseType(typeof(InvestmentRecommendationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRecommendationById(Guid userId, Guid id)
    {
        _logger.LogInformation("Fetching recommendation {RecommendationId} for user {UserId}", id, userId);
        
        try
        {
            // TODO: Send GetRecommendationQuery via MediatR
            // var query = new GetRecommendationQuery { UserId = userId, RecommendationId = id };
            // var result = await _mediator.Send(query);
            // return Ok(result);
            
            return StatusCode(StatusCodes.Status501NotImplemented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching recommendation {RecommendationId}", id);
            throw;
        }
    }

    /// <summary>
    /// Get all recommendations for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>List of recommendations</returns>
    /// <response code="200">Recommendations retrieved</response>
    /// <response code="404">User not found</response>
    [HttpGet("users/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<InvestmentRecommendationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserRecommendations(Guid userId)
    {
        _logger.LogInformation("Fetching all recommendations for user: {UserId}", userId);
        
        try
        {
            // TODO: Send GetUserRecommendationsQuery via MediatR
            // var query = new GetUserRecommendationsQuery { UserId = userId };
            // var result = await _mediator.Send(query);
            // return Ok(result);
            
            return StatusCode(StatusCodes.Status501NotImplemented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching recommendations for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get the latest recommendation for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Most recent recommendation</returns>
    /// <response code="200">Recommendation found</response>
    /// <response code="404">User not found or no recommendations exist</response>
    [HttpGet("users/{userId}/latest")]
    [ProducesResponseType(typeof(InvestmentRecommendationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLatestRecommendation(Guid userId)
    {
        _logger.LogInformation("Fetching latest recommendation for user: {UserId}", userId);
        
        try
        {
            // TODO: Send GetLatestRecommendationQuery via MediatR
            // var query = new GetLatestRecommendationQuery { UserId = userId };
            // var result = await _mediator.Send(query);
            // return Ok(result);
            
            return StatusCode(StatusCodes.Status501NotImplemented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching latest recommendation for user: {UserId}", userId);
            throw;
        }
    }
}
