using Prospera.Domain.Common;

namespace Prospera.Domain.Entities;

/// <summary>
/// Represents a coaching session - a personalized financial coaching interaction
/// designed to guide users toward their financial goals with accountability and progress tracking
/// </summary>
public class CoachingSession : BaseEntity
{
    public Guid UserId { get; set; }
    public string SessionTitle { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public string Assessment { get; set; } = string.Empty;
    public string CoachingMessage { get; set; } = string.Empty;
    public List<CoachingActionItem> ActionItems { get; set; } = new();
    public List<CoachingMilestone> Milestones { get; set; } = new();
    public decimal ProgressPercentage { get; set; } = 0m;
    public bool IsActive { get; set; } = true;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedDate { get; set; }
    public string? ModelUsed { get; set; }
    public string? Provider { get; set; }

    /// <summary>
    /// Coaching methodology/framework used (e.g., "SMART Goals", "Behavioral Coaching", "Habit Formation")
    /// </summary>
    public string CoachingMethodology { get; set; } = "Holistic Financial Coaching";

    public CoachingSession()
    {
    }

    public CoachingSession(Guid userId, string goal)
    {
        UserId = userId;
        Goal = goal;
        SessionTitle = $"Coaching Session - {DateTime.UtcNow:MMMM d, yyyy}";
    }

    public void AddActionItem(string title, string description, int priorityLevel = 1)
    {
        ActionItems.Add(new CoachingActionItem
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            PriorityLevel = priorityLevel,
            CreatedAt = DateTime.UtcNow,
            IsCompleted = false
        });
    }

    public void AddMilestone(string name, decimal targetPercentage, string description = "")
    {
        Milestones.Add(new CoachingMilestone
        {
            Id = Guid.NewGuid(),
            Name = name,
            TargetProgressPercentage = targetPercentage,
            Description = description,
            CreatedAt = DateTime.UtcNow
        });
    }

    public void CompleteActionItem(Guid actionItemId)
    {
        var item = ActionItems.FirstOrDefault(a => a.Id == actionItemId);
        if (item != null)
        {
            item.IsCompleted = true;
            item.CompletedAt = DateTime.UtcNow;
            UpdateProgressPercentage();
        }
    }

    public void UpdateProgressPercentage()
    {
        if (!ActionItems.Any())
            return;

        var completedCount = ActionItems.Count(a => a.IsCompleted);
        ProgressPercentage = (decimal)completedCount / ActionItems.Count * 100;

        if (ProgressPercentage >= 100)
        {
            CompletedDate = DateTime.UtcNow;
        }
    }

    public void MarkAsCompleted()
    {
        IsActive = false;
        CompletedDate = DateTime.UtcNow;
        ProgressPercentage = 100;
    }
}

/// <summary>
/// Action item assigned during a coaching session - specific, measurable steps
/// </summary>
public class CoachingActionItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PriorityLevel { get; set; } = 1; // 1 = High, 2 = Medium, 3 = Low
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ImplementationTips { get; set; }
}

/// <summary>
/// Milestone within a coaching session - longer-term targets
/// </summary>
public class CoachingMilestone
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TargetProgressPercentage { get; set; }
    public bool IsAchieved { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AchievedAt { get; set; }
}
