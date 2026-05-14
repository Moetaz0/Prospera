using Prospera.Domain.Entities;
using Prospera.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Prospera.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for custom financial rates
/// </summary>
public class CustomFinancialRateRepository : ICustomFinancialRateRepository
{
    private readonly ApplicationDbContext _context;

    public CustomFinancialRateRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomFinancialRate?> GetByCountryCodeAsync(string countryCode)
    {
        return await _context.CustomFinancialRates
            .FirstOrDefaultAsync(r => r.CountryCode == countryCode.ToUpper());
    }

    public async Task<IEnumerable<CustomFinancialRate>> GetAllActiveAsync()
    {
        return await _context.CustomFinancialRates
            .Where(r => r.IsActive && (r.ExpiresAt == null || r.ExpiresAt > DateTime.UtcNow))
            .ToListAsync();
    }

    public async Task AddAsync(CustomFinancialRate rate)
    {
        var existing = await GetByCountryCodeAsync(rate.CountryCode);

        if (existing != null)
        {
            _context.CustomFinancialRates.Remove(existing);
        }

        await _context.CustomFinancialRates.AddAsync(rate);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string countryCode)
    {
        var rate = await GetByCountryCodeAsync(countryCode);
        if (rate != null)
        {
            _context.CustomFinancialRates.Remove(rate);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<CustomFinancialRate>> GetAllAsync()
    {
        return await _context.CustomFinancialRates.ToListAsync();
    }
}
