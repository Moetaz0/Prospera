using Prospera.Application.DTOs;

namespace Prospera.Application.Common.Interfaces;

/// <summary>
/// Service for fetching inflation predictions from Python financial-api
/// </summary>
public interface IInflationPredictionApiService
{
    /// <summary>
    /// Get inflation predictions for a country
    /// </summary>
    /// <param name="countryCode">ISO country code</param>
    /// <param name="yearsAhead">Number of years to predict</param>
    /// <returns>Inflation prediction data</returns>
    Task<InflationPredictionDto?> GetInflationPredictionsAsync(string countryCode, int yearsAhead = 5);
}
