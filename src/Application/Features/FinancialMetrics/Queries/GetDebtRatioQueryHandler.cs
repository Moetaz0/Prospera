using MediatR;
using Prospera.Application.Common.Interfaces;

namespace Prospera.Application.Features.FinancialMetrics.Queries;

/// <summary>
/// Handler for GetDebtRatioQuery - calculates debt ratio
/// </summary>
public class GetDebtRatioQueryHandler : IRequestHandler<GetDebtRatioQuery, decimal>
{
    private readonly IApplicationDbContext _dbContext;

    public GetDebtRatioQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<decimal> Handle(GetDebtRatioQuery request, CancellationToken cancellationToken)
    {
        // Get user's assets and liabilities
        var assets = _dbContext.Assets.Where(a => a.UserId == request.UserId).ToList();
        var liabilities = _dbContext.Liabilities.Where(l => l.UserId == request.UserId).ToList();

        // Calculate totals
        var totalAssets = assets.Sum(a => a.CurrentValue);
        var totalLiabilities = liabilities.Sum(l => l.Amount);

        // Debt Ratio: Total Liabilities / Total Assets
        var debtRatio = totalAssets > 0 ? totalLiabilities / totalAssets : 0m;

        return Math.Round(debtRatio, 2);
    }
}
