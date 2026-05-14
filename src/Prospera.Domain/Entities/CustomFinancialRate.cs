using Prospera.Domain.Common;

namespace Prospera.Domain.Entities;

/// <summary>
/// Stores custom/override financial rates for countries
/// Used by admins to manually update rates or make corrections
/// </summary>
public class CustomFinancialRate : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// ISO 3166-1 alpha-2 country code (e.g., "US", "DE", "TN")
    /// </summary>
    public string CountryCode { get; private set; }

    /// <summary>
    /// Country name (e.g., "Tunisia", "United States")
    /// </summary>
    public string CountryName { get; private set; }

    /// <summary>
    /// Override inflation rate (if null, use World Bank or default)
    /// </summary>
    public decimal? InflationRate { get; private set; }

    /// <summary>
    /// Override car depreciation rate (if null, use regional default)
    /// </summary>
    public decimal? CarDepreciationRate { get; private set; }

    /// <summary>
    /// Override real estate appreciation rate (if null, use regional default)
    /// </summary>
    public decimal? RealEstateAppreciationRate { get; private set; }

    /// <summary>
    /// When this override was set
    /// </summary>
    public DateTime SetAt { get; private set; }

    /// <summary>
    /// Admin user ID who set this override
    /// </summary>
    public Guid? SetByAdminId { get; private set; }

    /// <summary>
    /// Reason for the override (e.g., "War in region", "Economic crisis")
    /// </summary>
    public string? Reason { get; private set; }

    /// <summary>
    /// When this override expires (if applicable)
    /// </summary>
    public DateTime? ExpiresAt { get; private set; }

    /// <summary>
    /// Is this override currently active?
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Constructor for new custom rate
    /// </summary>
    public CustomFinancialRate(string countryCode, string countryName)
    {
        CountryCode = countryCode;
        CountryName = countryName;
        SetAt = DateTime.UtcNow;
        IsActive = true;
    }

    /// <summary>
    /// Set override rates
    /// </summary>
    public void SetRates(
        decimal? inflationRate,
        decimal? carDepreciationRate,
        decimal? realEstateAppreciationRate,
        Guid? adminId = null,
        string? reason = null,
        DateTime? expiresAt = null)
    {
        InflationRate = inflationRate;
        CarDepreciationRate = carDepreciationRate;
        RealEstateAppreciationRate = realEstateAppreciationRate;
        SetByAdminId = adminId;
        Reason = reason;
        ExpiresAt = expiresAt;
        SetAt = DateTime.UtcNow;
        IsActive = true;
    }

    /// <summary>
    /// Clear the override (revert to defaults)
    /// </summary>
    public void Clear()
    {
        InflationRate = null;
        CarDepreciationRate = null;
        RealEstateAppreciationRate = null;
        SetByAdminId = null;
        Reason = null;
        ExpiresAt = null;
        IsActive = false;
    }

    /// <summary>
    /// Check if override has expired
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt < DateTime.UtcNow;

    /// <summary>
    /// Check if override is valid and active
    /// </summary>
    public bool IsValidAndActive => IsActive && !IsExpired;
}
