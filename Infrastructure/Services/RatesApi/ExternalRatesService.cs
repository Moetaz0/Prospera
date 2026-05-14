using Microsoft.Extensions.Caching.Memory;
using Prospera.Application.Common.Interfaces;
using Prospera.Domain.Interfaces;
using System.Text.Json.Nodes;

namespace Prospera.Infrastructure.Services.RatesApi;

/// <summary>
/// External rates service - fetches inflation from World Bank, keeps car/RE rates hardcoded
/// Supports admin overrides and caching
/// </summary>
public class ExternalRatesService : IExternalRatesService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ICustomFinancialRateRepository _customRatesRepository;
    private const string INFLATION_CACHE_KEY = "inflation_{0}";
    private const int CACHE_DURATION_DAYS = 90; // World Bank updates quarterly

    // Hardcoded regional defaults
    private static readonly Dictionary<string, (decimal Car, decimal RealEstate)> RegionalDefaults = new(StringComparer.OrdinalIgnoreCase)
    {
        // Africa
        { "TN", (15.0m, 2.0m) },

        // North America
        { "US", (15.0m, 3.5m) },
        { "CA", (15.0m, 3.2m) },
        { "MX", (16.0m, 3.5m) },

        // South America
        { "BR", (16.0m, 4.0m) },

        // Europe
        { "DE", (14.0m, 3.0m) },
        { "FR", (14.0m, 2.8m) },
        { "GB", (14.0m, 3.2m) },
        { "IT", (14.0m, 2.5m) },
        { "ES", (14.0m, 2.7m) },
        { "NL", (14.0m, 3.1m) },
        { "BE", (14.0m, 2.9m) },
        { "CH", (13.0m, 2.8m) },
        { "SE", (14.0m, 3.3m) },
        { "NO", (14.0m, 3.0m) },

        // Oceania
        { "AU", (15.0m, 4.0m) },
        { "NZ", (15.0m, 3.5m) },

        // Asia
        { "JP", (13.0m, 1.5m) },
        { "SG", (15.0m, 3.2m) },
        { "IN", (15.0m, 5.0m) },

        // Middle East
        { "AE", (15.0m, 4.0m) }
    };

    // Hardcoded default inflation rates (fallback)
    private static readonly Dictionary<string, decimal> DefaultInflationRates = new(StringComparer.OrdinalIgnoreCase)
    {
        { "TN", 7.0m },
        { "US", 3.5m },
        { "CA", 3.0m },
        { "MX", 4.5m },
        { "BR", 5.5m },
        { "DE", 2.5m },
        { "FR", 2.4m },
        { "GB", 3.0m },
        { "IT", 2.6m },
        { "ES", 2.5m },
        { "NL", 2.3m },
        { "BE", 2.4m },
        { "CH", 1.8m },
        { "SE", 2.7m },
        { "NO", 2.5m },
        { "AU", 3.8m },
        { "NZ", 3.4m },
        { "JP", 2.1m },
        { "SG", 2.2m },
        { "IN", 4.8m },
        { "AE", 2.0m }
    };

    public ExternalRatesService(
        HttpClient httpClient,
        IMemoryCache cache,
        ICustomFinancialRateRepository customRatesRepository)
    {
        _httpClient = httpClient;
        _cache = cache;
        _customRatesRepository = customRatesRepository;
    }

    /// <summary>
    /// Fetch inflation from World Bank API with caching and fallback
    /// </summary>
    public async Task<decimal> GetInflationRateAsync(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return 3.0m; // Global default
        }

        // Step 1: Check for admin override
        var customRates = await _customRatesRepository.GetByCountryCodeAsync(countryCode);
        if (customRates?.IsValidAndActive == true && customRates.InflationRate.HasValue)
        {
            return customRates.InflationRate.Value;
        }

        // Step 2: Check cache
        var cacheKey = string.Format(INFLATION_CACHE_KEY, countryCode.ToUpper());
        if (_cache.TryGetValue(cacheKey, out decimal cachedRate))
        {
            return cachedRate;
        }

        // Step 3: Fetch from World Bank API
        try
        {
            var rate = await FetchFromWorldBankAsync(countryCode);
            if (rate.HasValue && rate.Value > 0)
            {
                // Cache for 90 days
                _cache.Set(cacheKey, rate.Value, TimeSpan.FromDays(CACHE_DURATION_DAYS));
                return rate.Value;
            }
        }
        catch (Exception ex)
        {
            // Log error in production: _logger.LogWarning($"Failed to fetch inflation from World Bank for {countryCode}: {ex.Message}");
        }

        // Step 4: Return hardcoded default
        return DefaultInflationRates.TryGetValue(countryCode, out var defaultRate)
            ? defaultRate
            : 3.0m; // Global default
    }

    /// <summary>
    /// Get car depreciation rate (hardcoded by region)
    /// </summary>
    public async Task<decimal> GetCarDepreciationRate(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return 14.5m; // Global average
        }

        // Check for admin override
        var customRates = await _customRatesRepository.GetByCountryCodeAsync(countryCode);
        if (customRates?.IsValidAndActive == true && customRates.CarDepreciationRate.HasValue)
        {
            return customRates.CarDepreciationRate.Value;
        }

        // Return regional default
        if (RegionalDefaults.TryGetValue(countryCode, out var rates))
        {
            return rates.Car;
        }

        return 14.5m; // Global average
    }

    /// <summary>
    /// Get real estate appreciation rate (hardcoded by region)
    /// </summary>
    public async Task<decimal> GetRealEstateAppreciationRate(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return 2.5m; // Global average
        }

        // Check for admin override
        var customRates = await _customRatesRepository.GetByCountryCodeAsync(countryCode);
        if (customRates?.IsValidAndActive == true && customRates.RealEstateAppreciationRate.HasValue)
        {
            return customRates.RealEstateAppreciationRate.Value;
        }

        // Return regional default
        if (RegionalDefaults.TryGetValue(countryCode, out var rates))
        {
            return rates.RealEstate;
        }

        return 2.5m; // Global average
    }

    /// <summary>
    /// Set custom rates for a country (admin feature)
    /// </summary>
    public async Task SetCustomRatesAsync(string countryCode, decimal? inflationRate, decimal? carDepreciation, decimal? reAppreciation)
    {
        var existing = await _customRatesRepository.GetByCountryCodeAsync(countryCode);
        
        if (existing != null)
        {
            existing.SetRates(inflationRate, carDepreciation, reAppreciation);
            await _customRatesRepository.AddAsync(existing);
        }
        else
        {
            var newRate = new Prospera.Domain.Entities.CustomFinancialRate(countryCode, countryCode);
            newRate.SetRates(inflationRate, carDepreciation, reAppreciation);
            await _customRatesRepository.AddAsync(newRate);
        }

        // Invalidate cache
        InvalidateInflationCache(countryCode);
    }

    /// <summary>
    /// Check if custom rates exist for a country
    /// </summary>
    public async Task<bool> HasCustomRatesAsync(string countryCode)
    {
        var rates = await _customRatesRepository.GetByCountryCodeAsync(countryCode);
        return rates?.IsValidAndActive == true;
    }

    /// <summary>
    /// Get custom rates for a country
    /// </summary>
    public async Task<CustomRates?> GetCustomRatesAsync(string countryCode)
    {
        var rates = await _customRatesRepository.GetByCountryCodeAsync(countryCode);
        
        if (rates?.IsValidAndActive != true)
        {
            return null;
        }

        return new CustomRates
        {
            CountryCode = rates.CountryCode,
            InflationRate = rates.InflationRate,
            CarDepreciationRate = rates.CarDepreciationRate,
            RealEstateAppreciationRate = rates.RealEstateAppreciationRate,
            SetAt = rates.SetAt,
            SetByAdminId = rates.SetByAdminId?.ToString(),
            Reason = rates.Reason
        };
    }

    /// <summary>
    /// Clear custom rates and revert to defaults
    /// </summary>
    public async Task ClearCustomRatesAsync(string countryCode)
    {
        var rates = await _customRatesRepository.GetByCountryCodeAsync(countryCode);
        if (rates != null)
        {
            rates.Clear();
            await _customRatesRepository.AddAsync(rates);
        }

        // Invalidate cache
        InvalidateInflationCache(countryCode);
    }

    /// <summary>
    /// Refresh inflation rates from World Bank (called by background job)
    /// </summary>
    public async Task RefreshInflationRatesAsync(string? countryCodeFilter = null)
    {
        var countriesToUpdate = countryCodeFilter != null
            ? new[] { countryCodeFilter }
            : DefaultInflationRates.Keys.ToArray();

        foreach (var countryCode in countriesToUpdate)
        {
            try
            {
                // Skip if custom rates are set (don't override admin choices)
                var customRates = await _customRatesRepository.GetByCountryCodeAsync(countryCode);
                if (customRates?.IsValidAndActive == true && customRates.InflationRate.HasValue)
                {
                    continue;
                }

                // Fetch from World Bank
                var rate = await FetchFromWorldBankAsync(countryCode);
                if (rate.HasValue && rate.Value > 0)
                {
                    // Update cache
                    var cacheKey = string.Format(INFLATION_CACHE_KEY, countryCode.ToUpper());
                    _cache.Set(cacheKey, rate.Value, TimeSpan.FromDays(CACHE_DURATION_DAYS));
                }
            }
            catch (Exception ex)
            {
                // Log error: _logger.LogWarning($"Failed to refresh inflation for {countryCode}: {ex.Message}");
                // Continue with other countries
            }
        }
    }

    /// <summary>
    /// Get all current rates with source information
    /// </summary>
    public async Task<IEnumerable<CurrentRates>> GetAllCurrentRatesAsync()
    {
        var result = new List<CurrentRates>();

        foreach (var countryCode in DefaultInflationRates.Keys)
        {
            var customRates = await _customRatesRepository.GetByCountryCodeAsync(countryCode);
            var inflation = await GetInflationRateAsync(countryCode);
            var carDepr = await GetCarDepreciationRate(countryCode);
            var reAppr = await GetRealEstateAppreciationRate(countryCode);

            // Determine sources
            var inflationSource = customRates?.IsValidAndActive == true && customRates.InflationRate.HasValue
                ? InflationSource.AdminOverride
                : _cache.TryGetValue(string.Format(INFLATION_CACHE_KEY, countryCode.ToUpper()), out _)
                    ? InflationSource.Cached
                    : InflationSource.Hardcoded;

            var carDeprSource = customRates?.IsValidAndActive == true && customRates.CarDepreciationRate.HasValue
                ? RateSource.AdminOverride
                : RateSource.Hardcoded;

            var reApprSource = customRates?.IsValidAndActive == true && customRates.RealEstateAppreciationRate.HasValue
                ? RateSource.AdminOverride
                : RateSource.Hardcoded;

            result.Add(new CurrentRates
            {
                CountryCode = countryCode,
                CountryName = countryCode,
                Region = "Global",
                InflationRate = inflation,
                InflationSource = inflationSource,
                CarDepreciationRate = carDepr,
                CarDepreciationSource = carDeprSource,
                RealEstateAppreciationRate = reAppr,
                RealEstateAppreciationSource = reApprSource,
                LastUpdated = customRates?.SetAt ?? DateTime.UtcNow,
                LastFetchedFromApi = null
            });
        }

        return result.OrderBy(r => r.CountryCode);
    }

    // ===== Private Helpers =====

    /// <summary>
    /// Fetch inflation rate from World Bank API
    /// </summary>
    private async Task<decimal?> FetchFromWorldBankAsync(string countryCode)
    {
        try
        {
            var url = $"https://api.worldbank.org/v2/country/{countryCode}/indicator/FP.CPI.TOTL.ZG?format=json";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var jsonNode = JsonNode.Parse(json);

            // World Bank API returns: [metadata, [data items...]]
            var dataArray = jsonNode?[1];

            if (dataArray == null)
            {
                return null;
            }

            // Find most recent non-null value
            foreach (var item in dataArray.AsArray())
            {
                if (item?["value"]?.GetValue<string>() is string valueStr &&
                    !string.IsNullOrEmpty(valueStr) &&
                    decimal.TryParse(valueStr, out var value) &&
                    value > 0)
                {
                    return value;
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Clear inflation cache for a country
    /// </summary>
    private void InvalidateInflationCache(string countryCode)
    {
        var cacheKey = string.Format(INFLATION_CACHE_KEY, countryCode.ToUpper());
        _cache.Remove(cacheKey);
    }
}
