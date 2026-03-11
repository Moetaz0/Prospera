using MediatR;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Assets.Commands;

/// <summary>
/// Command to add a new asset to a user's portfolio
/// </summary>
public class AddAssetCommand : IRequest<AssetDto>
{
    public required Guid UserId { get; set; }
    public required string Name { get; set; }
    public required decimal CurrentValue { get; set; }
    public required string Type { get; set; } // AssetType as string
}
