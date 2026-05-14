using Prospera.Application.Common.Interfaces;

namespace Prospera.Infrastructure.Services.LocationBased;

/// <summary>
/// Service providing location-based financial rates
/// Handles country-specific inflation, car depreciation, and real estate appreciation rates
/// </summary>
public class LocationBasedRatesService : ILocationBasedRatesService
{
    private static readonly Dictionary<string, LocationBasedRates> CountryRates = new(StringComparer.OrdinalIgnoreCase)
    {
        // Africa
        {
            "TN", new LocationBasedRates
            {
                CountryCode = "TN",
                CountryName = "Tunisia",
                Region = "Africa",
                InflationRate = 7.0m,
                CarDepreciationRate = 15.0m,
                RealEstateAppreciationRate = 2.0m,
                LastUpdated = DateTime.UtcNow
            }
        },

        // North America
        {
            "US", new LocationBasedRates
            {
                CountryCode = "US",
                CountryName = "United States",
                Region = "North America",
                InflationRate = 3.5m,
                CarDepreciationRate = 15.0m,
                RealEstateAppreciationRate = 3.5m,
                LastUpdated = DateTime.UtcNow
            }
        },
        {
            "CA", new LocationBasedRates
            {
                CountryCode = "CA",
                CountryName = "Canada",
                Region = "North America",
                InflationRate = 3.0m,
                CarDepreciationRate = 15.0m,
                RealEstateAppreciationRate = 3.2m,
                LastUpdated = DateTime.UtcNow
            }
        },

        // Europe
        {
            "DE", new LocationBasedRates
            {
                CountryCode = "DE",
                CountryName = "Germany",
                Region = "Europe",
                InflationRate = 2.5m,
                CarDepreciationRate = 14.0m,
                RealEstateAppreciationRate = 3.0m,
                LastUpdated = DateTime.UtcNow
            }
        },
        {
            "FR", new LocationBasedRates
            {
                CountryCode = "FR",
                CountryName = "France",
                Region = "Europe",
                InflationRate = 2.4m,
                CarDepreciationRate = 14.0m,
                RealEstateAppreciationRate = 2.8m,
                LastUpdated = DateTime.UtcNow
            }
        },
        {
            "GB", new LocationBasedRates
            {
                CountryCode = "GB",
                CountryName = "United Kingdom",
                Region = "Europe",
                InflationRate = 3.0m,
                CarDepreciationRate = 14.0m,
                RealEstateAppreciationRate = 3.2m,
                LastUpdated = DateTime.UtcNow
            }
        },
        {
            "IT", new LocationBasedRates
            {
                CountryCode = "IT",
                CountryName = "Italy",
                Region = "Europe",
                InflationRate = 2.6m,
                CarDepreciationRate = 14.0m,
                RealEstateAppreciationRate = 2.5m,
                LastUpdated = DateTime.UtcNow
            }
        },
        {
            "ES", new LocationBasedRates
            {
                CountryCode = "ES",
                CountryName = "Spain",
                Region = "Europe",
                InflationRate = 2.5m,
                CarDepreciationRate = 14.0m,
                RealEstateAppreciationRate = 2.7m,
                LastUpdated = DateTime.UtcNow
            }
        },
        {
            "NL", new LocationBasedRates
            {
                CountryCode = "NL",
                CountryName = "Netherlands",
                Region = "Europe",
                InflationRate = 2.3m,
                CarDepreciationRate = 14.0m,
                RealEstateAppreciationRate = 3.1m,
                LastUpdated = DateTime.UtcNow
            }
        },
        {
            "BE", new LocationBasedRates
            {
                CountryCode = "BE",
                CountryName = "Belgium",
                Region = "Europe",
                InflationRate = 2.4m,
                CarDepreciationRate = 14.0m,
                RealEstateAppreciationRate = 2.9m,
                LastUpdated = DateTime.UtcNow
            }
        },
        {
            "CH", new LocationBasedRates
            {
                CountryCode = "CH",
                CountryName = "Switzerland",
                Region = "Europe",
                InflationRate = 1.8m,
                CarDepreciationRate = 13.0m,
                RealEstateAppreciationRate = 2.8m,
                LastUpdated = DateTime.UtcNow
            }
        },
        {
            "SE", new LocationBasedRates
            {
                CountryCode = "SE",
                CountryName = "Sweden",
                Region = "Europe",
                InflationRate = 2.7m,
                CarDepreciationRate = 14.0m,
                RealEstateAppreciationRate = 3.3m,
                LastUpdated = DateTime.UtcNow
            }
        },
        {
            "NO", new LocationBasedRates
            {
                CountryCode = "NO",
                CountryName = "Norway",
                Region = "Europe",
                InflationRate = 2.5m,
                CarDepreciationRate = 14.0m,
                RealEstateAppreciationRate = 3.0m,
                LastUpdated = DateTime.UtcNow
            }
        },
        {
            "AU", new LocationBasedRates
            {
                CountryCode = "AU",
                CountryName = "Australia",
                Region = "Oceania",
                InflationRate = 3.8m,
                CarDepreciationRate = 15.0m,
                RealEstateAppreciationRate = 4.0m,
                LastUpdated = DateTime.UtcNow
            }
        },
        {
            "NZ", new LocationBasedRates
            {
                CountryCode = "NZ",
                CountryName = "New Zealand",
                Region = "Oceania",
                InflationRate = 3.4m,
                CarDepreciationRate = 15.0m,
                RealEstateAppreciationRate = 3.5m,
                LastUpdated = DateTime.UtcNow
            }
        },
        {
            "SG", new LocationBasedRates
            {
                CountryCode = "SG",
                CountryName = "Singapore",
                Region = "Asia",
                InflationRate = 2.2m,
                CarDepreciationRate = 15.0m,
                RealEstateAppreciationRate = 3.2m,
                LastUpdated = DateTime.UtcNow
            }
        },
        {
            "JP", new LocationBasedRates
            {
                CountryCode = "JP",
                CountryName = "Japan",
                Region = "Asia",
                InflationRate = 2.1m,
                CarDepreciationRate = 13.0m,
                RealEstateAppreciationRate = 1.5m,
                LastUpdated = DateTime.UtcNow
            }
        },
        {
            "AE", new LocationBasedRates
            {
                CountryCode = "AE",
                CountryName = "United Arab Emirates",
                Region = "Middle East",
                InflationRate = 2.0m,
                CarDepreciationRate = 15.0m,
                RealEstateAppreciationRate = 4.0m,
                LastUpdated = DateTime.UtcNow
            }
        },
        {
            "MX", new LocationBasedRates
            {
                CountryCode = "MX",
                CountryName = "Mexico",
                Region = "North America",
                InflationRate = 4.5m,
                CarDepreciationRate = 16.0m,
                RealEstateAppreciationRate = 3.5m,
                LastUpdated = DateTime.UtcNow
            }
        },
        {
            "BR", new LocationBasedRates
            {
                CountryCode = "BR",
                CountryName = "Brazil",
                Region = "South America",
                InflationRate = 5.5m,
                CarDepreciationRate = 16.0m,
                RealEstateAppreciationRate = 4.0m,
                LastUpdated = DateTime.UtcNow
            }
        },
        {
            "IN", new LocationBasedRates
            {
                CountryCode = "IN",
                CountryName = "India",
                Region = "Asia",
                InflationRate = 4.8m,
                CarDepreciationRate = 15.0m,
                RealEstateAppreciationRate = 5.0m,
                LastUpdated = DateTime.UtcNow
            }
        }
    };

    /// <summary>
    /// Get rates by ISO country code
    /// </summary>
    public LocationBasedRates GetRatesByCountry(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            throw new ArgumentException("Country code cannot be null or empty", nameof(countryCode));
        }

        if (CountryRates.TryGetValue(countryCode, out var rates))
        {
            return rates;
        }

        throw new KeyNotFoundException($"Country code '{countryCode}' is not supported. Use GetAllAvailableLocations() to see supported countries.");
    }

    /// <summary>
    /// Get rates by country name
    /// </summary>
    public LocationBasedRates GetRatesByCountryName(string countryName)
    {
        if (string.IsNullOrWhiteSpace(countryName))
        {
            throw new ArgumentException("Country name cannot be null or empty", nameof(countryName));
        }

        var rates = CountryRates.Values.FirstOrDefault(r =>
            r.CountryName.Equals(countryName, StringComparison.OrdinalIgnoreCase));

        if (rates != null)
        {
            return rates;
        }

        throw new KeyNotFoundException($"Country '{countryName}' not found. Use GetAllAvailableLocations() to see supported countries.");
    }

    /// <summary>
    /// Get all available locations
    /// </summary>
    public IEnumerable<LocationRateMapping> GetAllAvailableLocations()
    {
        return CountryRates.Values
            .OrderBy(r => r.Region)
            .ThenBy(r => r.CountryName)
            .Select(r => new LocationRateMapping
            {
                CountryCode = r.CountryCode,
                CountryName = r.CountryName,
                Region = r.Region,
                InflationRate = r.InflationRate,
                CarDepreciationRate = r.CarDepreciationRate,
                RealEstateAppreciationRate = r.RealEstateAppreciationRate
            });
    }

    /// <summary>
    /// Check if country code is supported
    /// </summary>
    public bool IsCountrySupported(string countryCode)
    {
        return !string.IsNullOrWhiteSpace(countryCode) && CountryRates.ContainsKey(countryCode);
    }
}
