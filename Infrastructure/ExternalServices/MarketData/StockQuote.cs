namespace Prospera.Infrastructure.ExternalServices.MarketData;

public class StockQuote
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal ChangePercent { get; set; }
    public long Volume { get; set; }
    public DateTime Timestamp { get; set; }
}
