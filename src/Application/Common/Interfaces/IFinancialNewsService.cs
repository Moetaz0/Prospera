namespace Prospera.Application.Common.Interfaces;

public interface IFinancialNewsService
{
    /// <summary>
    /// Gets latest financial news from various sources
    /// </summary>
    Task<FinancialNewsData?> GetLatestNewsAsync(int pageSize = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets financial news by specific category (stocks, crypto, banking, etc.)
    /// </summary>
    Task<FinancialNewsData?> GetNewsByCategoryAsync(string category, int pageSize = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for financial news by keyword
    /// </summary>
    Task<FinancialNewsData?> SearchNewsAsync(string keyword, int pageSize = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets cryptocurrency-related news
    /// </summary>
    Task<FinancialNewsData?> GetCryptoNewsAsync(int pageSize = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets trending financial news
    /// </summary>
    Task<FinancialNewsData?> GetTrendingNewsAsync(int pageSize = 10, CancellationToken cancellationToken = default);
}

/// <summary>
/// Financial news data model for news articles
/// </summary>
public class NewsArticle
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Image { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? Category { get; set; }
    public DateTime PublishedAt { get; set; }
    public string? Sentiment { get; set; }
}

/// <summary>
/// Container for financial news data
/// </summary>
public class FinancialNewsData
{
    public bool Success { get; set; }
    public int TotalResults { get; set; }
    public List<NewsArticle> Articles { get; set; } = new();
    public DateTime FetchedAt { get; set; }
}
