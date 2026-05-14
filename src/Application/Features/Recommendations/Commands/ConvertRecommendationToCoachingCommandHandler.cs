using MediatR;
using Microsoft.Extensions.Logging;
using Prospera.Application.Common.Interfaces;
using Prospera.Application.DTOs.Coaching;
using Prospera.Application.Features.Coaching.Commands;
using Prospera.Domain.Interfaces;

namespace Prospera.Application.Features.Recommendations.Commands;

/// <summary>
/// Handler for converting an investment recommendation into a coaching session
/// Creates a personalized coaching goal and action plan based on the recommendation
/// </summary>
public class ConvertRecommendationToCoachingCommandHandler : IRequestHandler<ConvertRecommendationToCoachingCommand, CoachingSessionDto>
{
    private readonly IInvestmentRecommendationRepository _recommendationRepository;
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<ConvertRecommendationToCoachingCommandHandler> _logger;

    public ConvertRecommendationToCoachingCommandHandler(
        IInvestmentRecommendationRepository recommendationRepository,
        IMediator mediator,
        IApplicationDbContext dbContext,
        ILogger<ConvertRecommendationToCoachingCommandHandler> logger)
    {
        _recommendationRepository = recommendationRepository;
        _mediator = mediator;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<CoachingSessionDto> Handle(ConvertRecommendationToCoachingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Converting recommendation {RecommendationId} to coaching session for user {UserId}",
                request.RecommendationId, request.UserId);

            // Load the recommendation
            var recommendations = _dbContext.InvestmentRecommendations
                .Where(r => r.Id == request.RecommendationId && r.UserId == request.UserId)
                .ToList();

            if (!recommendations.Any())
                throw new KeyNotFoundException($"Recommendation {request.RecommendationId} not found");

            var recommendation = recommendations.First();

            // Load user's financial profile
            var userAssets = _dbContext.Assets.Where(a => a.UserId == request.UserId).ToList();
            var userLiabilities = _dbContext.Liabilities.Where(l => l.UserId == request.UserId).ToList();

            var totalAssets = userAssets.Sum(a => a.CurrentValue);
            var totalLiabilities = userLiabilities.Sum(l => l.Amount);

            // Compute current allocation percentages by type
            var currentAllocationNarrative = BuildCurrentAllocationNarrative(userAssets, totalAssets);

            // Build situation narrative comparing current to recommended
            var situation = $"My current portfolio: {currentAllocationNarrative}. " +
                           $"An AI recommendation suggests: {recommendation.SuggestedAllocation}. " +
                           $"Reason: {recommendation.Explanation}";

            // Build the coaching goal
            var goal = request.GoalOverride ?? $"Rebalance my portfolio to: {recommendation.SuggestedAllocation}";

            // Create and dispatch coaching session command
            var coachingCommand = new StartCoachingSessionCommand
            {
                UserId = request.UserId,
                CurrentSituation = situation,
                Goal = goal,
                Preferences = null,
                Provider = (int)request.Provider,
                ModelName = request.ModelName
            };

            var coachingSession = await _mediator.Send(coachingCommand, cancellationToken);

            _logger.LogInformation("Coaching session created successfully from recommendation");
            return coachingSession;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Recommendation not found");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting recommendation to coaching");
            throw;
        }
    }

    /// <summary>
    /// Build a narrative describing current portfolio allocation by asset type
    /// </summary>
    private string BuildCurrentAllocationNarrative(List<Prospera.Domain.Entities.Asset> userAssets, decimal totalAssets)
    {
        if (!userAssets.Any())
            return "no assets";

        var assetsByType = userAssets
            .GroupBy(a => a.Type)
            .OrderByDescending(g => g.Sum(a => a.CurrentValue))
            .ToList();

        var parts = new List<string>();
        foreach (var group in assetsByType)
        {
            var percentage = totalAssets > 0 ? (group.Sum(a => a.CurrentValue) / totalAssets * 100) : 0;
            parts.Add($"{(int)percentage}% {group.Key}");
        }

        return string.Join(", ", parts);
    }
}
