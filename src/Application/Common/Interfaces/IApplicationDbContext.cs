using Prospera.Domain.Entities;
using Prospera.Domain.Interfaces;

namespace Prospera.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    IQueryable<User> Users { get; }
    IQueryable<Transaction> Transactions { get; }
    IQueryable<Asset> Assets { get; }
    IQueryable<Liability> Liabilities { get; }
    IQueryable<InvestmentRecommendation> InvestmentRecommendations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}