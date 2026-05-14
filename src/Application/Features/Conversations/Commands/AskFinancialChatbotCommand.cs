using MediatR;
using Prospera.Application.DTOs.Conversations;

namespace Prospera.Application.Features.Conversations.Commands;

/// <summary>
/// Command to ask a general financial question in a conversation context
/// Not a recommendation request - just a chatbot-style question about markets, finance, etc.
/// </summary>
public class AskFinancialChatbotCommand : IRequest<ChatbotResponseDto>
{
    public required Guid UserId { get; set; }
    public Guid? ConversationId { get; set; } // Null = start new conversation
    public required string Question { get; set; }
    public string? Topic { get; set; } // "Market", "Portfolio", "Savings", etc. - inferred if not provided
    public int Provider { get; set; } = 0; // LLM provider
    public string? ModelName { get; set; }
}

/// <summary>
/// Request model for asking chatbot questions
/// </summary>
public class AskChatbotRequest
{
    public string Question { get; set; } = string.Empty;
    public Guid? ConversationId { get; set; } // Continue existing conversation or null for new
    public string? Topic { get; set; } // Optional topic hint
    public int Provider { get; set; } = 0;
    public string? ModelName { get; set; }
}
