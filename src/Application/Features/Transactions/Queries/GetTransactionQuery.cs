using MediatR;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Transactions.Queries;

public class GetTransactionQuery : IRequest<TransactionDto?>
{
    public Guid UserId { get; set; }
    public Guid TransactionId { get; set; }
}
