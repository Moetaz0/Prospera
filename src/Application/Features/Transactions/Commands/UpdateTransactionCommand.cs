using MediatR;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Transactions.Commands;

public class UpdateTransactionCommand : IRequest<TransactionDto?>
{
    public Guid UserId { get; set; }
    public Guid TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
