using Microsoft.EntityFrameworkCore;
using Prospera.Domain.Entities;
using Prospera.Domain.Interfaces;

namespace Prospera.Infrastructure.Persistence.Repositories;

public class InvestmentRecommendationRepository : IInvestmentRecommendationRepository
{
    private readonly ApplicationDbContext _context;

    public InvestmentRecommendationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InvestmentRecommendation?> GetByIdAsync(Guid id)
    {
        return await _context.InvestmentRecommendations.FindAsync(id);
    }

    public async Task<IEnumerable<InvestmentRecommendation>> GetByUserIdAsync(Guid userId)
    {
        return await _context.InvestmentRecommendations
            .Where(ir => ir.UserId == userId)
            .OrderByDescending(ir => ir.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<InvestmentRecommendation>> GetBySessionIdAsync(Guid sessionId)
    {
        return await _context.InvestmentRecommendations
            .Where(ir => ir.SessionId == sessionId)
            .OrderByDescending(ir => ir.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(InvestmentRecommendation recommendation)
    {
        await _context.InvestmentRecommendations.AddAsync(recommendation);
        await _context.SaveChangesAsync();
    }
}
