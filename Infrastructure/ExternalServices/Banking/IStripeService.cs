namespace Prospera.Infrastructure.ExternalServices.Banking;

public interface IStripeService
{
    Task<string> CreateConnectAccountLinkAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> VerifyAccountAsync(string stripeAccountId, CancellationToken cancellationToken = default);
}
