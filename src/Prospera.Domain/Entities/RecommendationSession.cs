using Prospera.Domain.Common;

namespace Prospera.Domain.Entities;

public class RecommendationSession : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation property for EF
    public List<InvestmentRecommendation> Recommendations { get; private set; } = new();

    public RecommendationSession(Guid userId, string title, string? description = null)
    {
        UserId = userId;
        Title = title;
        Description = description;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTitle(string title, string? description = null)
    {
        Title = title;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }
}
