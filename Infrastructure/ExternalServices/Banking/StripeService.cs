using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prospera.Application.Common.Interfaces;
using Stripe;

namespace Prospera.Infrastructure.ExternalServices.Banking;



public class StripeService : Prospera.Application.Common.Interfaces.IStripeService
{
    private readonly ILogger<StripeService> _logger;
    private readonly string _secretKey;
    private readonly string _frontendBaseUrl;

    public StripeService(IConfiguration configuration, ILogger<StripeService> logger)
    {
        _logger = logger;
        _secretKey = configuration["Stripe:SecretKey"] ?? string.Empty;
        _frontendBaseUrl = (configuration["Frontend:BaseUrl"] ?? "http://localhost:4200").TrimEnd('/');

        if (string.IsNullOrWhiteSpace(_secretKey))
        {
            _logger.LogWarning("Stripe secret key is not configured. Stripe sync/connect operations will fail.");
        }

        StripeConfiguration.ApiKey = _secretKey;
    }

    public async Task<string> CreateConnectAccountLinkAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            EnsureConfigured();

            var accountService = new AccountService();
            var account = await accountService.CreateAsync(new AccountCreateOptions
            {
                Type = "express"
            }, cancellationToken: cancellationToken);

            var accountLinkService = new AccountLinkService();
            var accountLink = await accountLinkService.CreateAsync(new AccountLinkCreateOptions
            {
                Account = account.Id,
                RefreshUrl = $"{_frontendBaseUrl}/profile?stripe=refresh",
                ReturnUrl = $"{_frontendBaseUrl}/profile?success=true&account_id={account.Id}",
                Type = "account_onboarding"
            }, cancellationToken: cancellationToken);

            return accountLink.Url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Stripe connect link for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> VerifyAccountAsync(string stripeAccountId, CancellationToken cancellationToken = default)
    {
        try
        {
            EnsureConfigured();
            if (string.IsNullOrWhiteSpace(stripeAccountId)) return false;

            var accountService = new AccountService();
            var account = await accountService.GetAsync(stripeAccountId, cancellationToken: cancellationToken);
            return account != null && !string.IsNullOrWhiteSpace(account.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying Stripe account {AccountId}", stripeAccountId);
            return false;
        }
    }

    public async Task<IReadOnlyList<StripeTransactionData>> GetBalanceTransactionsAsync(
        string stripeAccountId,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var options = new BalanceTransactionListOptions
        {
            Limit = Math.Clamp(limit, 1, 100)
        };

        var service = new BalanceTransactionService();
        var requestOptions = new RequestOptions
        {
            StripeAccount = stripeAccountId
        };

        var result = await service.ListAsync(options, requestOptions, cancellationToken);

        return result.Data.Select(tx => new StripeTransactionData(
            tx.Id,
            tx.Amount,
            tx.Currency ?? "usd",
            tx.Type ?? "balance",
            tx.Description,
            tx.Created.ToUniversalTime())).ToList();
    }

    public async Task<IReadOnlyList<StripeTransactionData>> GetPaymentIntentTransactionsAsync(
        string stripeAccountId,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var options = new PaymentIntentListOptions
        {
            Limit = Math.Clamp(limit, 1, 100)
        };

        var service = new PaymentIntentService();
        var requestOptions = new RequestOptions
        {
            StripeAccount = stripeAccountId
        };

        var result = await service.ListAsync(options, requestOptions, cancellationToken);

        return result.Data.Select(pi => new StripeTransactionData(
            pi.Id,
            pi.Amount,
            pi.Currency ?? "usd",
            "payment_intent",
            pi.Description,
            pi.Created.ToUniversalTime())).ToList();
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_secretKey))
        {
            throw new InvalidOperationException("Stripe secret key is missing. Configure Stripe:SecretKey in appsettings or user secrets.");
        }
    }
}