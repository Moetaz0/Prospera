using Prospera.Domain.Common;

namespace Prospera.Domain.Entities;

/// <summary>
/// Investment recommendation with structured portfolio analysis
/// Includes asset/liability breakdown, suggested actions, risks, and opportunities
/// </summary>
public class InvestmentRecommendation : BaseEntity
{
    public Guid UserId { get; private set; }
    public string SuggestedAllocation { get; private set; }
    public string Explanation { get; private set; }
    public string AnalysisContext { get; private set; } = string.Empty;
    public Guid? SessionId { get; private set; }

    // Structured recommendation fields (nullable for backward compatibility with existing MongoDB documents)
    public List<RecommendationAllocationItem>? AllocationItems { get; private set; }
    public List<string>? SuggestedActions { get; private set; }
    public List<string>? KeyRisks { get; private set; }
    public List<string>? Opportunities { get; private set; }
    public string RiskProfile { get; private set; } = string.Empty;
    public string PortfolioSummary { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    public InvestmentRecommendation(Guid userId, string suggestedAllocation, string explanation, string analysisContext = "", Guid? sessionId = null)
    {
        UserId = userId;
        SuggestedAllocation = suggestedAllocation;
        Explanation = explanation;
        AnalysisContext = analysisContext;
        SessionId = sessionId;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Enrich recommendation with structured analysis data
    /// </summary>
    public void Enrich(
        string riskProfile,
        string portfolioSummary,
        List<RecommendationAllocationItem>? allocationItems,
        List<string>? suggestedActions,
        List<string>? keyRisks,
        List<string>? opportunities)
    {
        RiskProfile = riskProfile;
        PortfolioSummary = portfolioSummary;
        AllocationItems = allocationItems;
        SuggestedActions = suggestedActions;
        KeyRisks = keyRisks;
        Opportunities = opportunities;
    }
}

/// <summary>
/// Allocation item with category, percentage, and rationale
/// </summary>
public class RecommendationAllocationItem
{
    public string? Id { get; set; } = Guid.NewGuid().ToString();
    public string Category { get; set; } = string.Empty;
    public int Percentage { get; set; }
    public string Rationale { get; set; } = string.Empty;
}
