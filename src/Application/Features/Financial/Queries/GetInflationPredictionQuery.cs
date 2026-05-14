using MediatR;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Financial.Queries;

/// <summary>
/// Query to get inflation predictions for a country
/// Uses Python financial-api to fetch historical data and trends
/// </summary>
public class GetInflationPredictionQuery : IRequest<InflationPredictionDto>
{
    /// <summary>
    /// ISO country code (e.g., "US", "FR", "TN")
    /// </summary>
    public required string CountryCode { get; set; }

    /// <summary>
    /// Number of years to predict ahead (default: 5)
    /// </summary>
    public int YearsAhead { get; set; } = 5;
}
