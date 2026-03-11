using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prospera.Domain.Enums;

namespace Prospera.Infrastructure.ExternalServices.MarketData;

public class AlphaVantageService : IMarketDataService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AlphaVantageService> _logger;
    private readonly string _apiKey;

    public AlphaVantageService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AlphaVantageService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _apiKey = configuration["AlphaVantage:ApiKey"] ?? "";
        _httpClient.BaseAddress = new Uri("https://www.alphavantage.co/");
    }

    public async Task<StockQuote?> GetStockQuoteAsync(string symbol, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"query?function=GLOBAL_QUOTE&symbol={symbol}&apikey={_apiKey}",
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            // Parse and return (simplified - you'll need to parse the actual JSON structure)

            return new StockQuote
            {
                Symbol = symbol,
                Price = 0, // Parse from response
                ChangePercent = 0,
                Volume = 0,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching quote for {Symbol}", symbol);
            return null;
        }
    }

    public async Task<List<MarketStock>> GetTopPerformingStocksAsync(
        RiskProfile riskLevel,
        CancellationToken cancellationToken = default)
    {
        // Get predefined symbols based on risk level
        var symbols = GetSymbolsForRiskLevel(riskLevel);
        var stocks = new List<MarketStock>();

        foreach (var symbol in symbols.Take(5)) // Limit to avoid API rate limits
        {
            var quote = await GetStockQuoteAsync(symbol, cancellationToken);
            if (quote != null)
            {
                stocks.Add(new MarketStock
                {
                    Symbol = symbol,
                    CompanyName = symbol,
                    CurrentPrice = quote.Price,
                    ChangePercent = quote.ChangePercent,
                    Volume = quote.Volume,
                    AssetType = AssetType.Stock
                });
            }

            // Rate limiting - Alpha Vantage free tier: 25 requests/day
            await Task.Delay(12000, cancellationToken); // 12 seconds between calls
        }

        return stocks;
    }

    private List<string> GetSymbolsForRiskLevel(RiskProfile level)
    {
        return level switch
        {
            RiskProfile.Conservative => new List<string> { "JNJ", "PG", "KO", "T", "VZ" },
            RiskProfile.Moderate => new List<string> { "AAPL", "MSFT", "GOOGL", "V", "JPM" },
            RiskProfile.Aggressive => new List<string> { "TSLA", "NVDA", "AMD", "PLTR", "COIN" },
            _ => new List<string>()
        };
    }
}