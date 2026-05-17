using Prospera.Domain.Entities;

namespace Prospera.Domain.Interfaces;

public interface IInvestmentRecommendationRepository
{
    Task<InvestmentRecommendation?> GetByIdAsync(Guid id);
    Task<IEnumerable<InvestmentRecommendation>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<InvestmentRecommendation>> GetBySessionIdAsync(Guid sessionId);
    Task AddAsync(InvestmentRecommendation recommendation);
}
