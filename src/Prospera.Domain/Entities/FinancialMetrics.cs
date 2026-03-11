namespace Prospera.Domain.Entities;

public class FinancialMetrics
{
    public decimal SavingsRate { get; private set; }
    public decimal LiquidityRatio { get; private set; }
    public decimal DebtRatio { get; private set; }

    public FinancialMetrics(decimal savingsRate, decimal liquidityRatio, decimal debtRatio)
    {
        SavingsRate = savingsRate;
        LiquidityRatio = liquidityRatio;
        DebtRatio = debtRatio;
    }
}
