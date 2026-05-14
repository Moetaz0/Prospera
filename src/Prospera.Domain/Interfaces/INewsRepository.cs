using Prospera.Domain.Entities;

namespace Prospera.Domain.Interfaces;

public interface INewsRepository
{
    /// <summary>
    /// Gets all cached news articles
    /// </summary>
    Task<IEnumerable<CachedNewsArticle>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets news articles by type (business, crypto, trending)
    /// </summary>
    Task<IEnumerable<CachedNewsArticle>> GetByTypeAsync(string newsType, int take = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent news (cached today)
    /// </summary>
    Task<IEnumerable<CachedNewsArticle>> GetRecentAsync(int days = 1, int take = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new news article
    /// </summary>
    Task AddAsync(CachedNewsArticle article, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple news articles
    /// </summary>
    Task AddRangeAsync(IEnumerable<CachedNewsArticle> articles, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes old news articles (older than specified days)
    /// </summary>
    Task DeleteOldAsync(int olderThanDays = 7, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all news of a specific type
    /// </summary>
    Task ClearByTypeAsync(string newsType, CancellationToken cancellationToken = default);
}
