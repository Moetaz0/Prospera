using MediatR;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Transactions.Queries;

public class GetUserTransactionsQuery : IRequest<IEnumerable<TransactionDto>>
{
    public Guid UserId { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 50;
}
