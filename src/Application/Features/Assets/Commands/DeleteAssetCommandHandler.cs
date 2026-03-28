using MediatR;
using Prospera.Domain.Interfaces;

namespace Prospera.Application.Features.Assets.Commands;

public class DeleteAssetCommandHandler : IRequestHandler<DeleteAssetCommand, bool>
{
    private readonly IAssetRepository _assetRepository;

    public DeleteAssetCommandHandler(IAssetRepository assetRepository)
    {
        _assetRepository = assetRepository;
    }

    public async Task<bool> Handle(DeleteAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await _assetRepository.GetByIdAsync(request.AssetId, request.UserId);
        if (asset is null)
        {
            return false;
        }

        await _assetRepository.DeleteAsync(request.AssetId);
        return true;
    }
}
