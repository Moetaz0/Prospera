namespace Prospera.Application.Common.Interfaces;

/// <summary>
/// Service for fetching and managing financial rates from external sources
/// Primary source: World Bank API for inflation
/// Fallback: Hardcoded regional rates
/// </summary>
public interface IExternalRatesService
{
    /// <summary>
    /// Fetch inflation rate for a country from World Bank API
    /// Cached for 90 days (quarterly updates from World Bank)
    /// </summary>
    /// <param name="countryCode">ISO 3166-1 alpha-2 country code</param>
    /// <returns>Inflation rate as percentage (e.g., 7.0 for 7%)</returns>
    Task<decimal> GetInflationRateAsync(string countryCode);

    /// <summary>
    /// Get car depreciation rate (hardcoded by region)
    /// </summary>
    Task<decimal> GetCarDepreciationRate(string countryCode);

    /// <summary>
    /// Get real estate appreciation rate (hardcoded by region)
    /// </summary>
    Task<decimal> GetRealEstateAppreciationRate(string countryCode);

    /// <summary>
    /// Manually override rates for a country (admin feature)
    /// </summary>
    Task SetCustomRatesAsync(string countryCode, decimal? inflationRate, decimal? carDepreciation, decimal? reAppreciation);

    /// <summary>
    /// Check if custom rates are set for a country
    /// </summary>
    Task<bool> HasCustomRatesAsync(string countryCode);

    /// <summary>
    /// Get custom rates for a country (if set)
    /// </summary>
    Task<CustomRates?> GetCustomRatesAsync(string countryCode);

    /// <summary>
    /// Clear custom rates and revert to defaults
    /// </summary>
    Task ClearCustomRatesAsync(string countryCode);

    /// <summary>
    /// Force refresh rates from World Bank (called by background job)
    /// </summary>
    Task RefreshInflationRatesAsync(string? countryCodeFilter = null);

    /// <summary>
    /// Get all current rates (including custom overrides) for display
    /// </summary>
    Task<IEnumerable<CurrentRates>> GetAllCurrentRatesAsync();
}

/// <summary>
/// Custom rate override set by admin
/// </summary>
public class CustomRates
{
    public string CountryCode { get; set; } = string.Empty;
    public decimal? InflationRate { get; set; }
    public decimal? CarDepreciationRate { get; set; }
    public decimal? RealEstateAppreciationRate { get; set; }
    public DateTime SetAt { get; set; }
    public string? SetByAdminId { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Current rates for a country (with sources)
/// </summary>
public class CurrentRates
{
    public string CountryCode { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    
    // Inflation with source
    public decimal InflationRate { get; set; }
    public InflationSource InflationSource { get; set; }
    
    // Car depreciation
    public decimal CarDepreciationRate { get; set; }
    public RateSource CarDepreciationSource { get; set; }
    
    // Real estate appreciation
    public decimal RealEstateAppreciationRate { get; set; }
    public RateSource RealEstateAppreciationSource { get; set; }
    
    // Metadata
    public DateTime LastUpdated { get; set; }
    public DateTime? LastFetchedFromApi { get; set; }
}

/// <summary>
/// Source of inflation rate
/// </summary>
public enum InflationSource
{
    WorldBankApi = 0,      // Fetched from World Bank API
    AdminOverride = 1,     // Manually set by admin
    Hardcoded = 2,         // Default hardcoded value
    Cached = 3             // From cache (90-day TTL)
}

/// <summary>
/// Source of other rates
/// </summary>
public enum RateSource
{
    AdminOverride = 0,  // Manually set by admin
    Hardcoded = 1       // Default hardcoded value by region
}
