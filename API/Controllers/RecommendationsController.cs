using MediatR;
using Microsoft.AspNetCore.Mvc;
using Prospera.Contracts.DTOs.Recommendations;
using Prospera.Domain.Common;
using Prospera.Application.Features.Recommendations.Commands;
using Prospera.Application.Features.Recommendations.Queries;
using Prospera.Application.Common.Interfaces;

namespace Prospera.API.Controllers;

/// <summary>
/// Endpoints for AI-powered investment recommendations
/// Supports multiple LLM providers (Ollama, OpenRouter) with model selection
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RecommendationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RecommendationsController> _logger;
    private readonly ILlmProviderFactory _llmProviderFactory;

    public RecommendationsController(
        IMediator mediator,
        ILogger<RecommendationsController> logger,
        ILlmProviderFactory llmProviderFactory)
    {
        _mediator = mediator;
        _logger = logger;
        _llmProviderFactory = llmProviderFactory;
    }

    /// <summary>
    /// Get available LLM models and providers for recommendation generation
    /// </summary>
    /// <returns>Dictionary of available providers and their models with detailed info</returns>
    /// <response code="200">Available models retrieved successfully</response>
    [HttpGet("models")]
    [ProducesResponseType(typeof(AvailableModelsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableModels(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching available LLM models and providers");
        
        try
        {
            var providersModels = await _llmProviderFactory.GetAllProvidersWithModelsAsync(cancellationToken);
            
            var response = new AvailableModelsResponse
            {
                DefaultProvider = "Ollama",
                Providers = new Dictionary<string, List<ModelInfo>>()
            };

            // Map provider models to response DTOs
            foreach (var kvp in providersModels)
            {
                var providerName = kvp.Key;
                var models = kvp.Value;
                
                response.Providers[providerName] = models
                    .Select(m => new ModelInfo
                    {
                        ModelId = m.ModelId,
                        DisplayName = m.DisplayName,
                        Provider = m.Provider,
                        Description = m.Description,
                        IsAvailable = m.IsAvailable,
                        CostPer1kTokens = m.CostPer1kTokens,
                        ContextWindow = m.ContextWindow
                    })
                    .ToList();
            }

            // Add recommended models
            response.RecommendedModels = new List<RecommendedModel>
            {
                new()
                {
                    Category = "Best for Local/Offline (Recommended)",
                    ModelId = "llama3.2",
                    DisplayName = "Llama 3.2",
                    Provider = "Ollama",
                    Reason = "State-of-the-art open-source model, runs locally, no cost, good balance of quality and speed"
                },
                new()
                {
                    Category = "Alternative Local Option",
                    ModelId = "mistral",
                    DisplayName = "Mistral 7B",
                    Provider = "Ollama",
                    Reason = "Efficient and fast, excellent for real-time recommendations, lower resource requirements"
                },
                new()
                {
                    Category = "Best for Cloud (High Quality)",
                    ModelId = "openai/gpt-4-turbo-preview",
                    DisplayName = "GPT-4 Turbo",
                    Provider = "OpenRouter",
                    Reason = "Most capable model, best quality recommendations, supports 128K token context"
                },
                new()
                {
                    Category = "Best for Cloud (Cost-Effective)",
                    ModelId = "anthropic/claude-3-sonnet",
                    DisplayName = "Claude 3 Sonnet",
                    Provider = "OpenRouter",
                    Reason = "Excellent balance of quality and cost, good for most use cases"
                },
                new()
                {
                    Category = "Budget Cloud Option",
                    ModelId = "openai/gpt-3.5-turbo",
                    DisplayName = "GPT-3.5 Turbo",
                    Provider = "OpenRouter",
                    Reason = "Fast and affordable, suitable for quick recommendations"
                }
            };

            _logger.LogInformation("Successfully retrieved {ProviderCount} providers with {ModelCount} total models",
                response.Providers.Count,
                response.Providers.Values.Sum(m => m.Count));

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching available models");
            throw;
        }
    }

    /// <summary>
    /// Generate AI-powered investment recommendations for a user
    /// Supports model and provider selection via request parameters
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="request">Recommendation request with optional provider and model selection</param>
    /// <returns>Generated investment recommendation with allocation strategy</returns>
    /// <response code="201">Recommendation generated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">User not found</response>
    [HttpPost("users/{userId}")]
    [ProducesResponseType(typeof(InvestmentRecommendationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateRecommendation(
        Guid userId,
        GenerateInvestmentRecommendationRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Generating investment recommendation for user: {UserId} using provider: {Provider}, model: {Model}",
            userId,
            request.Provider,
            request.ModelName ?? "default");
        
        try
        {
            var command = new GenerateRecommendationCommand 
            { 
                UserId = userId, 
                AnalysisContext = request.AnalysisContext,
                ModelName = request.ModelName,
                Provider = request.Provider,
                CustomEndpoint = request.CustomEndpoint
            };
            var result = await _mediator.Send(command, cancellationToken);
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
    public async Task<IActionResult> GetRecommendationById(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching recommendation {RecommendationId} for user {UserId}", id, userId);
        
        try
        {
            var query = new GetRecommendationQuery { UserId = userId, RecommendationId = id };
            var result = await _mediator.Send(query, cancellationToken);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
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
    public async Task<IActionResult> GetUserRecommendations(
        Guid userId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all recommendations for user: {UserId}", userId);

        try
        {
            var query = new GetUserRecommendationsQuery { UserId = userId };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching recommendations for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Convert an investment recommendation into a coaching session
    /// Creates a personalized coaching goal and action plan based on the recommendation
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="recommendationId">The recommendation ID to convert</param>
    /// <param name="request">Optional coaching preferences for the session</param>
    /// <returns>Created coaching session with action plan</returns>
    /// <response code="201">Coaching session created successfully</response>
    /// <response code="404">Recommendation not found or does not belong to user</response>
    [HttpPost("users/{userId}/{recommendationId}/to-coaching")]
    [ProducesResponseType(typeof(Prospera.Application.DTOs.Coaching.CoachingSessionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConvertToCoaching(
        Guid userId,
        Guid recommendationId,
        [FromBody] ConvertToCoachingRequest? request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Converting recommendation {RecommendationId} to coaching session for user {UserId}",
            recommendationId,
            userId);

        try
        {
            var command = new ConvertRecommendationToCoachingCommand
            {
                UserId = userId,
                RecommendationId = recommendationId,
                GoalOverride = request?.GoalOverride,
                Provider = (LlmProvider)(request?.Provider ?? 0),
                ModelName = request?.ModelName
            };

            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(
                "GetCoachingSession",
                new { userId, sessionId = result.Id },
                result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Recommendation not found: {RecommendationId}", recommendationId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting recommendation to coaching for user: {UserId}", userId);
            throw;
        }
    }
}

/// <summary>
/// Request model for converting a recommendation to a coaching session
/// </summary>
public class ConvertToCoachingRequest
{
    /// <summary>
    /// Optional custom goal to override the recommendation-based goal
    /// </summary>
    public string? GoalOverride { get; set; }

    /// <summary>
    /// LLM provider (0=default, 1=OpenRouter, etc.)
    /// </summary>
    public int Provider { get; set; } = 0;

    /// <summary>
    /// Specific model name to use for the coaching session
    /// </summary>
    public string? ModelName { get; set; }
}
