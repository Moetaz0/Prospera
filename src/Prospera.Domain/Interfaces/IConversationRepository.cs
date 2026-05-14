using Prospera.Domain.Entities;

namespace Prospera.Domain.Interfaces;

/// <summary>
/// Repository for managing financial conversation sessions
/// </summary>
public interface IConversationRepository
{
    /// <summary>
    /// Get a conversation by ID with full message history
    /// </summary>
    Task<Conversation?> GetByIdAsync(Guid conversationId, Guid userId);

    /// <summary>
    /// Get all conversations for a user, optionally filtered by topic
    /// </summary>
    Task<List<Conversation>> GetUserConversationsAsync(Guid userId, string? topic = null);

    /// <summary>
    /// Get active conversations for a user
    /// </summary>
    Task<List<Conversation>> GetActiveConversationsAsync(Guid userId);

    /// <summary>
    /// Get the most recent conversation for a user
    /// </summary>
    Task<Conversation?> GetMostRecentAsync(Guid userId);

    /// <summary>
    /// Create a new conversation
    /// </summary>
    Task AddAsync(Conversation conversation);

    /// <summary>
    /// Update an existing conversation (add messages, update summary, etc.)
    /// </summary>
    Task UpdateAsync(Conversation conversation);

    /// <summary>
    /// Close a conversation
    /// </summary>
    Task CloseAsync(Guid conversationId);

    /// <summary>
    /// Search conversations by topic or content
    /// </summary>
    Task<List<Conversation>> SearchAsync(Guid userId, string searchQuery);
}
