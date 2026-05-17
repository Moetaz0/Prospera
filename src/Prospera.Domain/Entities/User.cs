using Prospera.Domain.Common;
using Prospera.Domain.Enums;
using Prospera.Domain.Events;

namespace Prospera.Domain.Entities;

public class User : BaseEntity , IAggregateRoot
{
    public string FullName { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string Role { get; private set; }

    public RiskProfile RiskProfile { get; private set; }

    /// <summary>
    /// ISO 3166-1 alpha-2 country code (e.g., "US", "DE", "TN")
    /// Used for automatic location-based financial rates
    /// </summary>
    public string? Country { get; private set; }

    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiry { get; private set; }

    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiry { get; private set; }

    /// <summary>
    /// Stripe Connect account ID for transaction syncing
    /// </summary>
    public string? StripeAccountId { get; set; }

    /// <summary>
    /// Timestamp of the last successful Stripe sync
    /// </summary>
    public DateTime? LastStripeSync { get; set; }

    private readonly List<Asset> _assets = new();
    public IReadOnlyCollection<Asset> Assets => _assets.AsReadOnly();

    private readonly List<Liability> _liabilities = new();
    public IReadOnlyCollection<Liability> Liabilities => _liabilities.AsReadOnly();

    private readonly List<Transaction> _transactions = new();
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    public User(string fullName, string email)
    {
        FullName = fullName;
        Email = email;
        PasswordHash = string.Empty;
        Role = "User";
        RiskProfile = RiskProfile.Moderate;
    }

    public User(string fullName, string email, string passwordHash, string role = "User")
    {
        FullName = fullName;
        Email = email;
        PasswordHash = passwordHash;
        Role = string.IsNullOrWhiteSpace(role) ? "User" : role;
        RiskProfile = RiskProfile.Moderate;
    }

    public void UpdateProfile(string? fullName, string? email, RiskProfile? riskProfile)
    {
        if (!string.IsNullOrWhiteSpace(fullName))
        {
            FullName = fullName;
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            Email = email;
        }

        if (riskProfile.HasValue)
        {
            RiskProfile = riskProfile.Value;
        }
    }

    /// <summary>
    /// Set the user's country for location-based financial rates
    /// </summary>
    /// <param name="countryCode">ISO 3166-1 alpha-2 country code (e.g., "US", "DE", "TN")</param>
    public void SetCountry(string? countryCode)
    {
        Country = countryCode;
    }

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
    }

    public void SetRole(string role)
    {
        if (!string.IsNullOrWhiteSpace(role))
        {
            Role = role;
        }
    }

    public void GeneratePasswordResetToken(string token, int expiryMinutes = 15)
    {
        PasswordResetToken = token;
        PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(expiryMinutes);
    }

    public bool IsPasswordResetTokenValid()
    {
        return !string.IsNullOrWhiteSpace(PasswordResetToken) && 
               PasswordResetTokenExpiry.HasValue && 
               PasswordResetTokenExpiry > DateTime.UtcNow;
    }

    public void ClearPasswordResetToken()
    {
        PasswordResetToken = null;
        PasswordResetTokenExpiry = null;
    }

    public void SetRefreshToken(string token, int expiryDays = 7)
    {
        RefreshToken = token;
        RefreshTokenExpiry = DateTime.UtcNow.AddDays(expiryDays);
    }

    public bool IsRefreshTokenValid()
    {
        return !string.IsNullOrWhiteSpace(RefreshToken) && 
               RefreshTokenExpiry.HasValue && 
               RefreshTokenExpiry > DateTime.UtcNow;
    }

    public void ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiry = null;
    }

    public void AddAsset(Asset asset)
    {
        _assets.Add(asset);
        AddDomainEvent(new AssetAddedEvent(asset));
    }

    public decimal CalculateNetWorth()
    {
        var totalAssets = _assets.Sum(a => a.CurrentValue);
        var totalLiabilities = _liabilities.Sum(l => l.Amount);
        return totalAssets - totalLiabilities;
    }
}
