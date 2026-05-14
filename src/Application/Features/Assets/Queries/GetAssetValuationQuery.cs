using MediatR;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Assets.Queries;

/// <summary>
/// Query to get asset valuation with 1-year projections
/// Includes inflation impact, car depreciation, and real estate appreciation
/// </summary>
public class GetAssetValuationQuery : IRequest<AssetValuationDto>
{
    /// <summary>
    /// User ID who owns the asset
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// Asset ID to get valuation for
    /// </summary>
    public required Guid AssetId { get; set; }

    /// <summary>
    /// Optional: Override inflation rate (% e.g., 3.5 for 3.5%)
    /// </summary>
    public decimal? InflationRate { get; set; }

    /// <summary>
    /// Optional: Override car depreciation rate (% per year, default 15%)
    /// </summary>
    public decimal? CarDepreciationRate { get; set; }

    /// <summary>
    /// Optional: Override real estate appreciation rate (% per year, default 3%)
    /// </summary>
    public decimal? RealEstateAppreciationRate { get; set; }
}
