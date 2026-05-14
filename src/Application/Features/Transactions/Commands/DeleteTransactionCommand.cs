using MediatR;

namespace Prospera.Application.Features.Transactions.Commands;

public class DeleteTransactionCommand : IRequest<bool>
{
    public Guid UserId { get; set; }
    public Guid TransactionId { get; set; }
}
