

using Prospera.Domain.Entities;

namespace Prospera.Domain.Interfaces;

public interface ILiabilityRepository
{
    Task<IEnumerable<Liability>> GetByUserIdAsync(Guid userId);
    Task AddAsync(Liability liability);
    Task UpdateAsync(Liability liability);
    Task DeleteAsync(Guid id);
}