using Prospera.Domain.Common;

namespace Prospera.Domain.Entities;

/// <summary>
/// Represents a financial conversation session between user and AI advisor
/// Tracks chat history, context, and recommendations within a single conversation thread
/// </summary>
public class Conversation : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Title { get; private set; }
    public string Topic { get; private set; } // "Market", "Portfolio", "Savings", "Investment", etc.
    public List<ConversationMessage> Messages { get; private set; } = new();
    public List<Guid> RecommendationIds { get; private set; } = new(); // Recommendations made in this conversation
    public DateTime StartedAt { get; private set; }
    public DateTime? LastMessageAt { get; private set; }
    public bool IsActive { get; private set; }
    public string Summary { get; private set; } = string.Empty; // Auto-generated summary of conversation
    public string ConversationContext { get; private set; } = string.Empty; // Context for LLM to maintain continuity

    // Parameterless constructor for EF Core
    public Conversation() { }

    public Conversation(Guid userId, string topic, string initialUserMessage)
    {
        UserId = userId;
        Topic = topic;
        Title = GenerateTitle(topic, initialUserMessage);
        IsActive = true;
        StartedAt = DateTime.UtcNow;
        LastMessageAt = DateTime.UtcNow;
    }

    public void AddMessage(ConversationMessage message)
    {
        Messages.Add(message);
        LastMessageAt = DateTime.UtcNow;
        UpdateConversationContext();
    }

    public void AddRecommendation(Guid recommendationId)
    {
        if (!RecommendationIds.Contains(recommendationId))
        {
            RecommendationIds.Add(recommendationId);
        }
    }

    public void Close()
    {
        IsActive = false;
    }

    public void UpdateSummary(string summary)
    {
        Summary = summary;
    }

    private void UpdateConversationContext()
    {
        // Build context from recent messages for LLM continuity
        var recentMessages = Messages.TakeLast(6);
        var context = string.Join("\n", recentMessages.Select(m => $"{m.SenderType}: {m.Content}"));
        ConversationContext = context;
    }

    private string GenerateTitle(string topic, string initialMessage)
    {
        var words = initialMessage.Split(' ').Take(5).ToList();
        var title = string.Join(" ", words).Length > 50
            ? string.Join(" ", words.Take(3)) + "..."
            : string.Join(" ", words);
        return $"{topic} - {title}";
    }
}

/// <summary>
/// Individual message in a conversation
/// </summary>
public class ConversationMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string SenderType { get; set; } = string.Empty; // "User" or "AI"
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int TokenCount { get; set; } = 0; // For tracking API costs
    public string? MessageType { get; set; } // "Question", "Statement", "Recommendation", "Clarification"
    public Guid? RecommendationId { get; set; } // If this message is a recommendation, link to the recommendation ID

    public ConversationMessage() { }

    public ConversationMessage(string senderType, string content, string? messageType = null)
    {
        SenderType = senderType;
        Content = content;
        MessageType = messageType;
        CreatedAt = DateTime.UtcNow;
    }
}
