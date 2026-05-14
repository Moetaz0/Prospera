using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prospera.Application.Common.Interfaces;

namespace Prospera.Infrastructure.ExternalServices.BVMT;

/// <summary>
/// Client for BVMT (Tunisian Stock Exchange) data
/// Provides stock market data, analysis, and halal ratings for Tunisian securities
/// </summary>
public class BvmtService : IBvmtService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly ILogger<BvmtService> _logger;

    public BvmtService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<BvmtService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        var baseUrl = configuration["FinancialDataApi:Url"] ?? "http://localhost:8000";
        // Ensure the URL includes the /api/v1 path
        _baseUrl = baseUrl.TrimEnd('/') + "/api/v1";
    }

    public async Task<BvmtStockListData> GetAllStocksAsync(bool halalOnly = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = halalOnly
                ? $"{_baseUrl}/bvmt/stocks?halal_only=true"
                : $"{_baseUrl}/bvmt/stocks";

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<BvmtStockListData>(json) ?? new BvmtStockListData();
            }

            _logger.LogWarning($"Failed to get BVMT stocks (halalOnly={halalOnly}): {response.StatusCode}");
            return new BvmtStockListData();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching BVMT stocks");
            return new BvmtStockListData();
        }
    }

    public async Task<BvmtStockData?> GetStockByTickerAsync(string ticker, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/bvmt/stocks/{ticker}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<BvmtStockData>(json);
            }

            _logger.LogWarning($"Failed to get BVMT stock {ticker}: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching BVMT stock {ticker}");
            return null;
        }
    }

    public async Task<BvmtStockAnalysisData?> GetStockAnalysisAsync(string ticker, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/bvmt/stocks/{ticker}/analysis",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<BvmtStockAnalysisData>(json);
            }

            _logger.LogWarning($"Failed to get BVMT stock analysis for {ticker}: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching BVMT stock analysis for {ticker}");
            return null;
        }
    }

    public async Task<BvmtSectorsData> GetSectorsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/bvmt/sectors",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new();
                return new BvmtSectorsData { Data = data };
            }

            _logger.LogWarning($"Failed to get BVMT sectors: {response.StatusCode}");
            return new BvmtSectorsData();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching BVMT sectors");
            return new BvmtSectorsData();
        }
    }

    public async Task<BvmtStockListData> GetHalalStocksAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/bvmt/halal-stocks",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<BvmtStockListData>(json) ?? new BvmtStockListData();
            }

            _logger.LogWarning($"Failed to get BVMT halal stocks: {response.StatusCode}");
            return new BvmtStockListData();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching BVMT halal stocks");
            return new BvmtStockListData();
        }
    }

    public async Task<BvmtStockListData> GetHaramStocksAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/bvmt/haram-stocks",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<BvmtStockListData>(json) ?? new BvmtStockListData();
            }

            _logger.LogWarning($"Failed to get BVMT haram stocks: {response.StatusCode}");
            return new BvmtStockListData();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching BVMT haram stocks");
            return new BvmtStockListData();
        }
    }
}
