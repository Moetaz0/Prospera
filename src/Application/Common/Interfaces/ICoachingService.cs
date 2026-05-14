namespace Prospera.Application.Common.Interfaces;

/// <summary>
/// Service for generating personalized financial coaching advice
/// Acts as a real financial coach providing guidance, motivation, and action plans
/// </summary>
public interface ICoachingService
{
    /// <summary>
    /// Generate coaching advice for an asset
    /// Provides personalized guidance based on asset type, value, and portfolio context
    /// </summary>
    Task<Application.DTOs.CoachingAdviceDto> GenerateAssetCoachingAdviceAsync(
        Guid assetId,
        string assetName,
        string assetType,
        decimal currentValue,
        decimal projectedValue,
        string? projectionSummary,
        decimal riskTolerance,
        decimal portfolioAllocationPercentage,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate coaching advice for a liability
    /// Provides repayment strategies, prioritization advice, and motivational guidance
    /// </summary>
    Task<Application.DTOs.CoachingAdviceDto> GenerateLiabilityCoachingAdviceAsync(
        Guid liabilityId,
        string liabilityName,
        string liabilityType,
        decimal currentAmount,
        decimal? interestRate,
        decimal? monthlyPayment,
        decimal netWorth,
        decimal totalLiabilities,
        decimal debtToIncomeRatio,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate holistic coaching prompt for AI recommendation system
    /// Incorporates coaching philosophy into investment recommendations
    /// </summary>
    string GenerateCoachingPromptForRecommendations(
        decimal netWorth,
        decimal totalAssets,
        decimal totalLiabilities,
        int assetCount,
        int liabilityCount,
        decimal averageCoachingScore,
        string riskProfile);
}
