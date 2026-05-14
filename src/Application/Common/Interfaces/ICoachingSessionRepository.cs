using Prospera.Domain.Entities;
using Prospera.Domain.Interfaces;

namespace Prospera.Application.Common.Interfaces;

/// <summary>
/// Repository interface for coaching session persistence
/// </summary>
public interface ICoachingSessionRepository : IRepository<CoachingSession>
{
    /// <summary>
    /// Get active coaching sessions for a user
    /// </summary>
    Task<List<CoachingSession>> GetActiveSessionsByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all coaching sessions for a user (active and completed)
    /// </summary>
    Task<List<CoachingSession>> GetSessionsByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific coaching session by ID
    /// </summary>
    Task<CoachingSession?> GetByIdWithItemsAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get latest coaching session for a user
    /// </summary>
    Task<CoachingSession?> GetLatestSessionByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
