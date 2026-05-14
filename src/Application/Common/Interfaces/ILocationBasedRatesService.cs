namespace Prospera.Application.Common.Interfaces;

/// <summary>
/// Location-based financial rates (inflation, depreciation, appreciation)
/// </summary>
public interface ILocationBasedRatesService
{
    /// <summary>
    /// Get location-based default rates
    /// </summary>
    /// <param name="countryCode">ISO country code (e.g., "US", "TN", "DE")</param>
    /// <returns>Location-specific rates for inflation, car depreciation, and real estate appreciation</returns>
    LocationBasedRates GetRatesByCountry(string countryCode);

    /// <summary>
    /// Get rates by country name
    /// </summary>
    LocationBasedRates GetRatesByCountryName(string countryName);

    /// <summary>
    /// Get all available countries with their rates
    /// </summary>
    IEnumerable<LocationRateMapping> GetAllAvailableLocations();

    /// <summary>
    /// Check if a country code is supported
    /// </summary>
    bool IsCountrySupported(string countryCode);
}

/// <summary>
/// Rates specific to a location
/// </summary>
public class LocationBasedRates
{
    /// <summary>
    /// Country code (ISO 3166-1 alpha-2)
    /// </summary>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>
    /// Country name
    /// </summary>
    public string CountryName { get; set; } = string.Empty;

    /// <summary>
    /// Region/Continent
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Annual inflation rate (%)
    /// </summary>
    public decimal InflationRate { get; set; }

    /// <summary>
    /// Annual car depreciation rate (%)
    /// </summary>
    public decimal CarDepreciationRate { get; set; }

    /// <summary>
    /// Annual real estate appreciation rate (%)
    /// </summary>
    public decimal RealEstateAppreciationRate { get; set; }

    /// <summary>
    /// Last updated date
    /// </summary>
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Mapping of country code to rates
/// </summary>
public class LocationRateMapping
{
    public string CountryCode { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public decimal InflationRate { get; set; }
    public decimal CarDepreciationRate { get; set; }
    public decimal RealEstateAppreciationRate { get; set; }
}
