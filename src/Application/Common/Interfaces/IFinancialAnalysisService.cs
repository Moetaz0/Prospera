namespace Prospera.Application.Common.Interfaces;

public interface IFinancialAnalysisService
{
    /// <summary>
    /// Calculates savings rate: (Income - Expenses) / Income * 100
    /// </summary>
    decimal CalculateSavingsRate(decimal income, decimal expenses);

    /// <summary>
    /// Calculates liquidity ratio: Liquid Assets / Liabilities
    /// </summary>
    decimal CalculateLiquidityRatio(decimal liquidAssets, decimal liabilities);

    /// <summary>
    /// Calculates debt ratio: Total Debt / Total Assets * 100
    /// </summary>
    decimal CalculateDebtRatio(decimal totalDebt, decimal totalAssets);

    /// <summary>
    /// Calculates net worth: Total Assets - Total Liabilities
    /// </summary>
    decimal CalculateNetWorth(decimal totalAssets, decimal totalLiabilities);
}
