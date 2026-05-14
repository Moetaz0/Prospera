using MediatR;
using Prospera.Application.DTOs.Coaching;
using Prospera.Domain.Common;

namespace Prospera.Application.Features.Recommendations.Commands;

/// <summary>
/// Command to convert an investment recommendation into a coaching session
/// Creates a coaching path focused on achieving the recommended allocation
/// </summary>
public class ConvertRecommendationToCoachingCommand : IRequest<CoachingSessionDto>
{
    public required Guid UserId { get; set; }
    public required Guid RecommendationId { get; set; }

    /// <summary>
    /// Optional: Override the coaching goal with custom text
    /// If not provided, uses the recommendation's suggested allocation as the goal
    /// </summary>
    public string? GoalOverride { get; set; }

    /// <summary>
    /// Which LLM provider to use for coaching session generation (Ollama or OpenRouter)
    /// Defaults to Ollama if not specified
    /// </summary>
    public LlmProvider Provider { get; set; } = LlmProvider.Ollama;

    /// <summary>
    /// Optional: The AI model to use for coaching generation
    /// If not provided, uses default from configuration
    /// </summary>
    public string? ModelName { get; set; }
}
