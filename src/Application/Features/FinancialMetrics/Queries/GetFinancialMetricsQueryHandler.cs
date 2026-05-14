using MediatR;
using AutoMapper;
using Prospera.Application.Common.Interfaces;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.FinancialMetrics.Queries;

/// <summary>
/// Handler for GetFinancialMetricsQuery - calculates financial metrics
/// </summary>
public class GetFinancialMetricsQueryHandler : IRequestHandler<GetFinancialMetricsQuery, FinancialMetricsDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetFinancialMetricsQueryHandler(IApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<FinancialMetricsDto> Handle(GetFinancialMetricsQuery request, CancellationToken cancellationToken)
    {
        // Get user's assets and liabilities
        var assets = _dbContext.Assets.Where(a => a.UserId == request.UserId).ToList();
        var liabilities = _dbContext.Liabilities.Where(l => l.UserId == request.UserId).ToList();
        var transactions = _dbContext.Transactions.Where(t => t.UserId == request.UserId).ToList();

        // Calculate metrics
        var totalAssets = assets.Sum(a => a.CurrentValue);
        var totalLiabilities = liabilities.Sum(l => l.Amount);
        var netWorth = totalAssets - totalLiabilities;

        // Savings Rate: (Income - Expense) / Income (last 30 days)
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var recentTransactions = transactions.Where(t => t.Date >= thirtyDaysAgo).ToList();
        var income = recentTransactions.Where(t => t.Type.ToString() == "Income").Sum(t => t.Amount);
        var expenses = recentTransactions.Where(t => t.Type.ToString() == "Expense").Sum(t => t.Amount);
        var savingsRate = income > 0 ? (income - expenses) / income : 0m;

        // Liquidity Ratio: Liquid Assets / Current Liabilities
        var liquidAssets = assets.Where(a => a.Type.ToString() is "Cash").Sum(a => a.CurrentValue);
        var shortTermLiabilities = liabilities.Where(l => l.Type.ToString() is "CreditCard").Sum(l => l.Amount);
        var liquidityRatio = shortTermLiabilities > 0 ? liquidAssets / shortTermLiabilities : 0m;

        // Debt Ratio: Total Liabilities / Total Assets
        var debtRatio = totalAssets > 0 ? totalLiabilities / totalAssets : 0m;

        return new FinancialMetricsDto
        {
            TotalAssets = Math.Round(totalAssets, 2),
            TotalLiabilities = Math.Round(totalLiabilities, 2),
            NetWorth = Math.Round(netWorth, 2),
            SavingsRate = Math.Round(savingsRate, 2),
            LiquidityRatio = Math.Round(liquidityRatio, 2),
            DebtRatio = Math.Round(debtRatio, 2)
        };
    }
}
