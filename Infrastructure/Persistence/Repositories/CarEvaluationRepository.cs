using Microsoft.EntityFrameworkCore;
using Prospera.Domain.Entities;
using Prospera.Domain.Interfaces;

namespace Prospera.Infrastructure.Persistence.Repositories;

public class CarEvaluationRepository : ICarEvaluationRepository
{
    private readonly ApplicationDbContext _context;

    public CarEvaluationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CarEvaluation>> GetByUserIdAsync(Guid userId)
        => await _context.CarEvaluations
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.EvaluatedAt)
            .ToListAsync();

    public async Task<IEnumerable<CarEvaluation>> GetByAssetIdAsync(Guid assetId, Guid userId)
        => await _context.CarEvaluations
            .Where(c => c.AssetId == assetId && c.UserId == userId)
            .OrderByDescending(c => c.EvaluatedAt)
            .ToListAsync();

    public async Task<CarEvaluation?> GetLatestByAssetIdAsync(Guid assetId, Guid userId)
        => await _context.CarEvaluations
            .Where(c => c.AssetId == assetId && c.UserId == userId)
            .OrderByDescending(c => c.EvaluatedAt)
            .FirstOrDefaultAsync();

    public async Task AddAsync(CarEvaluation evaluation)
    {
        await _context.CarEvaluations.AddAsync(evaluation);
        await _context.SaveChangesAsync();
    }
}
