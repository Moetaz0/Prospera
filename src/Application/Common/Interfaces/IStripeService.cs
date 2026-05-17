namespace Prospera.Application.Common.Interfaces;

public record StripeTransactionData(
    string ExternalId,
    long Amount,
    string Currency,
    string Type,
    string? Description,
    DateTime CreatedAtUtc);

public interface IStripeService
{
    Task<string> CreateConnectAccountLinkAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> VerifyAccountAsync(string stripeAccountId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StripeTransactionData>> GetBalanceTransactionsAsync(
        string stripeAccountId,
        int limit = 100,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StripeTransactionData>> GetPaymentIntentTransactionsAsync(
        string stripeAccountId,
        int limit = 50,
        CancellationToken cancellationToken = default);
}
