using AutoMapper;
using MediatR;
using Prospera.Application.Common.Interfaces;
using Prospera.Application.DTOs;
using Prospera.Domain.Entities;
using Prospera.Domain.Enums;
using Prospera.Domain.Interfaces;

namespace Prospera.Application.Features.Transactions.Commands;

/// <summary>
/// Handler for SyncStripeTransactionsCommand
/// Fetches transactions from Stripe and creates Transaction records
/// </summary>
public class SyncStripeTransactionsCommandHandler : IRequestHandler<SyncStripeTransactionsCommand, IEnumerable<TransactionDto>>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IStripeService _stripeService;
    private readonly IMapper _mapper;

    public SyncStripeTransactionsCommandHandler(
        ITransactionRepository transactionRepository,
        IUserRepository userRepository,
        IStripeService stripeService,
        IMapper mapper)
    {
        _transactionRepository = transactionRepository;
        _userRepository = userRepository;
        _stripeService = stripeService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TransactionDto>> Handle(SyncStripeTransactionsCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.StripeAccountId))
        {
            throw new InvalidOperationException("No Stripe account connected. Please provide a Stripe account ID.");
        }

        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var stripeKey = request.StripeAccountId.Trim();
        var existingTransactions = (await _transactionRepository.GetByUserIdAsync(request.UserId, 0, 500)).ToList();

        var stripeBalanceTransactions = await _stripeService.GetBalanceTransactionsAsync(stripeKey, 100, cancellationToken);
        var stripePaymentIntents = await _stripeService.GetPaymentIntentTransactionsAsync(stripeKey, 50, cancellationToken);
        var mergedStripeTransactions = stripeBalanceTransactions
            .Concat(stripePaymentIntents)
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToList();

        var synced = new List<Transaction>();
        foreach (var item in mergedStripeTransactions)
        {
            var amount = Math.Abs(item.Amount) / 100m;
            if (amount <= 0)
            {
                continue;
            }

            var transactionType = MapStripeTypeToTransactionType(item.Type, item.Amount);
            var description = BuildDescription(item);

            var alreadyExists = existingTransactions.Any(t =>
                t.Description == description &&
                t.Amount == amount &&
                t.Type == transactionType &&
                Math.Abs((t.Date - item.CreatedAtUtc).TotalMinutes) < 2);

            if (alreadyExists)
            {
                continue;
            }

            var transaction = new Transaction(amount, transactionType, description, request.UserId);
            await _transactionRepository.AddAsync(transaction);
            synced.Add(transaction);
        }

        return _mapper.Map<IEnumerable<TransactionDto>>(synced);
    }

    private static TransactionType MapStripeTypeToTransactionType(string? stripeType, long signedAmount)
    {
        var normalized = (stripeType ?? string.Empty).ToLowerInvariant();

        if (normalized.Contains("fee") || normalized.Contains("refund") || signedAmount < 0)
        {
            return TransactionType.Expense;
        }

        if (normalized.Contains("transfer") || normalized.Contains("payout") || normalized.Contains("charge") || normalized.Contains("payment"))
        {
            return TransactionType.Income;
        }

        return TransactionType.Investment;
    }

    private static string BuildDescription(StripeTransactionData tx)
    {
        var detail = string.IsNullOrWhiteSpace(tx.Description) ? tx.Type : tx.Description;
        return $"Stripe {tx.ExternalId}: {detail}";
    }
}
