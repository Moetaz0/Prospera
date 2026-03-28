using MediatR;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Assets.Queries;

public class GetUserAssetsQuery : IRequest<IEnumerable<AssetDto>>
{
    public Guid UserId { get; set; }
}
