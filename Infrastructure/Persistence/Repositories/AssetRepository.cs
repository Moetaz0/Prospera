using Microsoft.EntityFrameworkCore;
using Prospera.Domain.Entities;
using Prospera.Domain.Interfaces;

namespace Prospera.Infrastructure.Persistence.Repositories;

public class AssetRepository : IAssetRepository
{
    private readonly ApplicationDbContext _context;

    public AssetRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Asset>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Assets
            .Where(a => a.UserId == userId)
            .ToListAsync();
    }

    public async Task<Asset?> GetByIdAsync(Guid id, Guid userId)
    {
        return await _context.Assets
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
    }

    public async Task AddAsync(Asset asset)
    {
        await _context.Assets.AddAsync(asset);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Asset asset)
    {
        _context.Assets.Update(asset);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset != null)
        {
            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();
        }
    }
}
