using MediatR;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Assets.Queries;

public class GetAssetQuery : IRequest<AssetDto?>
{
    public Guid UserId { get; set; }
    public Guid AssetId { get; set; }
}
