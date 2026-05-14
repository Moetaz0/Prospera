namespace Prospera.Domain.Entities;

/// <summary>
/// Represents a cached news article in the system (stored in database)
/// </summary>
public class CachedNewsArticle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Image { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? Category { get; set; }
    public DateTime PublishedAt { get; set; }
    public string? Sentiment { get; set; }
    public string NewsType { get; set; } = "business"; // business, crypto, trending
    public DateTime CachedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
