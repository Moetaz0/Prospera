using Microsoft.EntityFrameworkCore;
using Prospera.Domain.Entities;
using Prospera.Domain.Interfaces;

namespace Prospera.Infrastructure.Persistence.Repositories;

public class LiabilityRepository : ILiabilityRepository
{
    private readonly ApplicationDbContext _context;

    public LiabilityRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Liability>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Liabilities
            .Where(l => l.UserId == userId)
            .ToListAsync();
    }

    public async Task AddAsync(Liability liability)
    {
        await _context.Liabilities.AddAsync(liability);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Liability liability)
    {
        _context.Liabilities.Update(liability);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var liability = await _context.Liabilities.FindAsync(id);
        if (liability != null)
        {
            _context.Liabilities.Remove(liability);
            await _context.SaveChangesAsync();
        }
    }
}
