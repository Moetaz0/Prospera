using MediatR;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Recommendations.Queries;

public class GetRecommendationQuery : IRequest<InvestmentRecommendationDto?>
{
    public Guid UserId { get; set; }
    public Guid RecommendationId { get; set; }
}
