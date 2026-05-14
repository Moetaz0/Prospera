using Prospera.Domain.Entities;

namespace Prospera.Domain.Interfaces;

/// <summary>
/// Repository for custom financial rates
/// </summary>
public interface ICustomFinancialRateRepository
{
    /// <summary>
    /// Get custom rates by country code
    /// </summary>
    Task<CustomFinancialRate?> GetByCountryCodeAsync(string countryCode);

    /// <summary>
    /// Get all active custom rates
    /// </summary>
    Task<IEnumerable<CustomFinancialRate>> GetAllActiveAsync();

    /// <summary>
    /// Add or update custom rates
    /// </summary>
    Task AddAsync(CustomFinancialRate rate);

    /// <summary>
    /// Delete custom rates for a country
    /// </summary>
    Task DeleteAsync(string countryCode);

    /// <summary>
    /// Get all custom rates (including expired)
    /// </summary>
    Task<IEnumerable<CustomFinancialRate>> GetAllAsync();
}
