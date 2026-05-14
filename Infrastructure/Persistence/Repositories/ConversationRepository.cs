using Microsoft.EntityFrameworkCore;
using Prospera.Domain.Entities;
using Prospera.Domain.Interfaces;

namespace Prospera.Infrastructure.Persistence.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly ApplicationDbContext _context;

    public ConversationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Conversation?> GetByIdAsync(Guid conversationId, Guid userId)
    {
        return await _context.Conversations
            .Where(c => c.Id == conversationId && c.UserId == userId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Conversation>> GetUserConversationsAsync(Guid userId, string? topic = null)
    {
        var query = _context.Conversations
            .Where(c => c.UserId == userId);

        if (!string.IsNullOrEmpty(topic))
        {
            query = query.Where(c => c.Topic == topic);
        }

        return await query
            .OrderByDescending(c => c.LastMessageAt ?? c.StartedAt)
            .ToListAsync();
    }

    public async Task<List<Conversation>> GetActiveConversationsAsync(Guid userId)
    {
        return await _context.Conversations
            .Where(c => c.UserId == userId && c.IsActive)
            .OrderByDescending(c => c.LastMessageAt ?? c.StartedAt)
            .ToListAsync();
    }

    public async Task<Conversation?> GetMostRecentAsync(Guid userId)
    {
        return await _context.Conversations
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.LastMessageAt ?? c.StartedAt)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(Conversation conversation)
    {
        await _context.Conversations.AddAsync(conversation);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Conversation conversation)
    {
        _context.Conversations.Update(conversation);
        await _context.SaveChangesAsync();
    }

    public async Task CloseAsync(Guid conversationId)
    {
        var conversation = await _context.Conversations.FindAsync(conversationId);
        if (conversation != null)
        {
            conversation.Close();
            _context.Conversations.Update(conversation);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Conversation>> SearchAsync(Guid userId, string searchQuery)
    {
        var lowerQuery = searchQuery.ToLower();

        return await _context.Conversations
            .Where(c => c.UserId == userId && (
                c.Title.ToLower().Contains(lowerQuery) ||
                c.Topic.ToLower().Contains(lowerQuery) ||
                c.Summary.ToLower().Contains(lowerQuery) ||
                c.Messages.Any(m => m.Content.ToLower().Contains(lowerQuery))
            ))
            .OrderByDescending(c => c.LastMessageAt ?? c.StartedAt)
            .ToListAsync();
    }
}
