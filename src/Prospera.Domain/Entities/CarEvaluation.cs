using Prospera.Domain.Common;

namespace Prospera.Domain.Entities;

/// <summary>
/// Car depreciation valuation history
/// Tracks market value depreciation and forecasts based on car type, mileage, and location
/// </summary>
public class CarEvaluation : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid AssetId { get; private set; }
    public string Make { get; private set; } = string.Empty; // e.g., "Toyota", "Volkswagen"
    public string Model { get; private set; } = string.Empty; // e.g., "Corolla", "Golf"
    public string Category { get; private set; } = string.Empty; // e.g., "sedan", "suv", "truck"
    public string CountryCode { get; private set; } = string.Empty; // ISO 3166-1: "TN", "FR", "US"
    public decimal PurchasePrice { get; private set; }
    public int PurchaseYear { get; private set; }
    public int AnnualMileageKm { get; private set; }
    public decimal CurrentMarketValue { get; private set; }
    public decimal TotalDepreciationPct { get; private set; } // cumulative depreciation %
    public decimal AnnualDepreciationRate { get; private set; } // single-year depreciation %
    public decimal InflationAdjustedValue { get; private set; } // accounts for local CPI
    public decimal Forecast5YearValue { get; private set; }
    public DateTime EvaluatedAt { get; private set; }
    public string Source { get; private set; } = "Financial API"; // data source attribution

    public CarEvaluation(
        Guid userId,
        Guid assetId,
        string make,
        string model,
        string category,
        string countryCode,
        decimal purchasePrice,
        int purchaseYear,
        int annualMileageKm,
        decimal currentMarketValue,
        decimal totalDepreciationPct,
        decimal annualDepreciationRate,
        decimal inflationAdjustedValue,
        decimal forecast5YearValue,
        string source = "Financial API")
    {
        UserId = userId;
        AssetId = assetId;
        Make = make;
        Model = model;
        Category = category;
        CountryCode = countryCode;
        PurchasePrice = purchasePrice;
        PurchaseYear = purchaseYear;
        AnnualMileageKm = annualMileageKm;
        CurrentMarketValue = currentMarketValue;
        TotalDepreciationPct = totalDepreciationPct;
        AnnualDepreciationRate = annualDepreciationRate;
        InflationAdjustedValue = inflationAdjustedValue;
        Forecast5YearValue = forecast5YearValue;
        EvaluatedAt = DateTime.UtcNow;
        Source = source;
    }
}
