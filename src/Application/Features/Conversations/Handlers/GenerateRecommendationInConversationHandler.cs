using MediatR;
using Microsoft.Extensions.Logging;
using Prospera.Domain.Entities;
using Prospera.Domain.Interfaces;
using Prospera.Domain.Common;
using Prospera.Application.DTOs;
using Prospera.Application.Features.Conversations.Commands;
using Prospera.Application.Features.Recommendations.Commands;

namespace Prospera.Application.Features.Conversations.Handlers;

/// <summary>
/// Handler for generating investment recommendations within a conversation context
/// Integrates recommendation generation into the conversation flow
/// </summary>
public class GenerateRecommendationInConversationHandler : IRequestHandler<GenerateRecommendationInConversationCommand, RecommendationInConversationResponseDto>
{
    private readonly IMediator _mediator;
    private readonly IConversationRepository _conversationRepository;
    private readonly ILogger<GenerateRecommendationInConversationHandler> _logger;

    public GenerateRecommendationInConversationHandler(
        IMediator mediator,
        IConversationRepository conversationRepository,
        ILogger<GenerateRecommendationInConversationHandler> logger)
    {
        _mediator = mediator;
        _conversationRepository = conversationRepository;
        _logger = logger;
    }

    public async Task<RecommendationInConversationResponseDto> Handle(
        GenerateRecommendationInConversationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Generating recommendation in conversation {ConversationId} for user {UserId}",
                request.ConversationId,
                request.UserId);

            // Load the conversation
            var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId, request.UserId);
            if (conversation == null)
                throw new KeyNotFoundException($"Conversation {request.ConversationId} not found");

            // Generate recommendation using existing handler
            var recommendationCommand = new GenerateRecommendationCommand
            {
                UserId = request.UserId,
                AnalysisContext = request.AnalysisContext,
                Provider = (LlmProvider)request.Provider,
                ModelName = request.ModelName
            };

            var recommendation = await _mediator.Send(recommendationCommand, cancellationToken);

            // Add recommendation to conversation
            conversation.AddRecommendation(recommendation.Id);

            // Add system message about the recommendation
            var recommendationMessage = new ConversationMessage(
                "AI",
                $"I've generated a personalized investment recommendation based on your profile. Here's my analysis:\n\n{recommendation.SuggestedAllocation}\n\n{recommendation.Explanation}",
                "Recommendation")
            {
                RecommendationId = recommendation.Id
            };

            conversation.AddMessage(recommendationMessage);

            // Save updated conversation
            await _conversationRepository.UpdateAsync(conversation);

            // Create response
            var response = new RecommendationInConversationResponseDto
            {
                ConversationId = request.ConversationId,
                RecommendationId = recommendation.Id,
                Recommendation = recommendation,
                Message = "Based on your financial profile and current situation, here's your personalized investment recommendation. Feel free to ask follow-up questions or request adjustments.",
                GeneratedAt = DateTime.UtcNow
            };

            _logger.LogInformation(
                "Recommendation {RecommendationId} generated successfully in conversation {ConversationId}",
                recommendation.Id,
                request.ConversationId);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recommendation in conversation for user {UserId}", request.UserId);
            throw;
        }
    }
}
