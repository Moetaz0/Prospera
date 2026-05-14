using AutoMapper;
using MediatR;
using Prospera.Application.DTOs;
using Prospera.Domain.Interfaces;

namespace Prospera.Application.Features.Recommendations.Queries;

public class GetLatestRecommendationQueryHandler : IRequestHandler<GetLatestRecommendationQuery, InvestmentRecommendationDto?>
{
    private readonly IInvestmentRecommendationRepository _recommendationRepository;
    private readonly IMapper _mapper;

    public GetLatestRecommendationQueryHandler(IInvestmentRecommendationRepository recommendationRepository, IMapper mapper)
    {
        _recommendationRepository = recommendationRepository;
        _mapper = mapper;
    }

    public async Task<InvestmentRecommendationDto?> Handle(GetLatestRecommendationQuery request, CancellationToken cancellationToken)
    {
        var recommendations = await _recommendationRepository.GetByUserIdAsync(request.UserId);
        var latest = recommendations.FirstOrDefault();
        return latest is null ? null : _mapper.Map<InvestmentRecommendationDto>(latest);
    }
}
