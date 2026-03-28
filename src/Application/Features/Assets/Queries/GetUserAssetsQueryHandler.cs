using AutoMapper;
using MediatR;
using Prospera.Application.DTOs;
using Prospera.Domain.Interfaces;

namespace Prospera.Application.Features.Assets.Queries;

public class GetUserAssetsQueryHandler : IRequestHandler<GetUserAssetsQuery, IEnumerable<AssetDto>>
{
    private readonly IAssetRepository _assetRepository;
    private readonly IMapper _mapper;

    public GetUserAssetsQueryHandler(IAssetRepository assetRepository, IMapper mapper)
    {
        _assetRepository = assetRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AssetDto>> Handle(GetUserAssetsQuery request, CancellationToken cancellationToken)
    {
        var assets = await _assetRepository.GetByUserIdAsync(request.UserId);
        return _mapper.Map<IEnumerable<AssetDto>>(assets);
    }
}
