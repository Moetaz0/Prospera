using MediatR;

namespace Prospera.Application.Features.FinancialMetrics.Queries;

/// <summary>
/// Query to get debt ratio for a user
/// </summary>
public class GetDebtRatioQuery : IRequest<decimal>
{
    public required Guid UserId { get; set; }
}
