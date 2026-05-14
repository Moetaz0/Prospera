using MediatR;

namespace Prospera.Application.Features.FinancialMetrics.Queries;

/// <summary>
/// Query to get liquidity ratio for a user
/// </summary>
public class GetLiquidityRatioQuery : IRequest<decimal>
{
    public required Guid UserId { get; set; }
}
