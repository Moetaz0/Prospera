using MediatR;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Assets.Commands;

public class UpdateAssetCommand : IRequest<AssetDto?>
{
    public Guid UserId { get; set; }
    public Guid AssetId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal CurrentValue { get; set; }
    public string Type { get; set; } = string.Empty;
}
