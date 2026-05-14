using MediatR;
using Prospera.Application.Common.Interfaces;

namespace Prospera.Application.Features.FinancialMetrics.Queries;

/// <summary>
/// Handler for GetLiquidityRatioQuery - calculates liquidity ratio
/// </summary>
public class GetLiquidityRatioQueryHandler : IRequestHandler<GetLiquidityRatioQuery, decimal>
{
    private readonly IApplicationDbContext _dbContext;

    public GetLiquidityRatioQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<decimal> Handle(GetLiquidityRatioQuery request, CancellationToken cancellationToken)
    {
        // Get user's assets and liabilities
        var assets = _dbContext.Assets.Where(a => a.UserId == request.UserId).ToList();
        var liabilities = _dbContext.Liabilities.Where(l => l.UserId == request.UserId).ToList();

        // Liquidity Ratio: Liquid Assets / Current Liabilities
        var liquidAssets = assets.Where(a => a.Type.ToString() is "Cash").Sum(a => a.CurrentValue);
        var shortTermLiabilities = liabilities.Where(l => l.Type.ToString() is "CreditCard").Sum(l => l.Amount);
        var liquidityRatio = shortTermLiabilities > 0 ? liquidAssets / shortTermLiabilities : 0m;

        return Math.Round(liquidityRatio, 2);
    }
}
