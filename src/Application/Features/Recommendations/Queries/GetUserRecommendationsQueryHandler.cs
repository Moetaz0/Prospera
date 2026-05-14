using AutoMapper;
using MediatR;
using Prospera.Application.DTOs;
using Prospera.Domain.Interfaces;

namespace Prospera.Application.Features.Recommendations.Queries;

public class GetUserRecommendationsQueryHandler : IRequestHandler<GetUserRecommendationsQuery, IEnumerable<InvestmentRecommendationDto>>
{
    private readonly IInvestmentRecommendationRepository _recommendationRepository;
    private readonly IMapper _mapper;

    public GetUserRecommendationsQueryHandler(IInvestmentRecommendationRepository recommendationRepository, IMapper mapper)
    {
        _recommendationRepository = recommendationRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<InvestmentRecommendationDto>> Handle(GetUserRecommendationsQuery request, CancellationToken cancellationToken)
    {
        var recommendations = await _recommendationRepository.GetByUserIdAsync(request.UserId);
        return _mapper.Map<IEnumerable<InvestmentRecommendationDto>>(recommendations);
    }
}
