

using Prospera.Domain.Enums;

namespace Prospera.Infrastructure.ExternalServices.MarketData;

/// <summary>
/// Infrastructure-specific market data service interface for stock quotes and risk-based stock recommendations
/// </summary>
public interface IMarketDataServiceInfra
{
    Task<StockQuote?> GetStockQuoteAsync(string symbol, CancellationToken cancellationToken = default);
    Task<List<MarketStock>> GetTopPerformingStocksAsync(RiskProfile riskProfile, CancellationToken cancellationToken = default);
}
