using Prospera.Domain.Common;

namespace Prospera.Domain.Entities;

/// <summary>
/// Real estate property valuation history
/// Tracks appreciation rates and forecasts for residential/commercial properties
/// </summary>
public class RealEstateEvaluation : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid AssetId { get; private set; }
    public string Location { get; private set; } = string.Empty; // e.g., "Tunis", "Sfax"
    public string CountryCode { get; private set; } = string.Empty; // ISO 3166-1: "TN", "FR", "US"
    public decimal PurchasePrice { get; private set; }
    public int PurchaseYear { get; private set; }
    public decimal CurrentEstimatedValue { get; private set; }
    public decimal AnnualGrowthRate { get; private set; } // from Financial API
    public decimal TotalAppreciationPct { get; private set; } // cumulative appreciation %
    public decimal Forecast5YearValue { get; private set; }
    public decimal Forecast10YearValue { get; private set; }
    public DateTime EvaluatedAt { get; private set; }
    public string Source { get; private set; } = "Financial API"; // data source attribution

    public RealEstateEvaluation(
        Guid userId,
        Guid assetId,
        string location,
        string countryCode,
        decimal purchasePrice,
        int purchaseYear,
        decimal currentEstimatedValue,
        decimal annualGrowthRate,
        decimal totalAppreciationPct,
        decimal forecast5YearValue,
        decimal forecast10YearValue,
        string source = "Financial API")
    {
        UserId = userId;
        AssetId = assetId;
        Location = location;
        CountryCode = countryCode;
        PurchasePrice = purchasePrice;
        PurchaseYear = purchaseYear;
        CurrentEstimatedValue = currentEstimatedValue;
        AnnualGrowthRate = annualGrowthRate;
        TotalAppreciationPct = totalAppreciationPct;
        Forecast5YearValue = forecast5YearValue;
        Forecast10YearValue = forecast10YearValue;
        EvaluatedAt = DateTime.UtcNow;
        Source = source;
    }

    public void UpdateCurrentValue(decimal newValue)
    {
        CurrentEstimatedValue = newValue;
    }
}
