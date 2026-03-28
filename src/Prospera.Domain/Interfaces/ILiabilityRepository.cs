

using Prospera.Domain.Entities;

namespace Prospera.Domain.Interfaces;

public interface ILiabilityRepository
{
    Task<Liability?> GetByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<Liability>> GetByUserIdAsync(Guid userId);
    Task AddAsync(Liability liability);
    Task UpdateAsync(Liability liability);
    Task DeleteAsync(Guid id);
}