using MediatR;
using Prospera.Application.Common.Interfaces;

namespace Prospera.Application.Features.FinancialMetrics.Queries;

/// <summary>
/// Handler for GetSavingsRateQuery - calculates savings rate
/// </summary>
public class GetSavingsRateQueryHandler : IRequestHandler<GetSavingsRateQuery, decimal>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSavingsRateQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<decimal> Handle(GetSavingsRateQuery request, CancellationToken cancellationToken)
    {
        // Get user's transactions
        var transactions = _dbContext.Transactions.Where(t => t.UserId == request.UserId).ToList();

        // Savings Rate: (Income - Expense) / Income (last 30 days)
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var recentTransactions = transactions.Where(t => t.Date >= thirtyDaysAgo).ToList();
        var income = recentTransactions.Where(t => t.Type.ToString() == "Income").Sum(t => t.Amount);
        var expenses = recentTransactions.Where(t => t.Type.ToString() == "Expense").Sum(t => t.Amount);
        var savingsRate = income > 0 ? (income - expenses) / income : 0m;

        return Math.Round(savingsRate, 2);
    }
}
