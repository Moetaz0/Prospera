using System.Text.Json;
using System.Text.Json.Serialization;
using Prospera.Application.Common.Interfaces;
using Prospera.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace Prospera.Infrastructure.Services.RatesApi;

/// <summary>
/// Service for fetching inflation predictions from Python financial-api
/// </summary>
public class InflationPredictionApiService : IInflationPredictionApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<InflationPredictionApiService> _logger;
    private const string FinancialApiBaseUrl = "http://localhost:8000"; // Python API URL

    public InflationPredictionApiService(HttpClient httpClient, ILogger<InflationPredictionApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Get inflation predictions from Python financial-api
    /// </summary>
    public async Task<InflationPredictionDto?> GetInflationPredictionsAsync(string countryCode, int yearsAhead = 5)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{FinancialApiBaseUrl}/api/v1/inflation/predict/{countryCode.ToUpper()}?years={yearsAhead}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Inflation prediction API returned {response.StatusCode} for country {countryCode}");
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var apiResponse = JsonSerializer.Deserialize<InflationPredictionResponse>(responseContent, options);

            if (apiResponse == null)
            {
                _logger.LogWarning("Failed to deserialize inflation prediction response");
                return null;
            }

            // Map API response to our model
            return new InflationPredictionDto
            {
                CountryCode = apiResponse.country_code,
                CurrentInflationRate = (decimal)apiResponse.current_inflation_rate,
                PredictionMethod = apiResponse.prediction_method ?? "Python Financial-API",
                YearlyPredictions = apiResponse.yearly_predictions?.Select(y => new InflationYearPredictionDto
                {
                    Year = y.year,
                    PredictedInflationRate = (decimal)y.predicted_inflation_rate,
                    Confidence = (decimal)(y.confidence ?? 0.7)
                }).ToList() ?? new(),
                Notes = apiResponse.notes ?? string.Empty,
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(
                $"Failed to connect to financial-api for inflation prediction: {ex.Message}. " +
                $"Ensure Python financial-api is running at {FinancialApiBaseUrl}");
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError($"Failed to parse inflation prediction response: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error getting inflation predictions: {ex.Message}");
            return null;
        }
    }
}

/// <summary>
/// Response model from inflation prediction API
/// </summary>
public class InflationPredictionResponse
{
    [JsonPropertyName("country_code")]
    public string country_code { get; set; } = string.Empty;

    [JsonPropertyName("current_inflation_rate")]
    public float current_inflation_rate { get; set; }

    [JsonPropertyName("prediction_method")]
    public string? prediction_method { get; set; }

    [JsonPropertyName("yearly_predictions")]
    public List<InflationYearPredictionResponse>? yearly_predictions { get; set; }

    [JsonPropertyName("notes")]
    public string? notes { get; set; }
}

/// <summary>
/// Yearly inflation prediction from API response
/// </summary>
public class InflationYearPredictionResponse
{
    [JsonPropertyName("year")]
    public int year { get; set; }

    [JsonPropertyName("predicted_inflation_rate")]
    public float predicted_inflation_rate { get; set; }

    [JsonPropertyName("confidence")]
    public float? confidence { get; set; }
}
