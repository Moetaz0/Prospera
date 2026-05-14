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
    IQueryable<CoachingSession> CoachingSessions { get; }
    IQueryable<RealEstateEvaluation> RealEstateEvaluations { get; }
    IQueryable<CarEvaluation> CarEvaluations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}