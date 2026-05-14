using MediatR;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Recommendations.Queries;

public class GetUserRecommendationsQuery : IRequest<IEnumerable<InvestmentRecommendationDto>>
{
    public Guid UserId { get; set; }
}
