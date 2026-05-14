namespace Prospera.Application.DTOs;

public class FinancialMetricsDto
{
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal NetWorth { get; set; }
    public decimal SavingsRate { get; set; }
    public decimal LiquidityRatio { get; set; }
    public decimal DebtRatio { get; set; }
}
