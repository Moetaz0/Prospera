using AutoMapper;
using MediatR;
using Prospera.Application.DTOs;
using Prospera.Domain.Interfaces;

namespace Prospera.Application.Features.Recommendations.Queries;

public class GetRecommendationQueryHandler : IRequestHandler<GetRecommendationQuery, InvestmentRecommendationDto?>
{
    private readonly IInvestmentRecommendationRepository _recommendationRepository;
    private readonly IMapper _mapper;

    public GetRecommendationQueryHandler(IInvestmentRecommendationRepository recommendationRepository, IMapper mapper)
    {
        _recommendationRepository = recommendationRepository;
        _mapper = mapper;
    }

    public async Task<InvestmentRecommendationDto?> Handle(GetRecommendationQuery request, CancellationToken cancellationToken)
    {
        var recommendation = await _recommendationRepository.GetByIdAsync(request.RecommendationId);
        if (recommendation is null || recommendation.UserId != request.UserId)
        {
            return null;
        }

        return _mapper.Map<InvestmentRecommendationDto>(recommendation);
    }
}
