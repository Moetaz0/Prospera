namespace Prospera.Application.Common.Interfaces;

public interface IMarketDataService
{
    /// <summary>
    /// Retrieves current market data for a given asset symbol
    /// </summary>
    Task<MarketDataDto> GetMarketDataAsync(string symbol, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves historical market data for trend analysis
    /// </summary>
    Task<IEnumerable<MarketDataDto>> GetHistoricalDataAsync(string symbol, int days, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches market trend data including stock market (S&P 500) and cryptocurrency (Bitcoin) prices
    /// </summary>
    Task<MarketTrendData?> GetMarketTrendDataAsync(CancellationToken cancellationToken = default);
}

public class MarketDataDto
{
    public string Symbol { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal ChangePercent { get; set; }
    public DateTime Timestamp { get; set; }
    public long Volume { get; set; }
}

/// <summary>
/// Market trend data snapshot for portfolio allocation decisions
/// </summary>
public class MarketTrendData
{
    /// <summary>
    /// S&P 500 stock market price (using SPY as proxy)
    /// </summary>
    public decimal StockMarketPrice { get; set; } = 400m;

    /// <summary>
    /// S&P 500 stock market trend as decimal (e.g., 0.05 = 5% increase)
    /// </summary>
    public decimal StockMarketTrend { get; set; } = 0m;

    /// <summary>
    /// Bitcoin price in USD
    /// </summary>
    public decimal CryptoPriceUSD { get; set; } = 50000m;

    /// <summary>
    /// When this market data was fetched
    /// </summary>
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
}
