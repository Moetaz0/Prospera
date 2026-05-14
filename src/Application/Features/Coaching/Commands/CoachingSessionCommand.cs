using MediatR;
using Prospera.Application.DTOs.Coaching;

namespace Prospera.Application.Features.Coaching.Commands;

/// <summary>
/// Command to start a coaching session with a user
/// </summary>
public class StartCoachingSessionCommand : IRequest<CoachingSessionDto>
{
    public Guid UserId { get; set; }
    public string CurrentSituation { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public string? Preferences { get; set; }
    public int Provider { get; set; } = 0;
    public string? ModelName { get; set; }
}

/// <summary>
/// Command to update action item completion
/// </summary>
public class CompleteCoachingActionCommand : IRequest<CoachingProgressDto>
{
    public Guid UserId { get; set; }
    public Guid SessionId { get; set; }
    public Guid ActionItemId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Command to get coaching progress
/// </summary>
public class GetCoachingProgressCommand : IRequest<CoachingProgressDto>
{
    public Guid UserId { get; set; }
    public Guid SessionId { get; set; }
}

/// <summary>
/// Command to get coaching history
/// </summary>
public class GetCoachingHistoryCommand : IRequest<CoachingHistoryDto>
{
    public Guid UserId { get; set; }
}
