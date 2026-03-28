using Prospera.Domain.Entities;

namespace Prospera.Domain.Interfaces;

public interface IAssetRepository
{
    Task<Asset?> GetByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<Asset>> GetByUserIdAsync(Guid userId);
    Task AddAsync(Asset asset);
    Task UpdateAsync(Asset asset);
    Task DeleteAsync(Guid id);
}
