using Prospera.Domain.Entities;

namespace Prospera.Domain.Interfaces;

public interface ITransactionRepository
{
    Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId);
    Task AddAsync(Transaction transaction);
}
