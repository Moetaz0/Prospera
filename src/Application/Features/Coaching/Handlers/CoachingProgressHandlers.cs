using MediatR;
using AutoMapper;
using Prospera.Application.DTOs.Coaching;
using Prospera.Application.Common.Interfaces;
using Prospera.Application.Features.Coaching.Commands;

namespace Prospera.Application.Features.Coaching.Handlers;

/// <summary>
/// Handler for getting coaching session progress
/// Includes market context and data-driven recommendations
/// </summary>
public class GetCoachingProgressCommandHandler : IRequestHandler<GetCoachingProgressCommand, CoachingProgressDto>
{
    private readonly ICoachingSessionRepository _coachingSessionRepository;
    private readonly IFinancialDataService _financialDataService;
    private readonly IMapper _mapper;

    public GetCoachingProgressCommandHandler(
        ICoachingSessionRepository coachingSessionRepository,
        IFinancialDataService financialDataService,
        IMapper mapper)
    {
        _coachingSessionRepository = coachingSessionRepository;
        _financialDataService = financialDataService;
        _mapper = mapper;
    }

    public async Task<CoachingProgressDto> Handle(GetCoachingProgressCommand request, CancellationToken cancellationToken)
    {
        var session = await _coachingSessionRepository.GetByIdWithItemsAsync(request.SessionId, cancellationToken);
        if (session == null)
        {
            throw new KeyNotFoundException($"Coaching session {request.SessionId} not found");
        }

        var pending = session.ActionItems.Where(a => !a.IsCompleted).ToList();
        var completed = session.ActionItems.Where(a => a.IsCompleted).ToList();

        // Gather market data for enhanced recommendations (non-blocking)
        var inflationData = await _financialDataService.GetInflationDataAsync("TN", cancellationToken).ConfigureAwait(false);
        var gdpPrediction = await _financialDataService.GetGdpPredictionAsync("TN", cancellationToken).ConfigureAwait(false);

        var dto = new CoachingProgressDto
        {
            SessionId = session.Id,
            SessionTitle = session.SessionTitle,
            Goal = session.Goal,
            ProgressPercentage = session.ProgressPercentage,
            TotalActionItems = session.ActionItems.Count,
            CompletedActionItems = completed.Count,
            PendingActionItems = _mapper.Map<List<ActionItemDto>>(pending),
            CompletedItems = _mapper.Map<List<ActionItemDto>>(completed),
            Milestones = _mapper.Map<List<MilestoneDto>>(session.Milestones),
            NextRecommendation = GetNextRecommendationWithContext(
                session.ProgressPercentage, 
                pending.Count, 
                session.Goal,
                inflationData,
                gdpPrediction)
        };

        return dto;
    }

    private string GetNextRecommendationWithContext(
        decimal progress, 
        int pendingItems, 
        string goal,
        InflationData? inflation,
        PredictionData? gdpPrediction)
    {
        var baseRecommendation = GetNextRecommendation(progress, pendingItems, goal);

        // Append market context if available
        if (inflation != null || gdpPrediction != null)
        {
            baseRecommendation += " | Market Context: ";

            if (inflation != null && inflation.CurrentRate > 5)
            {
                baseRecommendation += $"High inflation ({inflation.CurrentRate:F1}%) - prioritize actions that protect/grow your money.";
            }
            else if (inflation != null && inflation.CurrentRate > 0)
            {
                baseRecommendation += $"Inflation at {inflation.CurrentRate:F1}% - factor this into your savings goals.";
            }

            if (gdpPrediction != null && !string.IsNullOrEmpty(gdpPrediction.Reasoning))
            {
                baseRecommendation += $" Economic outlook: {gdpPrediction.Reasoning}";
            }
        }

        return baseRecommendation;
    }

    private string GetNextRecommendation(decimal progress, int pendingItems, string goal)
    {
        if (pendingItems == 0 && progress < 100)
            return $"All action items are complete! Mark them off as you finish them to track progress toward: {goal}";

        return progress switch
        {
            < 25 => $"Focus on completing your first 25% of action items. You're building momentum!",
            < 50 => $"You're making progress! Keep going - you're halfway to your goal of: {goal}",
            < 75 => $"Excellent work! You're in the home stretch. Keep the momentum going.",
            < 100 => $"Almost there! Finish these last few items to complete your coaching goal.",
            _ => $"🎉 Congratulations! You've completed your coaching goal: {goal}"
        };
    }
}

/// <summary>
/// Handler for completing a coaching action item
/// Includes market-aware progress recommendations
/// </summary>
public class CompleteCoachingActionCommandHandler : IRequestHandler<CompleteCoachingActionCommand, CoachingProgressDto>
{
    private readonly ICoachingSessionRepository _coachingSessionRepository;
    private readonly IFinancialDataService _financialDataService;
    private readonly IMapper _mapper;

    public CompleteCoachingActionCommandHandler(
        ICoachingSessionRepository coachingSessionRepository,
        IFinancialDataService financialDataService,
        IMapper mapper)
    {
        _coachingSessionRepository = coachingSessionRepository;
        _financialDataService = financialDataService;
        _mapper = mapper;
    }

    public async Task<CoachingProgressDto> Handle(CompleteCoachingActionCommand request, CancellationToken cancellationToken)
    {
        var session = await _coachingSessionRepository.GetByIdWithItemsAsync(request.SessionId, cancellationToken);
        if (session == null)
        {
            throw new KeyNotFoundException($"Coaching session {request.SessionId} not found");
        }

        session.CompleteActionItem(request.ActionItemId);
        await _coachingSessionRepository.UpdateAsync(session, cancellationToken);

        var pending = session.ActionItems.Where(a => !a.IsCompleted).ToList();
        var completed = session.ActionItems.Where(a => a.IsCompleted).ToList();

        // Gather market data for context-aware next recommendations
        var inflationData = await _financialDataService.GetInflationDataAsync("TN", cancellationToken).ConfigureAwait(false);
        var gdpPrediction = await _financialDataService.GetGdpPredictionAsync("TN", cancellationToken).ConfigureAwait(false);

        var dto = new CoachingProgressDto
        {
            SessionId = session.Id,
            SessionTitle = session.SessionTitle,
            Goal = session.Goal,
            ProgressPercentage = session.ProgressPercentage,
            TotalActionItems = session.ActionItems.Count,
            CompletedActionItems = completed.Count,
            PendingActionItems = _mapper.Map<List<ActionItemDto>>(pending),
            CompletedItems = _mapper.Map<List<ActionItemDto>>(completed),
            Milestones = _mapper.Map<List<MilestoneDto>>(session.Milestones),
            NextRecommendation = GetNextRecommendationWithMarketContext(
                session.ProgressPercentage, 
                pending.Count, 
                session.Goal,
                inflationData,
                gdpPrediction)
        };

        return dto;
    }

    private string GetNextRecommendationWithMarketContext(
        decimal progress,
        int pendingItems,
        string goal,
        InflationData? inflation,
        PredictionData? gdpPrediction)
    {
        var recommendation = GetNextRecommendation(progress, pendingItems, goal);

        // Add market insight if available
        if (inflation?.CurrentRate > 3)
        {
            recommendation += $"\n💡 Pro tip: With inflation at {inflation.CurrentRate:F1}%, make sure your savings are in instruments that outpace inflation.";
        }

        if (progress >= 50 && inflation?.CurrentRate < 2)
        {
            recommendation += "\n💡 Great progress! Low inflation environment - a good time to plan long-term investments.";
        }

        return recommendation;
    }

    private string GetNextRecommendation(decimal progress, int pendingItems, string goal)
    {
        if (pendingItems == 0 && progress < 100)
            return $"🎉 All action items are complete! You're on the final stretch toward: {goal}";

        return progress switch
        {
            < 25 => "🚀 Great start! You're building momentum. Keep the energy going!",
            < 50 => "💪 Halfway there! You're making real progress. The hardest part is behind you.",
            < 75 => "🌟 Excellent work! You're in the home stretch. Keep pushing!",
            < 100 => "🔥 Almost there! Just a few more actions to complete your goal!",
            _ => "🏆 Congratulations! You've completed your coaching goal!"
        };
    }
}

/// <summary>
/// Handler for getting coaching history
/// </summary>
public class GetCoachingHistoryCommandHandler : IRequestHandler<GetCoachingHistoryCommand, CoachingHistoryDto>
{
    private readonly ICoachingSessionRepository _coachingSessionRepository;
    private readonly IMapper _mapper;

    public GetCoachingHistoryCommandHandler(
        ICoachingSessionRepository coachingSessionRepository,
        IMapper mapper)
    {
        _coachingSessionRepository = coachingSessionRepository;
        _mapper = mapper;
    }

    public async Task<CoachingHistoryDto> Handle(GetCoachingHistoryCommand request, CancellationToken cancellationToken)
    {
        var sessions = await _coachingSessionRepository.GetSessionsByUserAsync(request.UserId, cancellationToken);

        var dto = new CoachingHistoryDto
        {
            Sessions = _mapper.Map<List<CoachingSessionDto>>(sessions),
            TotalSessions = sessions.Count,
            ActiveSessions = sessions.Count(s => s.IsActive),
            CompletedSessions = sessions.Count(s => !s.IsActive),
            AverageProgressPercentage = sessions.Any() ? sessions.Average(s => s.ProgressPercentage) : 0m,
            FirstSessionDate = sessions.OrderBy(s => s.StartDate).FirstOrDefault()?.StartDate,
            LastSessionDate = sessions.OrderByDescending(s => s.StartDate).FirstOrDefault()?.StartDate
        };

        return dto;
    }
}