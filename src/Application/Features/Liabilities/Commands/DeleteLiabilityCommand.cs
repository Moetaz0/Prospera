using MediatR;

namespace Prospera.Application.Features.Liabilities.Commands;

public class DeleteLiabilityCommand : IRequest<bool>
{
    public Guid UserId { get; set; }
    public Guid LiabilityId { get; set; }
}
