using AutoMapper;
using MediatR;
using Prospera.Application.DTOs;
using Prospera.Domain.Interfaces;

namespace Prospera.Application.Features.Liabilities.Queries;

public class GetLiabilityQueryHandler : IRequestHandler<GetLiabilityQuery, LiabilityDto?>
{
    private readonly ILiabilityRepository _liabilityRepository;
    private readonly IMapper _mapper;

    public GetLiabilityQueryHandler(ILiabilityRepository liabilityRepository, IMapper mapper)
    {
        _liabilityRepository = liabilityRepository;
        _mapper = mapper;
    }

    public async Task<LiabilityDto?> Handle(GetLiabilityQuery request, CancellationToken cancellationToken)
    {
        var liability = await _liabilityRepository.GetByIdAsync(request.LiabilityId, request.UserId);
        return liability is null ? null : _mapper.Map<LiabilityDto>(liability);
    }
}
