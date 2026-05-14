using Microsoft.EntityFrameworkCore;
using Prospera.Domain.Entities;
using Prospera.Domain.Interfaces;
using Prospera.Infrastructure.Persistence;

namespace Prospera.Infrastructure.Persistence.Repositories;

public class NewsRepository : INewsRepository
{
    private readonly ApplicationDbContext _context;

    public NewsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CachedNewsArticle>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<CachedNewsArticle>()
            .OrderByDescending(n => n.PublishedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CachedNewsArticle>> GetByTypeAsync(string newsType, int take = 10, CancellationToken cancellationToken = default)
    {
        return await _context.Set<CachedNewsArticle>()
            .Where(n => n.NewsType == newsType)
            .OrderByDescending(n => n.PublishedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CachedNewsArticle>> GetRecentAsync(int days = 1, int take = 20, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return await _context.Set<CachedNewsArticle>()
            .Where(n => n.CachedAt >= cutoffDate)
            .OrderByDescending(n => n.PublishedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CachedNewsArticle article, CancellationToken cancellationToken = default)
    {
        await _context.Set<CachedNewsArticle>().AddAsync(article, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<CachedNewsArticle> articles, CancellationToken cancellationToken = default)
    {
        await _context.Set<CachedNewsArticle>().AddRangeAsync(articles, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteOldAsync(int olderThanDays = 7, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);
        var oldArticles = await _context.Set<CachedNewsArticle>()
            .Where(n => n.CachedAt < cutoffDate)
            .ToListAsync(cancellationToken);

        if (oldArticles.Any())
        {
            _context.Set<CachedNewsArticle>().RemoveRange(oldArticles);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task ClearByTypeAsync(string newsType, CancellationToken cancellationToken = default)
    {
        var articles = await _context.Set<CachedNewsArticle>()
            .Where(n => n.NewsType == newsType)
            .ToListAsync(cancellationToken);

        if (articles.Any())
        {
            _context.Set<CachedNewsArticle>().RemoveRange(articles);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
