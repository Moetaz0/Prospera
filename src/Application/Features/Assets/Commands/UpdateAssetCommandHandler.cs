using AutoMapper;
using MediatR;
using Prospera.Application.DTOs;
using Prospera.Domain.Enums;
using Prospera.Domain.Interfaces;

namespace Prospera.Application.Features.Assets.Commands;

public class UpdateAssetCommandHandler : IRequestHandler<UpdateAssetCommand, AssetDto?>
{
    private readonly IAssetRepository _assetRepository;
    private readonly IMapper _mapper;

    public UpdateAssetCommandHandler(IAssetRepository assetRepository, IMapper mapper)
    {
        _assetRepository = assetRepository;
        _mapper = mapper;
    }

    public async Task<AssetDto?> Handle(UpdateAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await _assetRepository.GetByIdAsync(request.AssetId, request.UserId);
        if (asset is null)
        {
            return null;
        }

        if (!Enum.TryParse<AssetType>(request.Type, out var type))
        {
            type = AssetType.Other;
        }

        asset.UpdateDetails(request.Name, request.CurrentValue, type);
        await _assetRepository.UpdateAsync(asset);

        return _mapper.Map<AssetDto>(asset);
    }
}
