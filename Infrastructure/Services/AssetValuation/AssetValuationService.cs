namespace Prospera.Infrastructure.Services.AssetValuation;

using Microsoft.Extensions.Logging;
using Prospera.Application.Common.Interfaces;
using Prospera.Application.DTOs;

/// <summary>
/// Service for calculating asset valuations and future projections
/// Handles:
/// - Inflation adjustment (purchasing power over time)
/// - Car amortization/depreciation (15% annual default)
/// - Real estate appreciation (3% annual default)
/// </summary>
public class AssetValuationService : IAssetValuationService
{
    private readonly ILogger<AssetValuationService> _logger;

    // Default rates
    private const decimal DEFAULT_INFLATION_RATE = 3.5m; // 3.5% annual inflation
    private const decimal DEFAULT_CAR_DEPRECIATION_RATE = 15m; // 15% annual depreciation
    private const decimal DEFAULT_REAL_ESTATE_APPRECIATION_RATE = 3m; // 3% annual appreciation

    public AssetValuationService(ILogger<AssetValuationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Calculate projected valuation for an asset 1 year ahead
    /// </summary>
    public AssetValuationDto CalculateProjectedValuation(
        Guid assetId,
        string assetName,
        string assetType,
        decimal currentValue,
        decimal? inflationRate = null,
        decimal? carDepreciationRate = null,
        decimal? realEstateAppreciationRate = null)
    {
        var inflation = inflationRate ?? DEFAULT_INFLATION_RATE;
        var carDepreciation = carDepreciationRate ?? DEFAULT_CAR_DEPRECIATION_RATE;
        var realEstateAppreciation = realEstateAppreciationRate ?? DEFAULT_REAL_ESTATE_APPRECIATION_RATE;

        // Calculate inflation-adjusted value (purchasing power)
        var inflationAdjustedValue = CalculateInflationAdjustedValue(currentValue, 1, inflation);
        var inflationLoss = currentValue - inflationAdjustedValue;

        var valuation = new AssetValuationDto
        {
            Id = assetId,
            Name = assetName,
            Type = assetType,
            CurrentValue = currentValue,
            ProjectedValueAfterInflation = inflationAdjustedValue,
            InflationLoss = inflationLoss,
            CalculatedAt = DateTime.UtcNow
        };

        // Type-specific calculations
        if (string.Equals(assetType, "Car", StringComparison.OrdinalIgnoreCase))
        {
            CalculateCarValuation(valuation, currentValue, carDepreciation, inflation, inflationAdjustedValue, assetId);
        }
        else if (string.Equals(assetType, "RealEstate", StringComparison.OrdinalIgnoreCase))
        {
            CalculateRealEstateValuation(valuation, currentValue, realEstateAppreciation, inflation, inflationAdjustedValue, assetId);
        }
        else
        {
            CalculateOtherAssetValuation(valuation, assetType, currentValue, inflation, inflationLoss);
        }

        // Calculate growth metrics
        CalculateGrowthMetrics(valuation, inflation);

        // Calculate multi-year projections
        CalculateMultiYearProjections(valuation, assetType, carDepreciation, realEstateAppreciation, inflation);

        // Generate insights and recommendations
        GenerateInsights(valuation, assetType, inflation);

        // Calculate performance score
        CalculatePerformanceScore(valuation, assetType);

        return valuation;
    }

    private void CalculateCarValuation(AssetValuationDto valuation, decimal currentValue, decimal carDepreciation, 
        decimal inflation, decimal inflationAdjustedValue, Guid assetId)
    {
        var carValue = CalculateCarValue(currentValue, 1, carDepreciation);
        valuation.ProjectedValueCar = carValue;
        valuation.CarDepreciation = currentValue - carValue;
        valuation.CarDepreciationRate = carDepreciation;
        valuation.ProjectionSummary = 
            $"Your car worth ${currentValue:F2} will depreciate to ${carValue:F2} in 1 year " +
            $"({carDepreciation:F1}% annual depreciation). Additionally, due to {inflation:F1}% inflation, " +
            $"the purchasing power of ${currentValue:F2} will be ${inflationAdjustedValue:F2}.";

        valuation.RiskLevel = "High";
        valuation.WealthBuildingPotential = "🔴 Poor - Depreciating Asset";

        _logger.LogInformation(
            "Car valuation calculated: ID={AssetId}, Current=${Current:F2}, Projected=${Projected:F2}, Depreciation={Rate:F1}%",
            assetId, currentValue, carValue, carDepreciation);
    }

    private void CalculateRealEstateValuation(AssetValuationDto valuation, decimal currentValue, decimal realEstateAppreciation, 
        decimal inflation, decimal inflationAdjustedValue, Guid assetId)
    {
        var realEstateValue = CalculateRealEstateValue(currentValue, 1, realEstateAppreciation);
        valuation.ProjectedValueRealEstate = realEstateValue;
        valuation.RealEstateAppreciation = realEstateValue - currentValue;
        valuation.RealEstateAppreciationRate = realEstateAppreciation;
        valuation.ProjectionSummary = 
            $"Your real estate worth ${currentValue:F2} is projected to appreciate to ${realEstateValue:F2} in 1 year " +
            $"({realEstateAppreciation:F1}% annual appreciation). " +
            $"This growth outpaces inflation ({inflation:F1}%), providing real wealth building.";

        var beatInflation = realEstateAppreciation > inflation;
        valuation.RiskLevel = "Low to Medium";
        valuation.WealthBuildingPotential = beatInflation 
            ? "🟢 Excellent - Wealth Builder" 
            : "🟡 Good - Moderate Growth";

        _logger.LogInformation(
            "Real estate valuation calculated: ID={AssetId}, Current=${Current:F2}, Projected=${Projected:F2}, Appreciation={Rate:F1}%",
            assetId, currentValue, realEstateValue, realEstateAppreciation);
    }

    private void CalculateOtherAssetValuation(AssetValuationDto valuation, string assetType, decimal currentValue, 
        decimal inflation, decimal inflationLoss)
    {
        valuation.ProjectionSummary = 
            $"Your {assetType} asset worth ${currentValue:F2} will have a purchasing power of ${valuation.ProjectedValueAfterInflation:F2} in 1 year " +
            $"due to {inflation:F1}% inflation (loss: ${inflationLoss:F2}).";

        // Determine risk level and wealth potential by asset type
        valuation.RiskLevel = assetType switch
        {
            "Cash" => "Low",
            "Stock" => "High",
            "Bond" => "Medium",
            "Crypto" => "Very High",
            _ => "Medium"
        };

        valuation.WealthBuildingPotential = assetType switch
        {
            "Cash" => "🔴 Poor - Losing Purchasing Power",
            "Stock" => "🟢 Good - Growth Potential",
            "Bond" => "🟡 Moderate - Stable Returns",
            "Crypto" => "🔵 High Risk, High Reward",
            _ => "🟡 Moderate"
        };
    }

    private void CalculateGrowthMetrics(AssetValuationDto valuation, decimal inflationRate)
    {
        var oneYearGrowthPercent = ((valuation.ProjectedValueAfterInflation - valuation.CurrentValue) / valuation.CurrentValue) * 100;
        var realReturn = valuation.CurrentValue > 0 
            ? ((valuation.ProjectedValueAfterInflation - valuation.InflationLoss - valuation.CurrentValue) / valuation.CurrentValue) * 100
            : 0;
        var beatsInflation = oneYearGrowthPercent > inflationRate;

        valuation.GrowthMetrics = new GrowthMetricsDto
        {
            OneYearGrowthPercent = Math.Round(oneYearGrowthPercent, 2),
            RealReturnPercent = Math.Round(realReturn, 2),
            BeatsInflation = beatsInflation,
            GrowthAmount = Math.Round(valuation.ProjectedValueAfterInflation - valuation.CurrentValue, 2),
            CompoundGrowthRate = Math.Round(oneYearGrowthPercent, 2)
        };

        // Calculate comparison metrics
        var performanceVsInflation = oneYearGrowthPercent - inflationRate;
        var maintainsPurchasingPower = valuation.ProjectedValueAfterInflation >= valuation.CurrentValue;
        var trend = performanceVsInflation > 0 
            ? "🟢 Beating Inflation" 
            : performanceVsInflation < -5 
                ? "🔴 Losing Value Fast" 
                : "🟡 Keeping Pace with Inflation";

        valuation.ComparisonMetrics = new ComparisonMetricsDto
        {
            InflationRate = inflationRate,
            PerformanceVsInflation = Math.Round(performanceVsInflation, 2),
            MaintainsPurchasingPower = maintainsPurchasingPower,
            Trend = trend
        };
    }

    private void CalculateMultiYearProjections(AssetValuationDto valuation, string assetType, 
        decimal carDepreciation, decimal realEstateAppreciation, decimal inflation)
    {
        var multiYearProjection = new MultiYearProjectionDto();

        if (string.Equals(assetType, "Car", StringComparison.OrdinalIgnoreCase))
        {
            multiYearProjection.Year5 = CreateProjectionYear(5, valuation.CurrentValue, 
                (y) => CalculateCarValue(valuation.CurrentValue, y, carDepreciation), inflation);
            multiYearProjection.Year10 = CreateProjectionYear(10, valuation.CurrentValue, 
                (y) => CalculateCarValue(valuation.CurrentValue, y, carDepreciation), inflation);
            multiYearProjection.Year20 = CreateProjectionYear(20, valuation.CurrentValue, 
                (y) => CalculateCarValue(valuation.CurrentValue, y, carDepreciation), inflation);
        }
        else if (string.Equals(assetType, "RealEstate", StringComparison.OrdinalIgnoreCase))
        {
            multiYearProjection.Year5 = CreateProjectionYear(5, valuation.CurrentValue, 
                (y) => CalculateRealEstateValue(valuation.CurrentValue, y, realEstateAppreciation), inflation);
            multiYearProjection.Year10 = CreateProjectionYear(10, valuation.CurrentValue, 
                (y) => CalculateRealEstateValue(valuation.CurrentValue, y, realEstateAppreciation), inflation);
            multiYearProjection.Year20 = CreateProjectionYear(20, valuation.CurrentValue, 
                (y) => CalculateRealEstateValue(valuation.CurrentValue, y, realEstateAppreciation), inflation);
        }
        else
        {
            // For other assets, show inflation impact only
            multiYearProjection.Year5 = CreateProjectionYear(5, valuation.CurrentValue, 
                (y) => CalculateInflationAdjustedValue(valuation.CurrentValue, y, inflation), inflation);
            multiYearProjection.Year10 = CreateProjectionYear(10, valuation.CurrentValue, 
                (y) => CalculateInflationAdjustedValue(valuation.CurrentValue, y, inflation), inflation);
            multiYearProjection.Year20 = CreateProjectionYear(20, valuation.CurrentValue, 
                (y) => CalculateInflationAdjustedValue(valuation.CurrentValue, y, inflation), inflation);
        }

        valuation.MultiYearProjection = multiYearProjection;
    }

    private ProjectionYearDto CreateProjectionYear(int year, decimal currentValue, Func<int, decimal> calculator, decimal inflation)
    {
        var projectedValue = calculator(year);
        var totalGain = projectedValue - currentValue;
        var gainPercent = currentValue > 0 ? (totalGain / currentValue) * 100 : 0;
        var inflationAdjusted = CalculateInflationAdjustedValue(currentValue, year, inflation);

        return new ProjectionYearDto
        {
            Year = year,
            ProjectedValue = Math.Round(projectedValue, 2),
            TotalGain = Math.Round(totalGain, 2),
            GainPercentage = Math.Round(gainPercent, 2),
            InflationLoss = Math.Round(currentValue - inflationAdjusted, 2)
        };
    }

    private void GenerateInsights(AssetValuationDto valuation, string assetType, decimal inflationRate)
    {
        var insights = new List<InsightDto>();
        var recommendations = new List<string>();

        if (string.Equals(assetType, "Car", StringComparison.OrdinalIgnoreCase))
        {
            insights.Add(new InsightDto 
            { 
                Type = "Warning", 
                Message = "Cars are depreciating assets that lose value every year.", 
                Impact = "High" 
            });
            insights.Add(new InsightDto 
            { 
                Type = "Information", 
                Message = $"Your car loses approximately {valuation.CarDepreciationRate:F1}% of its value annually.", 
                Impact = "High" 
            });
            recommendations.Add("Consider keeping your car longer to maximize value retention");
            recommendations.Add("Regular maintenance can help slow depreciation");
            recommendations.Add("Invest the savings in appreciating assets like real estate");
        }
        else if (string.Equals(assetType, "RealEstate", StringComparison.OrdinalIgnoreCase))
        {
            var beatsInflation = valuation.RealEstateAppreciationRate > inflationRate;
            insights.Add(new InsightDto 
            { 
                Type = "Opportunity", 
                Message = "Real estate typically appreciates and builds long-term wealth.", 
                Impact = "High" 
            });
            if (beatsInflation)
            {
                insights.Add(new InsightDto 
                { 
                    Type = "Strength", 
                    Message = $"Your property appreciation ({valuation.RealEstateAppreciationRate:F1}%) outpaces inflation ({inflationRate:F1}%), creating real wealth growth.", 
                    Impact = "High" 
                });
                recommendations.Add("Continue holding this valuable asset");
                recommendations.Add("Consider leveraging equity for additional investments");
            }
            else
            {
                recommendations.Add("Monitor property market for optimization opportunities");
            }
            recommendations.Add("Track maintenance costs and property taxes for ROI calculations");
        }
        else if (string.Equals(assetType, "Cash", StringComparison.OrdinalIgnoreCase))
        {
            insights.Add(new InsightDto 
            { 
                Type = "Warning", 
                Message = $"Cash loses purchasing power due to {inflationRate:F1}% annual inflation.", 
                Impact = "High" 
            });
            insights.Add(new InsightDto 
            { 
                Type = "Information", 
                Message = $"Your ${valuation.CurrentValue:F2} will be worth ${valuation.ProjectedValueAfterInflation:F2} in one year.", 
                Impact = "Medium" 
            });
            recommendations.Add("Consider investing excess cash in growth assets");
            recommendations.Add("Keep only necessary cash reserves");
            recommendations.Add("Explore high-yield savings accounts or short-term bonds");
        }
        else if (string.Equals(assetType, "Stock", StringComparison.OrdinalIgnoreCase))
        {
            insights.Add(new InsightDto 
            { 
                Type = "Opportunity", 
                Message = "Stocks have historically provided strong long-term returns.", 
                Impact = "High" 
            });
            recommendations.Add("Consider holding for long-term wealth building");
            recommendations.Add("Diversify across different sectors");
            recommendations.Add("Review your portfolio allocation regularly");
        }
        else if (string.Equals(assetType, "Crypto", StringComparison.OrdinalIgnoreCase))
        {
            insights.Add(new InsightDto 
            { 
                Type = "Warning", 
                Message = "Cryptocurrency is highly volatile and carries significant risk.", 
                Impact = "High" 
            });
            recommendations.Add("Only invest what you can afford to lose");
            recommendations.Add("Diversify your portfolio to reduce risk");
            recommendations.Add("Keep crypto as a small portion of overall wealth");
        }

        valuation.Insights = insights;
        valuation.Recommendations = recommendations;
    }

    private void CalculatePerformanceScore(AssetValuationDto valuation, string assetType)
    {
        var score = 50m; // Base score

        // Adjust based on type
        if (string.Equals(assetType, "RealEstate", StringComparison.OrdinalIgnoreCase))
            score += 25;
        else if (string.Equals(assetType, "Stock", StringComparison.OrdinalIgnoreCase))
            score += 20;
        else if (string.Equals(assetType, "Bond", StringComparison.OrdinalIgnoreCase))
            score += 15;
        else if (string.Equals(assetType, "Car", StringComparison.OrdinalIgnoreCase))
            score -= 25;
        else if (string.Equals(assetType, "Cash", StringComparison.OrdinalIgnoreCase))
            score -= 10;
        else if (string.Equals(assetType, "Crypto", StringComparison.OrdinalIgnoreCase))
            score += 10; // High potential but high risk

        // Adjust based on growth
        if (valuation.GrowthMetrics?.BeatsInflation ?? false)
            score += 15;

        valuation.PerformanceScore = Math.Min(100, Math.Max(0, score));
    }

    /// <summary>
    /// Get current inflation rate
    /// In production, this could fetch from external service or central bank API
    /// </summary>
    public async Task<decimal> GetCurrentInflationRateAsync()
    {
        // TODO: Integrate with external inflation data service (BLS, World Bank, etc.)
        // For now, returning default
        await Task.Delay(0); // Placeholder for async call
        return DEFAULT_INFLATION_RATE;
    }

    /// <summary>
    /// Calculate car value after depreciation
    /// Formula: Value = Current * (1 - DepreciationRate/100)^Years
    /// </summary>
    public decimal CalculateCarValue(decimal currentValue, int yearsAhead, decimal depreciationRate)
    {
        if (currentValue <= 0 || yearsAhead < 0 || depreciationRate < 0 || depreciationRate > 100)
        {
            _logger.LogWarning(
                "Invalid car valuation parameters: Value={Value}, Years={Years}, Rate={Rate}%",
                currentValue, yearsAhead, depreciationRate);
            return currentValue;
        }

        var depreciationFactor = 1m - (depreciationRate / 100m);
        var projectedValue = currentValue * (decimal)Math.Pow((double)depreciationFactor, yearsAhead);

        return Math.Max(0, projectedValue); // Ensure non-negative
    }

    /// <summary>
    /// Calculate real estate value after appreciation
    /// Formula: Value = Current * (1 + AppreciationRate/100)^Years
    /// </summary>
    public decimal CalculateRealEstateValue(decimal currentValue, int yearsAhead, decimal appreciationRate)
    {
        if (currentValue <= 0 || yearsAhead < 0 || appreciationRate < 0)
        {
            _logger.LogWarning(
                "Invalid real estate valuation parameters: Value={Value}, Years={Years}, Rate={Rate}%",
                currentValue, yearsAhead, appreciationRate);
            return currentValue;
        }

        var appreciationFactor = 1m + (appreciationRate / 100m);
        var projectedValue = currentValue * (decimal)Math.Pow((double)appreciationFactor, yearsAhead);

        return projectedValue;
    }

    /// <summary>
    /// Calculate inflation-adjusted value (purchasing power)
    /// Lower value reflects loss of purchasing power
    /// Formula: Value = Current / (1 + InflationRate/100)^Years
    /// </summary>
    public decimal CalculateInflationAdjustedValue(decimal currentValue, int yearsAhead, decimal inflationRate)
    {
        if (currentValue <= 0 || yearsAhead < 0 || inflationRate < 0)
        {
            _logger.LogWarning(
                "Invalid inflation calculation parameters: Value={Value}, Years={Years}, Rate={Rate}%",
                currentValue, yearsAhead, inflationRate);
            return currentValue;
        }

        var inflationFactor = 1m + (inflationRate / 100m);
        var adjustedValue = currentValue / (decimal)Math.Pow((double)inflationFactor, yearsAhead);

        return adjustedValue;
    }
}
