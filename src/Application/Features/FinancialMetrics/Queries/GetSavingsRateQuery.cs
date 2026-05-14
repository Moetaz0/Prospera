using MediatR;

namespace Prospera.Application.Features.FinancialMetrics.Queries;

/// <summary>
/// Query to get savings rate for a user
/// </summary>
public class GetSavingsRateQuery : IRequest<decimal>
{
    public required Guid UserId { get; set; }
}
