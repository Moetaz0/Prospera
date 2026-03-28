using AutoMapper;
using MediatR;
using Prospera.Application.DTOs;
using Prospera.Domain.Interfaces;

namespace Prospera.Application.Features.Assets.Queries;

public class GetAssetQueryHandler : IRequestHandler<GetAssetQuery, AssetDto?>
{
    private readonly IAssetRepository _assetRepository;
    private readonly IMapper _mapper;

    public GetAssetQueryHandler(IAssetRepository assetRepository, IMapper mapper)
    {
        _assetRepository = assetRepository;
        _mapper = mapper;
    }

    public async Task<AssetDto?> Handle(GetAssetQuery request, CancellationToken cancellationToken)
    {
        var asset = await _assetRepository.GetByIdAsync(request.AssetId, request.UserId);
        return asset is null ? null : _mapper.Map<AssetDto>(asset);
    }
}
