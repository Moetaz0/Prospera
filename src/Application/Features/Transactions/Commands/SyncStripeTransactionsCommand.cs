using MediatR;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Transactions.Commands;

/// <summary>
/// Command to sync transactions from Stripe for a user
/// This automatically fetches recent transactions from Stripe Connect
/// </summary>
public class SyncStripeTransactionsCommand : IRequest<IEnumerable<TransactionDto>>
{
    public required Guid UserId { get; set; }
    public string? StripeAccountId { get; set; }
}
