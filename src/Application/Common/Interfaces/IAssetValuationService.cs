namespace Prospera.Application.Common.Interfaces;

using Prospera.Application.DTOs;

/// <summary>
/// Service for calculating asset valuations including projections
/// </summary>
public interface IAssetValuationService
{
    /// <summary>
    /// Calculate projected valuation for an asset 1 year ahead
    /// Includes:
    /// - Inflation adjustment (purchasing power)
    /// - Car amortization/depreciation
    /// - Real estate appreciation
    /// </summary>
    /// <param name="assetName">Name of the asset</param>
    /// <param name="assetType">Type of asset</param>
    /// <param name="currentValue">Current value in USD</param>
    /// <param name="inflationRate">Annual inflation rate (% e.g., 3.5 for 3.5%)</param>
    /// <param name="carDepreciationRate">Car depreciation rate (% per year, default 15%)</param>
    /// <param name="realEstateAppreciationRate">Real estate appreciation rate (% per year, default 3%)</param>
    /// <returns>Asset valuation with projections</returns>
    AssetValuationDto CalculateProjectedValuation(
        Guid assetId,
        string assetName,
        string assetType,
        decimal currentValue,
        decimal? inflationRate = null,
        decimal? carDepreciationRate = null,
        decimal? realEstateAppreciationRate = null);

    /// <summary>
    /// Get current inflation rate (can be fetched from external service)
    /// </summary>
    Task<decimal> GetCurrentInflationRateAsync();

    /// <summary>
    /// Calculate car depreciation for multiple years
    /// </summary>
    decimal CalculateCarValue(decimal currentValue, int yearsAhead, decimal depreciationRate);

    /// <summary>
    /// Calculate real estate appreciation for multiple years
    /// </summary>
    decimal CalculateRealEstateValue(decimal currentValue, int yearsAhead, decimal appreciationRate);

    /// <summary>
    /// Calculate inflation-adjusted value (purchasing power)
    /// </summary>
    decimal CalculateInflationAdjustedValue(decimal currentValue, int yearsAhead, decimal inflationRate);
}
