using Microsoft.EntityFrameworkCore;
using Prospera.Domain.Entities;
using Prospera.Domain.Interfaces;

namespace Prospera.Infrastructure.Persistence.Repositories;

public class RecommendationSessionRepository : IRecommendationSessionRepository
{
    private readonly ApplicationDbContext _context;

    public RecommendationSessionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RecommendationSession>> GetByUserIdAsync(Guid userId)
    {
        return await _context.RecommendationSessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync();
    }

    public async Task<RecommendationSession?> GetByIdAsync(Guid id, Guid userId)
    {
        return await _context.RecommendationSessions
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    public async Task AddAsync(RecommendationSession session)
    {
        await _context.RecommendationSessions.AddAsync(session);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(RecommendationSession session)
    {
        _context.RecommendationSessions.Update(session);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var session = await _context.RecommendationSessions
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (session != null)
        {
            _context.RecommendationSessions.Remove(session);
            await _context.SaveChangesAsync();
        }
    }
}
