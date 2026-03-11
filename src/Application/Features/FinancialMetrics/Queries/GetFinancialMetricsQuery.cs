using MediatR;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.FinancialMetrics.Queries;

/// <summary>
/// Query to get financial metrics for a user
/// </summary>
public class GetFinancialMetricsQuery : IRequest<FinancialMetricsDto>
{
    public required Guid UserId { get; set; }
}
