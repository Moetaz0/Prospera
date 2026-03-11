

using Prospera.Domain.Enums;

namespace Prospera.Infrastructure.ExternalServices.MarketData;

public class MarketStock
{
    public string Symbol { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal ChangePercent { get; set; }
    public long Volume { get; set; }
    public long MarketCap { get; set; }
    public decimal PeRatio { get; set; }
    public decimal DividendYield { get; set; }
    public string Sector { get; set; } = string.Empty;
    public AssetType AssetType { get; set; }
    public decimal Volatility { get; set; }
}
