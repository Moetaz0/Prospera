using MediatR;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Liabilities.Queries;

public class GetUserLiabilitiesQuery : IRequest<IEnumerable<LiabilityDto>>
{
    public Guid UserId { get; set; }
}
