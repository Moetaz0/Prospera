namespace Prospera.Application.Common.Interfaces;

/// <summary>
/// Service for fetching car valuation data from the Python financial-api
/// Provides depreciation schedules and detailed car value projections
/// </summary>
public interface ICarValuationApiService
{
    /// <summary>
    /// Calculate car depreciation and valuation using the Python financial-api
    /// </summary>
    /// <param name="purchasePrice">Purchase price of the car</param>
    /// <param name="purchaseYear">Year the car was purchased</param>
    /// <param name="targetYear">Year to calculate value for (optional, defaults to current year)</param>
    /// <param name="category">Car category (sedan, suv, truck, luxury, economy, electric)</param>
    /// <param name="country">ISO country code for market adjustments</param>
    /// <returns>Car valuation response with depreciation schedule</returns>
    Task<CarValuationResponse?> GetCarValuationAsync(
        decimal purchasePrice,
        int purchaseYear,
        int? targetYear = null,
        string category = "sedan",
        string country = "US");
}

/// <summary>
/// Response from car valuation API
/// </summary>
public class CarValuationResponse
{
    public decimal PurchasePrice { get; set; }
    public int PurchaseYear { get; set; }
    public int TargetYear { get; set; }
    public decimal CurrentMarketValue { get; set; }
    public decimal TotalDepreciationPercent { get; set; }
    public decimal? InflationAdjustedValue { get; set; }
    public string Country { get; set; } = string.Empty;
    public string CarCategory { get; set; } = string.Empty;
    public List<CarYearlyDepreciation> YearlyBreakdown { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// Yearly depreciation data for car valuations
/// </summary>
public class CarYearlyDepreciation
{
    public int Year { get; set; }
    public decimal Value { get; set; }
    public decimal DepreciationRatePercent { get; set; }
    public decimal CumulativeDepreciationPercent { get; set; }
}
