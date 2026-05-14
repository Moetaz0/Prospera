using MediatR;
using Prospera.Application.Common.Interfaces;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Financial.Queries;

/// <summary>
/// Handler for GetInflationPredictionQuery
/// Fetches inflation predictions from Python financial-api
/// </summary>
public class GetInflationPredictionQueryHandler : IRequestHandler<GetInflationPredictionQuery, InflationPredictionDto>
{
    private readonly IExternalRatesService _externalRatesService;
    private readonly IInflationPredictionApiService _inflationApiService;

    public GetInflationPredictionQueryHandler(
        IExternalRatesService externalRatesService,
        IInflationPredictionApiService inflationApiService)
    {
        _externalRatesService = externalRatesService;
        _inflationApiService = inflationApiService;
    }

    public async Task<InflationPredictionDto> Handle(GetInflationPredictionQuery request, CancellationToken cancellationToken)
    {
        // Get current inflation rate
        var currentInflationRate = await _externalRatesService.GetInflationRateAsync(request.CountryCode);

        // Get inflation predictions from Python financial-api
        var predictions = await _inflationApiService.GetInflationPredictionsAsync(
            countryCode: request.CountryCode,
            yearsAhead: request.YearsAhead);

        if (predictions != null)
        {
            return predictions;
        }

        // Fallback: return basic prediction based on current rate
        return new InflationPredictionDto
        {
            CountryCode = request.CountryCode,
            CurrentInflationRate = currentInflationRate,
            PredictionMethod = "Fallback - Linear Projection",
            YearlyPredictions = GenerateFallbackPredictions(currentInflationRate, request.YearsAhead),
            Notes = "Predictions based on current rate. For more accurate forecasts, ensure Python financial-api is running."
        };
    }

    /// <summary>
    /// Generate simple fallback predictions when API is unavailable
    /// </summary>
    private List<InflationYearPredictionDto> GenerateFallbackPredictions(decimal currentRate, int yearsAhead)
    {
        var predictions = new List<InflationYearPredictionDto>();
        var startYear = DateTime.UtcNow.Year;

        for (int i = 1; i <= yearsAhead; i++)
        {
            // Simple linear projection with slight regression to mean
            var adjustedRate = currentRate * (1 - (0.05m * i)); // 5% regression per year
            adjustedRate = Math.Max(0.5m, Math.Min(adjustedRate, 15m)); // Clamp between 0.5% and 15%

            predictions.Add(new InflationYearPredictionDto
            {
                Year = startYear + i,
                PredictedInflationRate = adjustedRate,
                Confidence = 0.6m - (0.05m * i) // Decreasing confidence for distant years
            });
        }

        return predictions;
    }
}
