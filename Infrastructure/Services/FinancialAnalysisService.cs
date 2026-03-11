namespace Prospera.Infrastructure.Services;

public class FinancialAnalysisService
{
    public decimal CalculateSavingsRate(decimal income, decimal expenses)
    {
        if (income <= 0)
            return 0;

        var savings = income - expenses;
        return (savings / income) * 100;
    }

    public decimal CalculateLiquidityRatio(decimal liquidAssets, decimal liabilities)
    {
        if (liabilities <= 0)
            return 0;

        return liquidAssets / liabilities;
    }

    public decimal CalculateDebtRatio(decimal totalDebt, decimal totalAssets)
    {
        if (totalAssets <= 0)
            return 0;

        return (totalDebt / totalAssets) * 100;
    }

    public decimal CalculateNetWorth(decimal totalAssets, decimal totalLiabilities)
    {
        return totalAssets - totalLiabilities;
    }
}
