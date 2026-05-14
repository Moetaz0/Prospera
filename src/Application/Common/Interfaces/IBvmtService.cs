using System.Text.Json.Serialization;

namespace Prospera.Application.Common.Interfaces;

/// <summary>
/// Service for accessing BVMT (Bourse des Valeurs Mobilières de Tunis) stock market data
/// Integrates with external FastAPI for Tunisian stock exchange data, analysis, and halal ratings
/// </summary>
public interface IBvmtService
{
    /// <summary>
    /// Get all BVMT stocks with optional halal filtering
    /// </summary>
    Task<BvmtStockListData> GetAllStocksAsync(bool halalOnly = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a single BVMT stock by ticker
    /// </summary>
    Task<BvmtStockData?> GetStockByTickerAsync(string ticker, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get AI-powered analysis for a stock (includes SWOT, recommendation, target price)
    /// </summary>
    Task<BvmtStockAnalysisData?> GetStockAnalysisAsync(string ticker, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all BVMT sectors with aggregated statistics
    /// </summary>
    Task<BvmtSectorsData> GetSectorsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get only halal-compliant BVMT stocks
    /// </summary>
    Task<BvmtStockListData> GetHalalStocksAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get only haram (non-compliant) BVMT stocks
    /// </summary>
    Task<BvmtStockListData> GetHaramStocksAsync(CancellationToken cancellationToken = default);
}

#region DTOs

/// <summary>
/// BVMT stock data
/// </summary>
public class BvmtStockData
{
    [JsonPropertyName("ticker")]
    public string Ticker { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("sector")]
    public string Sector { get; set; } = string.Empty;

    [JsonPropertyName("current_price")]
    public decimal CurrentPrice { get; set; }

    [JsonPropertyName("pe_ratio")]
    public decimal? PeRatio { get; set; }

    [JsonPropertyName("dividend_yield")]
    public decimal? DividendYield { get; set; }

    [JsonPropertyName("market_cap_tnd")]
    public decimal? MarketCapTnd { get; set; }

    [JsonPropertyName("annual_revenue_tnd")]
    public decimal? AnnualRevenueTnd { get; set; }

    [JsonPropertyName("profit_margin")]
    public decimal? ProfitMargin { get; set; }

    [JsonPropertyName("roe")]
    public decimal? Roe { get; set; }

    [JsonPropertyName("is_halal")]
    public bool IsHalal { get; set; } = true;

    [JsonPropertyName("halal_rating")]
    public string? HalalRating { get; set; } // "A", "B", "C"

    [JsonPropertyName("haram_sectors")]
    public List<string>? HaramSectors { get; set; }

    [JsonPropertyName("last_updated")]
    public string LastUpdated { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = "BVMT";
}

/// <summary>
/// BVMT stock list with aggregated market data
/// </summary>
public class BvmtStockListData
{
    [JsonPropertyName("total_stocks")]
    public int TotalStocks { get; set; }

    [JsonPropertyName("stocks")]
    public List<BvmtStockData> Stocks { get; set; } = new();

    [JsonPropertyName("sectors")]
    public List<string> Sectors { get; set; } = new();

    [JsonPropertyName("market_cap_total_tnd")]
    public decimal? MarketCapTotalTnd { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; } = "BVMT";
}

/// <summary>
/// AI-powered stock analysis with recommendation
/// </summary>
public class BvmtStockAnalysisData
{
    [JsonPropertyName("ticker")]
    public string Ticker { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("current_price")]
    public decimal CurrentPrice { get; set; }

    [JsonPropertyName("recommendation")]
    public string Recommendation { get; set; } = string.Empty; // "BUY", "HOLD", "SELL"

    [JsonPropertyName("rating")]
    public int Rating { get; set; } // 1-5 stars

    [JsonPropertyName("target_price")]
    public decimal? TargetPrice { get; set; }

    [JsonPropertyName("upside_downside")]
    public decimal? UpsideDownside { get; set; } // percentage

    [JsonPropertyName("key_metrics")]
    public Dictionary<string, object>? KeyMetrics { get; set; }

    [JsonPropertyName("strengths")]
    public List<string> Strengths { get; set; } = new();

    [JsonPropertyName("weaknesses")]
    public List<string> Weaknesses { get; set; } = new();

    [JsonPropertyName("opportunities")]
    public List<string> Opportunities { get; set; } = new();

    [JsonPropertyName("threats")]
    public List<string> Threats { get; set; } = new();

    [JsonPropertyName("ai_reasoning")]
    public string AiReasoning { get; set; } = string.Empty;

    [JsonPropertyName("confidence")]
    public string Confidence { get; set; } = "medium"; // "low", "medium", "high"

    [JsonPropertyName("model_used")]
    public string ModelUsed { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;
}

/// <summary>
/// BVMT sectors data with aggregated statistics
/// </summary>
public class BvmtSectorsData
{
    public Dictionary<string, object> Data { get; set; } = new();
    public string Source { get; set; } = "BVMT";
}

#endregion
