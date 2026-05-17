using System.Text.Json.Serialization;

namespace Prospera.Application.Common.Interfaces;

/// <summary>
/// Service for enriching coaching sessions with real-world financial market data
/// Integrates with external FastAPI for inflation, exchange rates, real estate, and predictions
/// </summary>
public interface IFinancialDataService
{
    /// <summary>
    /// Get inflation data for a country
    /// </summary>
    Task<InflationData> GetInflationDataAsync(string countryCode, CancellationToken cancellationToken);

    /// <summary>
    /// Compare inflation across multiple countries
    /// </summary>
    Task<MultiCountryInflationData> GetMultiCountryInflationAsync(List<string> countryCodes, CancellationToken cancellationToken);

    /// <summary>
    /// Get current exchange rates for a base currency
    /// </summary>
    Task<ExchangeRateData> GetCurrentExchangeRatesAsync(string baseCurrency, CancellationToken cancellationToken);

    /// <summary>
    /// Get TND (Tunisian Dinar) specific exchange rates against major currencies
    /// </summary>
    Task<ExchangeRateData> GetTndExchangeRatesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Get historical exchange rate data
    /// </summary>
    Task<ExchangeHistoryData> GetExchangeHistoryAsync(string currencyCode, CancellationToken cancellationToken);

    /// <summary>
    /// Get purchasing power parity index based on CPI
    /// </summary>
    Task<PurchasingPowerData> GetPurchasingPowerAsync(string countryCode, CancellationToken cancellationToken);

    /// <summary>
    /// Get available real estate market locations
    /// </summary>
    Task<List<string>> GetRealEstateLocationsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Get real estate data for a Tunisian location
    /// </summary>
    Task<RealEstateData> GetTunisianRealEstateAsync(string location, CancellationToken cancellationToken);

    /// <summary>
    /// Get real estate data (global context via World Bank proxy)
    /// </summary>
    Task<RealEstateData> GetGlobalRealEstateAsync(string countryCode, CancellationToken cancellationToken);

    /// <summary>
    /// Get car amortization/depreciation schedule
    /// </summary>
    Task<CarDepreciationData> GetCarDepreciationAsync(
        decimal purchasePrice,
        int purchaseYear,
        string category,
        string countryCode,
        int annualMileageKm,
        CancellationToken cancellationToken);

    /// <summary>
    /// Get car categories and depreciation multipliers
    /// </summary>
    Task<List<CarCategory>> GetCarCategoriesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Get LLM-powered inflation forecast
    /// </summary>
    Task<PredictionData> GetInflationPredictionAsync(string countryCode, CancellationToken cancellationToken);

    /// <summary>
    /// Get LLM-powered exchange rate forecast
    /// </summary>
    Task<PredictionData> GetExchangeRatePredictionAsync(string currencyCode, CancellationToken cancellationToken);

    /// <summary>
    /// Get LLM-powered real estate price forecast
    /// </summary>
    Task<PredictionData> GetRealEstatePredictionAsync(string location, CancellationToken cancellationToken);

    /// <summary>
    /// Get LLM-powered GDP growth forecast
    /// </summary>
    Task<PredictionData> GetGdpPredictionAsync(string countryCode, CancellationToken cancellationToken);

    /// <summary>
    /// Get stock valuation metrics (P/E, P/B, price target, dividend yield, etc.)
    /// </summary>
    Task<StockValuationData> GetStockValuationAsync(string ticker, CancellationToken cancellationToken);

    /// <summary>
    /// Get valuation metrics for multiple stocks
    /// </summary>
    Task<List<StockValuationData>> GetMultipleStockValuationsAsync(List<string> tickers, CancellationToken cancellationToken);

    /// <summary>
    /// Get valuation-based recommendation for a stock
    /// </summary>
    Task<ValuationRecommendationData> GetValuationRecommendationAsync(string ticker, CancellationToken cancellationToken);
}

#region DTOs

/// <summary>
/// Historical CPI inflation data
/// </summary>
public class InflationData
{
    public string CountryCode { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public decimal CurrentRate { get; set; }
    public decimal? PreviousYearRate { get; set; }
    public Dictionary<string, decimal> HistoricalRates { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Multi-country inflation comparison
/// </summary>
public class MultiCountryInflationData
{
    public List<InflationData> Countries { get; set; } = new();
    public DateTime ComparisonDate { get; set; }
    public string? LowestInflationCountry { get; set; }
    public string? HighestInflationCountry { get; set; }
}

/// <summary>
/// Current and historical exchange rate data
/// </summary>
public class ExchangeRateData
{
    public string BaseCurrency { get; set; } = string.Empty;
    public Dictionary<string, decimal> Rates { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public string? Source { get; set; }
}

/// <summary>
/// Exchange rate history for trend analysis
/// </summary>
public class ExchangeHistoryData
{
    public string CurrencyCode { get; set; } = string.Empty;
    public string TargetCurrency { get; set; } = "USD";
    public Dictionary<string, decimal> History { get; set; } = new();
    public DateTime? EarliestDate { get; set; }
    public DateTime? LatestDate { get; set; }
}

/// <summary>
/// Purchasing power parity index
/// </summary>
public class PurchasingPowerData
{
    public string CountryCode { get; set; } = string.Empty;
    public decimal PppIndex { get; set; }
    public decimal? IndexChange { get; set; }
    public string? InterpretationContext { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Real estate market data
/// </summary>
public class RealEstateData
{
    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("country_code")]
    public string? Country { get; set; }

    public decimal? AveragePricePerSqm { get; set; }
    public decimal? RentalYield { get; set; }

    [JsonPropertyName("average_annual_growth")]
    public decimal AverageAnnualGrowth { get; set; } // Annual growth rate percentage

    [JsonPropertyName("data")]
    public List<RealEstateHistoricalData> Data { get; set; } = new(); // Historical price/growth data

    public Dictionary<string, object>? MarketIndicators { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Historical real estate data for growth tracking
/// </summary>
public class RealEstateHistoricalData
{
    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("index_value")]
    public decimal IndexValue { get; set; }

    [JsonPropertyName("growth_rate")]
    public decimal GrowthRate { get; set; }
}

/// <summary>
/// Car depreciation schedule
/// </summary>
public class CarDepreciationData
{
    public decimal InitialPrice { get; set; }
    public int YearCount { get; set; }
    public List<YearlyDepreciation> Schedule { get; set; } = new();
    public decimal TotalDepreciation { get; set; }
    public string? CountryContext { get; set; }
}

/// <summary>
/// Yearly depreciation breakdown
/// </summary>
public class YearlyDepreciation
{
    public int Year { get; set; }
    public decimal Value { get; set; }
    public decimal DepreciationAmount { get; set; }
    public decimal DepreciationPercentage { get; set; }
}

/// <summary>
/// Car category information
/// </summary>
public class CarCategory
{
    public string Name { get; set; } = string.Empty;
    public decimal DepreciationMultiplier { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// LLM-generated prediction data
/// </summary>
public class PredictionData
{
    public string MetricType { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public decimal? CurrentValue { get; set; }
    public decimal? PredictedValue { get; set; }
    public DateTime PredictionDate { get; set; }
    public string? Reasoning { get; set; }
    public string? Confidence { get; set; }
}

/// <summary>
/// Stock valuation metrics for investment decision-making
/// </summary>
public class StockValuationData
{
    public string Ticker { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal? PeRatio { get; set; }
    public decimal? PbRatio { get; set; }
    public decimal? DividendYield { get; set; }
    public decimal? EarningsPerShare { get; set; }
    public decimal? BookValuePerShare { get; set; }
    public decimal? FairValue { get; set; }
    public decimal? PriceTarget { get; set; }
    public string? ValuationHealth { get; set; } // "Undervalued", "Fair", "Overvalued"
    public decimal? MarginOfSafety { get; set; } // Percentage discount to fair value
    public Dictionary<string, object>? AdditionalMetrics { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Valuation-based investment recommendation for a stock
/// </summary>
public class ValuationRecommendationData
{
    public string Ticker { get; set; } = string.Empty;
    public StockValuationData? Valuation { get; set; }
    public string? Recommendation { get; set; } // "Strong Buy", "Buy", "Hold", "Sell", "Strong Sell"
    public decimal? RecommendationConfidence { get; set; } // 0.0 to 1.0
    public string? Reasoning { get; set; }
    public decimal? TargetReturn { get; set; }
    public DateTime RecommendationDate { get; set; }
}

#endregion
