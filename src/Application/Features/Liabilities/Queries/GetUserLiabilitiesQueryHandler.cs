using AutoMapper;
using MediatR;
using Prospera.Application.DTOs;
using Prospera.Domain.Interfaces;

namespace Prospera.Application.Features.Liabilities.Queries;

public class GetUserLiabilitiesQueryHandler : IRequestHandler<GetUserLiabilitiesQuery, IEnumerable<LiabilityDto>>
{
    private readonly ILiabilityRepository _liabilityRepository;
    private readonly IMapper _mapper;

    public GetUserLiabilitiesQueryHandler(ILiabilityRepository liabilityRepository, IMapper mapper)
    {
        _liabilityRepository = liabilityRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<LiabilityDto>> Handle(GetUserLiabilitiesQuery request, CancellationToken cancellationToken)
    {
        var liabilities = await _liabilityRepository.GetByUserIdAsync(request.UserId);
        return _mapper.Map<IEnumerable<LiabilityDto>>(liabilities);
    }
}
