using MediatR;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Recommendations.Queries;

public class GetLatestRecommendationQuery : IRequest<InvestmentRecommendationDto?>
{
    public Guid UserId { get; set; }
}
