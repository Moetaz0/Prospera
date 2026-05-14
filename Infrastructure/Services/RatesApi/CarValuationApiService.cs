using System.Text.Json;
using System.Text.Json.Serialization;
using Prospera.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Prospera.Infrastructure.Services.RatesApi;

/// <summary>
/// Service for fetching car valuation data from Python financial-api
/// Provides detailed depreciation schedules and vehicle value projections
/// </summary>
public class CarValuationApiService : ICarValuationApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CarValuationApiService> _logger;
    private const string FinancialApiBaseUrl = "http://localhost:8000"; // Python API URL

    public CarValuationApiService(HttpClient httpClient, ILogger<CarValuationApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Calculate car depreciation using Python financial-api
    /// </summary>
    public async Task<CarValuationResponse?> GetCarValuationAsync(
        decimal purchasePrice,
        int purchaseYear,
        int? targetYear = null,
        string category = "sedan",
        string country = "US")
    {
        try
        {
            var request = new CarAmortizationRequest
            {
                purchase_price = (float)purchasePrice,
                purchase_year = purchaseYear,
                target_year = targetYear,
                car_category = category,
                country = country,
                annual_mileage_km = 15000
            };

            var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"{FinancialApiBaseUrl}/api/v1/cars/amortization",
                content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    $"Car valuation API returned {response.StatusCode} for car purchased at {purchasePrice} in {purchaseYear}");
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<CarAmortizationResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            if (apiResponse == null)
            {
                _logger.LogWarning("Failed to deserialize car valuation response");
                return null;
            }

            // Map API response to our model
            return new CarValuationResponse
            {
                PurchasePrice = (decimal)apiResponse.purchase_price,
                PurchaseYear = apiResponse.purchase_year,
                TargetYear = apiResponse.target_year,
                CurrentMarketValue = (decimal)apiResponse.current_market_value,
                TotalDepreciationPercent = (decimal)apiResponse.total_depreciation_pct,
                InflationAdjustedValue = apiResponse.inflation_adjusted_value.HasValue
                    ? (decimal)apiResponse.inflation_adjusted_value.Value
                    : null,
                Country = apiResponse.country,
                CarCategory = apiResponse.car_category,
                YearlyBreakdown = apiResponse.yearly_breakdown?.Select(y => new CarYearlyDepreciation
                {
                    Year = y.year,
                    Value = (decimal)y.value,
                    DepreciationRatePercent = (decimal)y.depreciation_rate_pct,
                    CumulativeDepreciationPercent = (decimal)y.cumulative_depreciation_pct
                }).ToList() ?? new(),
                Notes = apiResponse.notes
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(
                $"Failed to connect to financial-api for car valuation: {ex.Message}. " +
                $"Ensure Python financial-api is running at {FinancialApiBaseUrl}");
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError($"Failed to parse car valuation response: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error getting car valuation: {ex.Message}");
            return null;
        }
    }
}

/// <summary>
/// Request model for car amortization API
/// </summary>
public class CarAmortizationRequest
{
    [JsonPropertyName("purchase_price")]
    public float purchase_price { get; set; }

    [JsonPropertyName("purchase_year")]
    public int purchase_year { get; set; }

    [JsonPropertyName("target_year")]
    public int? target_year { get; set; }

    [JsonPropertyName("car_category")]
    public string car_category { get; set; } = "sedan";

    [JsonPropertyName("country")]
    public string country { get; set; } = "US";

    [JsonPropertyName("annual_mileage_km")]
    public int annual_mileage_km { get; set; } = 15000;
}

/// <summary>
/// Response model from car amortization API
/// </summary>
public class CarAmortizationResponse
{
    [JsonPropertyName("purchase_price")]
    public float purchase_price { get; set; }

    [JsonPropertyName("purchase_year")]
    public int purchase_year { get; set; }

    [JsonPropertyName("target_year")]
    public int target_year { get; set; }

    [JsonPropertyName("current_market_value")]
    public float current_market_value { get; set; }

    [JsonPropertyName("total_depreciation_pct")]
    public float total_depreciation_pct { get; set; }

    [JsonPropertyName("inflation_adjusted_value")]
    public float? inflation_adjusted_value { get; set; }

    [JsonPropertyName("country")]
    public string country { get; set; } = string.Empty;

    [JsonPropertyName("car_category")]
    public string car_category { get; set; } = string.Empty;

    [JsonPropertyName("yearly_breakdown")]
    public List<YearlyDepreciationResponse>? yearly_breakdown { get; set; }

    [JsonPropertyName("notes")]
    public string notes { get; set; } = string.Empty;
}

/// <summary>
/// Yearly depreciation data from API response
/// </summary>
public class YearlyDepreciationResponse
{
    [JsonPropertyName("year")]
    public int year { get; set; }

    [JsonPropertyName("value")]
    public float value { get; set; }

    [JsonPropertyName("depreciation_rate_pct")]
    public float depreciation_rate_pct { get; set; }

    [JsonPropertyName("cumulative_depreciation_pct")]
    public float cumulative_depreciation_pct { get; set; }
}
