using MediatR;
using AutoMapper;
using Prospera.Domain.Entities;
using Prospera.Domain.Enums;
using Prospera.Domain.Interfaces;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Assets.Commands;

/// <summary>
/// Handler for AddAssetCommand
/// </summary>
public class AddAssetCommandHandler : IRequestHandler<AddAssetCommand, AssetDto>
{
    private readonly IAssetRepository _assetRepository;
    private readonly IMapper _mapper;

    public AddAssetCommandHandler(IAssetRepository assetRepository, IMapper mapper)
    {
        _assetRepository = assetRepository;
        _mapper = mapper;
    }

    public async Task<AssetDto> Handle(AddAssetCommand request, CancellationToken cancellationToken)
    {
        // Parse asset type
        if (!Enum.TryParse<AssetType>(request.Type, out var assetType))
        {
            assetType = AssetType.Other;
        }

        // Create new asset
        var asset = new Asset(request.Name, request.CurrentValue, assetType, request.UserId);

        // Add to repository
        await _assetRepository.AddAsync(asset);

        // Map and return
        return _mapper.Map<AssetDto>(asset);
    }
}
