using Microsoft.EntityFrameworkCore;
using Prospera.Domain.Entities;
using Prospera.Domain.Interfaces;

namespace Prospera.Infrastructure.Persistence.Repositories;

public class RealEstateEvaluationRepository : IRealEstateEvaluationRepository
{
    private readonly ApplicationDbContext _context;

    public RealEstateEvaluationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RealEstateEvaluation>> GetByUserIdAsync(Guid userId)
        => await _context.RealEstateEvaluations
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.EvaluatedAt)
            .ToListAsync();

    public async Task<IEnumerable<RealEstateEvaluation>> GetByAssetIdAsync(Guid assetId, Guid userId)
        => await _context.RealEstateEvaluations
            .Where(r => r.AssetId == assetId && r.UserId == userId)
            .OrderByDescending(r => r.EvaluatedAt)
            .ToListAsync();

    public async Task<RealEstateEvaluation?> GetLatestByAssetIdAsync(Guid assetId, Guid userId)
        => await _context.RealEstateEvaluations
            .Where(r => r.AssetId == assetId && r.UserId == userId)
            .OrderByDescending(r => r.EvaluatedAt)
            .FirstOrDefaultAsync();

    public async Task AddAsync(RealEstateEvaluation evaluation)
    {
        await _context.RealEstateEvaluations.AddAsync(evaluation);
        await _context.SaveChangesAsync();
    }
}
