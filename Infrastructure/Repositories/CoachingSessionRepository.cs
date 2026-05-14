using Prospera.Domain.Entities;
using Prospera.Application.Common.Interfaces;
using Prospera.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Prospera.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for coaching session persistence
/// </summary>
public class CoachingSessionRepository : ICoachingSessionRepository
{
    private readonly ApplicationDbContext _context;

    public CoachingSessionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CoachingSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.CoachingSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<CoachingSession>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CoachingSessions
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<CoachingSession> AddAsync(CoachingSession entity, CancellationToken cancellationToken = default)
    {
        await _context.CoachingSessions.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(CoachingSession entity, CancellationToken cancellationToken = default)
    {
        _context.CoachingSessions.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(CoachingSession entity, CancellationToken cancellationToken = default)
    {
        _context.CoachingSessions.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<CoachingSession?> GetByIdWithItemsAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.CoachingSessions
            .AsNoTracking()
            .Include(c => c.ActionItems)
            .Include(c => c.Milestones)
            .FirstOrDefaultAsync(c => c.Id == sessionId, cancellationToken);
    }

    public async Task<List<CoachingSession>> GetActiveSessionsByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.CoachingSessions
            .AsNoTracking()
            .Where(c => c.UserId == userId && c.IsActive)
            .Include(c => c.ActionItems)
            .Include(c => c.Milestones)
            .OrderByDescending(c => c.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CoachingSession>> GetSessionsByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.CoachingSessions
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .Include(c => c.ActionItems)
            .Include(c => c.Milestones)
            .OrderByDescending(c => c.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<CoachingSession?> GetLatestSessionByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.CoachingSessions
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .Include(c => c.ActionItems)
            .Include(c => c.Milestones)
            .OrderByDescending(c => c.StartDate)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
