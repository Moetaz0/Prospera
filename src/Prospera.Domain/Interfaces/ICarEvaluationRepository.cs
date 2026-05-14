using Prospera.Domain.Entities;

namespace Prospera.Domain.Interfaces;

public interface ICarEvaluationRepository
{
    Task<IEnumerable<CarEvaluation>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<CarEvaluation>> GetByAssetIdAsync(Guid assetId, Guid userId);
    Task<CarEvaluation?> GetLatestByAssetIdAsync(Guid assetId, Guid userId);
    Task AddAsync(CarEvaluation evaluation);
}
