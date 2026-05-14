using MediatR;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Conversations.Commands;

/// <summary>
/// Command to generate an investment recommendation within a conversation context
/// This is an explicit request for a recommendation, integrated into the conversation
/// </summary>
public class GenerateRecommendationInConversationCommand : IRequest<RecommendationInConversationResponseDto>
{
    public required Guid UserId { get; set; }
    public required Guid ConversationId { get; set; }
    public string? AnalysisContext { get; set; } // User's specific situation or preferences
    public int Provider { get; set; } = 0;
    public string? ModelName { get; set; }
}

/// <summary>
/// Request model for generating recommendation in conversation
/// </summary>
public class GenerateRecommendationRequest
{
    public string? AnalysisContext { get; set; } // "I want to save for retirement", "I'm risk-averse", etc.
    public int Provider { get; set; } = 0;
    public string? ModelName { get; set; }
}

/// <summary>
/// Response when recommendation is generated in conversation
/// </summary>
public class RecommendationInConversationResponseDto
{
    public Guid ConversationId { get; set; }
    public Guid RecommendationId { get; set; }
    public InvestmentRecommendationDto Recommendation { get; set; }
    public string Message { get; set; } = string.Empty; // Intro message about the recommendation
    public DateTime GeneratedAt { get; set; }
}
