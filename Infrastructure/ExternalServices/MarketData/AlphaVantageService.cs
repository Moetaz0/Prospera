using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prospera.Domain.Enums;
using Prospera.Application.Common.Interfaces;

namespace Prospera.Infrastructure.ExternalServices.MarketData;

/// <summary>
/// Alpha Vantage implementation of market data service
/// Implements both the Application layer IMarketDataService and Infrastructure-specific IMarketDataServiceInfra
/// </summary>
public class AlphaVantageService : IMarketDataService, IMarketDataServiceInfra
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AlphaVantageService> _logger;
    private readonly string _apiKey;
    private const string AlphaVantageBaseUrl = "https://www.alphavantage.co/";

    public AlphaVantageService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AlphaVantageService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _apiKey = configuration["AlphaVantage:ApiKey"] ?? "demo";
        _httpClient.BaseAddress = new Uri(AlphaVantageBaseUrl);
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
            var jsonData = JsonSerializer.Deserialize<JsonElement>(content);

            if (!jsonData.TryGetProperty("Global Quote", out var globalQuote))
            {
                _logger.LogWarning("No 'Global Quote' found in response for {Symbol}", symbol);
                return null;
            }

            // Extract fields from Alpha Vantage response
            decimal price = 0;
            decimal changePercent = 0;
            long volume = 0;

            if (globalQuote.TryGetProperty("05. price", out var priceElement))
            {
                decimal.TryParse(priceElement.GetString(), out price);
            }

            if (globalQuote.TryGetProperty("10. change percent", out var changePercentElement))
            {
                var changePercentStr = changePercentElement.GetString()?.Replace("%", "") ?? "0";
                decimal.TryParse(changePercentStr, out changePercent);
            }

            if (globalQuote.TryGetProperty("06. volume", out var volumeElement))
            {
                long.TryParse(volumeElement.GetString(), out volume);
            }

            return new StockQuote
            {
                Symbol = symbol,
                Price = price,
                ChangePercent = changePercent,
                Volume = volume,
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

    /// <summary>
    /// Fetches market trend data including stock market (S&P 500) and cryptocurrency (Bitcoin) prices
    /// </summary>
    public async Task<MarketTrendData?> GetMarketTrendDataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var marketData = new MarketTrendData();

            // Get S&P 500 data (proxy: SPY ETF)
            var sp500Response = await GetStockQuoteAsync("SPY", cancellationToken);
            if (sp500Response != null)
            {
                marketData.StockMarketPrice = sp500Response.Price;
                marketData.StockMarketTrend = sp500Response.ChangePercent / 100m;
            }

            // Get Bitcoin price via currency exchange rate endpoint
            try
            {
                var btcResponse = await _httpClient.GetAsync(
                    $"query?function=CURRENCY_EXCHANGE_RATE&from_currency=BTC&to_currency=USD&apikey={_apiKey}",
                    cancellationToken);

                if (btcResponse.IsSuccessStatusCode)
                {
                    var content = await btcResponse.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogInformation("Bitcoin API response: {Response}", content);
                    var btcData = JsonSerializer.Deserialize<JsonElement>(content);

                    if (btcData.TryGetProperty("Realtime Currency Exchange Rate", out var rate))
                    {
                        if (rate.TryGetProperty("5. Exchange Rate", out var exchangeRate))
                        {
                            if (decimal.TryParse(exchangeRate.GetString(), out var btcPrice))
                            {
                                marketData.CryptoPriceUSD = btcPrice;
                                _logger.LogInformation("Successfully fetched Bitcoin price: ${Price}", btcPrice);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("'Realtime Currency Exchange Rate' not found in BTC API response");
                    }
                }
                else
                {
                    _logger.LogWarning("Bitcoin API returned status {StatusCode}. Using default value.", btcResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch Bitcoin price, using default value");
            }

            marketData.FetchedAt = DateTime.UtcNow;
            return marketData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching market trend data");
            // Return default market data if API calls fail
            return new MarketTrendData { FetchedAt = DateTime.UtcNow };
        }
    }

    /// <summary>
    /// Retrieves current market data for a given asset symbol (Application interface implementation)
    /// </summary>
    public async Task<MarketDataDto> GetMarketDataAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var quote = await GetStockQuoteAsync(symbol, cancellationToken);

        if (quote != null)
        {
            return new MarketDataDto
            {
                Symbol = symbol,
                CurrentPrice = quote.Price,
                ChangePercent = quote.ChangePercent,
                Timestamp = quote.Timestamp,
                Volume = quote.Volume
            };
        }

        return new MarketDataDto 
        { 
            Symbol = symbol, 
            Timestamp = DateTime.UtcNow 
        };
    }

    /// <summary>
    /// Retrieves historical market data for trend analysis (Application interface implementation)
    /// </summary>
    public async Task<IEnumerable<MarketDataDto>> GetHistoricalDataAsync(string symbol, int days, CancellationToken cancellationToken = default)
    {
        // For now, return current quote as placeholder
        // Full historical data implementation would require TIME_SERIES function from Alpha Vantage
        var currentData = await GetMarketDataAsync(symbol, cancellationToken);
        return new List<MarketDataDto> { currentData };
    }
}