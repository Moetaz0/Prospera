

using Prospera.Domain.Enums;

namespace Prospera.Infrastructure.ExternalServices.MarketData;

public interface IMarketDataService
{
    Task<StockQuote?> GetStockQuoteAsync(string symbol, CancellationToken cancellationToken = default);
    Task<List<MarketStock>> GetTopPerformingStocksAsync(RiskProfile riskProfile, CancellationToken cancellationToken = default);
}
