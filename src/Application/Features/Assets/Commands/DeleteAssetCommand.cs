using MediatR;

namespace Prospera.Application.Features.Assets.Commands;

public class DeleteAssetCommand : IRequest<bool>
{
    public Guid UserId { get; set; }
    public Guid AssetId { get; set; }
}
