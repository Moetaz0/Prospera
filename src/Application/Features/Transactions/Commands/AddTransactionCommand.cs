using MediatR;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Transactions.Commands;

/// <summary>
/// Command to record a new transaction for a user
/// </summary>
public class AddTransactionCommand : IRequest<TransactionDto>
{
    public required Guid UserId { get; set; }
    public required decimal Amount { get; set; }
    public required string Type { get; set; } // TransactionType as string
    public required string Description { get; set; }
}
