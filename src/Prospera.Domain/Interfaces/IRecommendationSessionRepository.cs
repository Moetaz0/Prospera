using Prospera.Domain.Entities;

namespace Prospera.Domain.Interfaces;

public interface IRecommendationSessionRepository
{
    Task<IEnumerable<RecommendationSession>> GetByUserIdAsync(Guid userId);
    Task<RecommendationSession?> GetByIdAsync(Guid id, Guid userId);
    Task AddAsync(RecommendationSession session);
    Task UpdateAsync(RecommendationSession session);
    Task DeleteAsync(Guid id, Guid userId);
}
