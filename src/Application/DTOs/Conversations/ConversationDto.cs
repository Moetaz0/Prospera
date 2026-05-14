namespace Prospera.Application.DTOs.Conversations;

/// <summary>
/// DTO for a conversation session
/// </summary>
public class ConversationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public List<ConversationMessageDto> Messages { get; set; } = new();
    public int MessageCount { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public bool IsActive { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<Guid> RecommendationIds { get; set; } = new();
}

/// <summary>
/// DTO for a single message in a conversation
/// </summary>
public class ConversationMessageDto
{
    public Guid Id { get; set; }
    public string SenderType { get; set; } = string.Empty; // "User" or "AI"
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? MessageType { get; set; }
}

/// <summary>
/// Response when asking a question
/// </summary>
public class ChatbotResponseDto
{
    public Guid ConversationId { get; set; }
    public Guid MessageId { get; set; }
    public string Response { get; set; } = string.Empty;
    public string SenderType { get; set; } = "AI";
    public DateTime CreatedAt { get; set; }
    public string MessageType { get; set; } = "Response";
    public bool IsRecommendation { get; set; } = false;
    public Guid? RecommendationId { get; set; } // If this response included a recommendation
}

/// <summary>
/// Conversation history response
/// </summary>
public class ConversationHistoryDto
{
    public Guid ConversationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public List<ConversationMessageDto> Messages { get; set; } = new();
    public DateTime StartedAt { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int TotalMessages { get; set; }
}
