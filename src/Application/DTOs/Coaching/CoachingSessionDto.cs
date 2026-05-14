namespace Prospera.Application.DTOs.Coaching;

/// <summary>
/// Request to start a new coaching session
/// </summary>
public class StartCoachingSessionRequest
{
    /// <summary>
    /// User's current financial situation and context
    /// </summary>
    public string CurrentSituation { get; set; } = string.Empty;

    /// <summary>
    /// User's primary financial goal/challenge
    /// </summary>
    public string Goal { get; set; } = string.Empty;

    /// <summary>
    /// Optional: User's preferences or constraints
    /// </summary>
    public string? Preferences { get; set; }

    /// <summary>
    /// Optional: LLM provider selection (0 = Ollama, 1 = OpenRouter)
    /// </summary>
    public int Provider { get; set; } = 0;

    /// <summary>
    /// Optional: Specific model name
    /// </summary>
    public string? ModelName { get; set; }
}

/// <summary>
/// Coaching action item DTO
/// </summary>
public class ActionItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PriorityLevel { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ImplementationTips { get; set; }
}

/// <summary>
/// Coaching milestone DTO
/// </summary>
public class MilestoneDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TargetProgressPercentage { get; set; }
    public bool IsAchieved { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AchievedAt { get; set; }
}

/// <summary>
/// Complete coaching session response - what a real coach would provide
/// </summary>
public class CoachingSessionDto
{
    /// <summary>
    /// Unique session ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Session title/date
    /// </summary>
    public string SessionTitle { get; set; } = string.Empty;

    /// <summary>
    /// The goal being coached toward
    /// </summary>
    public string Goal { get; set; } = string.Empty;

    /// <summary>
    /// Initial assessment of the situation - coach's analysis
    /// </summary>
    public string Assessment { get; set; } = string.Empty;

    /// <summary>
    /// Personalized coaching message - motivational and actionable
    /// </summary>
    public string CoachingMessage { get; set; } = string.Empty;

    /// <summary>
    /// Specific action items to work on
    /// </summary>
    public List<ActionItemDto> ActionItems { get; set; } = new();

    /// <summary>
    /// Longer-term milestones to track
    /// </summary>
    public List<MilestoneDto> Milestones { get; set; } = new();

    /// <summary>
    /// Progress toward the goal (0-100%)
    /// </summary>
    public decimal ProgressPercentage { get; set; }

    /// <summary>
    /// Coaching methodology/framework used
    /// </summary>
    public string CoachingMethodology { get; set; } = string.Empty;

    /// <summary>
    /// Whether the session is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Session start date
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Model used for coaching
    /// </summary>
    public string? ModelUsed { get; set; }

    /// <summary>
    /// Provider used
    /// </summary>
    public string? Provider { get; set; }
}

/// <summary>
/// Progress update for a coaching session
/// </summary>
public class CoachingProgressDto
{
    public Guid SessionId { get; set; }
    public string SessionTitle { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public decimal ProgressPercentage { get; set; }
    public int TotalActionItems { get; set; }
    public int CompletedActionItems { get; set; }
    public List<ActionItemDto> PendingActionItems { get; set; } = new();
    public List<ActionItemDto> CompletedItems { get; set; } = new();
    public List<MilestoneDto> Milestones { get; set; } = new();
    public string NextRecommendation { get; set; } = string.Empty;
}

/// <summary>
/// History of coaching sessions
/// </summary>
public class CoachingHistoryDto
{
    public List<CoachingSessionDto> Sessions { get; set; } = new();
    public int TotalSessions { get; set; }
    public int ActiveSessions { get; set; }
    public int CompletedSessions { get; set; }
    public decimal AverageProgressPercentage { get; set; }
    public DateTime? FirstSessionDate { get; set; }
    public DateTime? LastSessionDate { get; set; }
}

/// <summary>
/// Request to update action item completion
/// </summary>
public class CompleteActionItemRequest
{
    public Guid ActionItemId { get; set; }
    public string? Notes { get; set; }
}
