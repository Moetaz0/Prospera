using Prospera.Domain.Entities;

namespace Prospera.Domain.Interfaces;

public interface IRealEstateEvaluationRepository
{
    Task<IEnumerable<RealEstateEvaluation>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<RealEstateEvaluation>> GetByAssetIdAsync(Guid assetId, Guid userId);
    Task<RealEstateEvaluation?> GetLatestByAssetIdAsync(Guid assetId, Guid userId);
    Task AddAsync(RealEstateEvaluation evaluation);
}
