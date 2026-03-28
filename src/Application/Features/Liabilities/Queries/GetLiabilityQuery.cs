using MediatR;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Liabilities.Queries;

public class GetLiabilityQuery : IRequest<LiabilityDto?>
{
    public Guid UserId { get; set; }
    public Guid LiabilityId { get; set; }
}
