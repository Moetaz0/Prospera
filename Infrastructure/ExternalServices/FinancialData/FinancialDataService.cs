using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prospera.Application.Common.Interfaces;

namespace Prospera.Infrastructure.ExternalServices.FinancialData;

/// <summary>
/// Client for external FastAPI financial data system
/// Provides real-time market data for coaching enrichment
/// </summary>
public class FinancialDataService : IFinancialDataService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly ILogger<FinancialDataService> _logger;
    private const int RequestTimeoutSeconds = 10;

    public FinancialDataService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<FinancialDataService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["FinancialDataApi:Url"] ?? "http://localhost:8000/api/v1";

        _httpClient.Timeout = TimeSpan.FromSeconds(RequestTimeoutSeconds);
    }

    public async Task<InflationData> GetInflationDataAsync(string countryCode, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/inflation/{countryCode}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<InflationData>(json) ?? new InflationData { CountryCode = countryCode };
            }

            _logger.LogWarning($"Failed to get inflation data for {countryCode}: {response.StatusCode}");
            return new InflationData { CountryCode = countryCode };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching inflation data for {countryCode}");
            return new InflationData { CountryCode = countryCode };
        }
    }

    public async Task<MultiCountryInflationData> GetMultiCountryInflationAsync(List<string> countryCodes, CancellationToken cancellationToken)
    {
        try
        {
            var countriesParam = string.Join(",", countryCodes);
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/inflation?countries={countriesParam}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<MultiCountryInflationData>(json) ?? new MultiCountryInflationData();
            }

            _logger.LogWarning($"Failed to get multi-country inflation data: {response.StatusCode}");
            return new MultiCountryInflationData();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching multi-country inflation data");
            return new MultiCountryInflationData();
        }
    }

    public async Task<ExchangeRateData> GetCurrentExchangeRatesAsync(string baseCurrency, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/exchange/current/{baseCurrency}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<ExchangeRateData>(json) ?? new ExchangeRateData { BaseCurrency = baseCurrency };
            }

            _logger.LogWarning($"Failed to get exchange rates for {baseCurrency}: {response.StatusCode}");
            return new ExchangeRateData { BaseCurrency = baseCurrency };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching exchange rates for {baseCurrency}");
            return new ExchangeRateData { BaseCurrency = baseCurrency };
        }
    }

    public async Task<ExchangeRateData> GetTndExchangeRatesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/exchange/tnd",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<ExchangeRateData>(json) ?? new ExchangeRateData { BaseCurrency = "TND" };
            }

            _logger.LogWarning($"Failed to get TND exchange rates: {response.StatusCode}");
            return new ExchangeRateData { BaseCurrency = "TND" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching TND exchange rates");
            return new ExchangeRateData { BaseCurrency = "TND" };
        }
    }

    public async Task<ExchangeHistoryData> GetExchangeHistoryAsync(string currencyCode, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/exchange/history/{currencyCode}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<ExchangeHistoryData>(json) ?? new ExchangeHistoryData { CurrencyCode = currencyCode };
            }

            _logger.LogWarning($"Failed to get exchange history for {currencyCode}: {response.StatusCode}");
            return new ExchangeHistoryData { CurrencyCode = currencyCode };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching exchange history for {currencyCode}");
            return new ExchangeHistoryData { CurrencyCode = currencyCode };
        }
    }

    public async Task<PurchasingPowerData> GetPurchasingPowerAsync(string countryCode, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/exchange/purchasing-power/{countryCode}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<PurchasingPowerData>(json) ?? new PurchasingPowerData { CountryCode = countryCode };
            }

            _logger.LogWarning($"Failed to get purchasing power for {countryCode}: {response.StatusCode}");
            return new PurchasingPowerData { CountryCode = countryCode };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching purchasing power for {countryCode}");
            return new PurchasingPowerData { CountryCode = countryCode };
        }
    }

    public async Task<List<string>> GetRealEstateLocationsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/real-estate/locations",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }

            _logger.LogWarning($"Failed to get real estate locations: {response.StatusCode}");
            return new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching real estate locations");
            return new List<string>();
        }
    }

    public async Task<RealEstateData> GetTunisianRealEstateAsync(string location, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/real-estate/tunisia/{location}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<RealEstateData>(json) ?? new RealEstateData { Location = location, Country = "Tunisia" };
            }

            _logger.LogWarning($"Failed to get Tunisian real estate data for {location}: {response.StatusCode}");
            return new RealEstateData { Location = location, Country = "Tunisia" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching Tunisian real estate data for {location}");
            return new RealEstateData { Location = location, Country = "Tunisia" };
        }
    }

    public async Task<RealEstateData> GetGlobalRealEstateAsync(string countryCode, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/real-estate/global/{countryCode}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<RealEstateData>(json) ?? new RealEstateData { Location = countryCode };
            }

            _logger.LogWarning($"Failed to get global real estate data for {countryCode}: {response.StatusCode}");
            return new RealEstateData { Location = countryCode };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching global real estate data for {countryCode}");
            return new RealEstateData { Location = countryCode };
        }
    }

    public async Task<CarDepreciationData> GetCarDepreciationAsync(decimal purchasePrice, int years, string countryCode, CancellationToken cancellationToken)
    {
        try
        {
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "purchase_price", purchasePrice.ToString() },
                { "years", years.ToString() },
                { "country_code", countryCode }
            });

            var response = await _httpClient.PostAsync(
                $"{_baseUrl}/cars/amortization",
                content,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<CarDepreciationData>(json) ?? new CarDepreciationData { InitialPrice = purchasePrice };
            }

            _logger.LogWarning($"Failed to get car depreciation: {response.StatusCode}");
            return new CarDepreciationData { InitialPrice = purchasePrice };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching car depreciation");
            return new CarDepreciationData { InitialPrice = purchasePrice };
        }
    }

    public async Task<List<CarCategory>> GetCarCategoriesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/cars/categories",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<List<CarCategory>>(json) ?? new List<CarCategory>();
            }

            _logger.LogWarning($"Failed to get car categories: {response.StatusCode}");
            return new List<CarCategory>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching car categories");
            return new List<CarCategory>();
        }
    }

    public async Task<PredictionData> GetInflationPredictionAsync(string countryCode, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/predictions/inflation/{countryCode}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<PredictionData>(json) ?? new PredictionData { MetricType = "Inflation", Subject = countryCode };
            }

            _logger.LogWarning($"Failed to get inflation prediction for {countryCode}: {response.StatusCode}");
            return new PredictionData { MetricType = "Inflation", Subject = countryCode };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching inflation prediction for {countryCode}");
            return new PredictionData { MetricType = "Inflation", Subject = countryCode };
        }
    }

    public async Task<PredictionData> GetExchangeRatePredictionAsync(string currencyCode, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/predictions/exchange-rate/{currencyCode}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<PredictionData>(json) ?? new PredictionData { MetricType = "ExchangeRate", Subject = currencyCode };
            }

            _logger.LogWarning($"Failed to get exchange rate prediction for {currencyCode}: {response.StatusCode}");
            return new PredictionData { MetricType = "ExchangeRate", Subject = currencyCode };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching exchange rate prediction for {currencyCode}");
            return new PredictionData { MetricType = "ExchangeRate", Subject = currencyCode };
        }
    }

    public async Task<PredictionData> GetRealEstatePredictionAsync(string location, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/predictions/real-estate/tunisia/{location}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<PredictionData>(json) ?? new PredictionData { MetricType = "RealEstate", Subject = location };
            }

            _logger.LogWarning($"Failed to get real estate prediction for {location}: {response.StatusCode}");
            return new PredictionData { MetricType = "RealEstate", Subject = location };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching real estate prediction for {location}");
            return new PredictionData { MetricType = "RealEstate", Subject = location };
        }
    }

    public async Task<PredictionData> GetGdpPredictionAsync(string countryCode, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/predictions/gdp-growth/{countryCode}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<PredictionData>(json) ?? new PredictionData { MetricType = "GdpGrowth", Subject = countryCode };
            }

            _logger.LogWarning($"Failed to get GDP prediction for {countryCode}: {response.StatusCode}");
            return new PredictionData { MetricType = "GdpGrowth", Subject = countryCode };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching GDP prediction for {countryCode}");
            return new PredictionData { MetricType = "GdpGrowth", Subject = countryCode };
        }
    }

    public async Task<StockValuationData> GetStockValuationAsync(string ticker, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/valuation/{ticker}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<StockValuationData>(json) ?? new StockValuationData { Ticker = ticker };
            }

            _logger.LogWarning($"Failed to get valuation data for {ticker}: {response.StatusCode}");
            return new StockValuationData { Ticker = ticker };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching valuation data for {ticker}");
            return new StockValuationData { Ticker = ticker };
        }
    }

    public async Task<List<StockValuationData>> GetMultipleStockValuationsAsync(List<string> tickers, CancellationToken cancellationToken)
    {
        try
        {
            var tickerList = string.Join(",", tickers);
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/valuation/multiple?tickers={tickerList}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<List<StockValuationData>>(json) ?? new List<StockValuationData>();
            }

            _logger.LogWarning($"Failed to get multiple valuations for tickers: {response.StatusCode}");
            return tickers.Select(t => new StockValuationData { Ticker = t }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching multiple valuations");
            return tickers.Select(t => new StockValuationData { Ticker = t }).ToList();
        }
    }

    public async Task<ValuationRecommendationData> GetValuationRecommendationAsync(string ticker, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/valuation/recommendation/{ticker}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<ValuationRecommendationData>(json) ?? new ValuationRecommendationData { Ticker = ticker };
            }

            _logger.LogWarning($"Failed to get valuation recommendation for {ticker}: {response.StatusCode}");
            return new ValuationRecommendationData { Ticker = ticker };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching valuation recommendation for {ticker}");
            return new ValuationRecommendationData { Ticker = ticker };
        }
    }
}
