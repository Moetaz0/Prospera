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
}

public class MarketDataDto
{
    public string Symbol { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal ChangePercent { get; set; }
    public DateTime Timestamp { get; set; }
    public long Volume { get; set; }
}
