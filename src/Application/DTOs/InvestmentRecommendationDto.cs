namespace Prospera.Application.DTOs;

/// <summary>
/// Investment recommendation response with structured portfolio analysis
/// </summary>
public class InvestmentRecommendationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    // Keep backward compat
    public string SuggestedAllocation { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string AnalysisContext { get; set; } = string.Empty;

    // Structured response
    public PortfolioProfileDto Profile { get; set; } = new();
    public AllocationBreakdownDto Allocation { get; set; } = new();
    public List<string> SuggestedActions { get; set; } = new();
    public List<string> KeyRisks { get; set; } = new();
    public List<string> Opportunities { get; set; } = new();
    public List<CoachingGoalSuggestionDto> SuggestedCoachingGoals { get; set; } = new();

    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Portfolio profile with aggregated financial metrics and breakdowns
/// </summary>
public class PortfolioProfileDto
{
    public string RiskProfile { get; set; } = string.Empty;
    public decimal NetWorth { get; set; }
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public List<AssetGroupSummaryDto> AssetBreakdown { get; set; } = new();
    public List<LiabilityGroupSummaryDto> LiabilityBreakdown { get; set; } = new();
}

/// <summary>
/// Asset summary grouped by type
/// </summary>
public class AssetGroupSummaryDto
{
    public string Type { get; set; } = string.Empty;
    public decimal TotalValue { get; set; }
    public decimal PortfolioPercentage { get; set; }
    public int Count { get; set; }
    public List<string> Names { get; set; } = new();
}

/// <summary>
/// Liability summary grouped by type
/// </summary>
public class LiabilityGroupSummaryDto
{
    public string Type { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int Count { get; set; }
    public List<string> Names { get; set; } = new();
}

/// <summary>
/// Allocation breakdown with structured percentages per category
/// </summary>
public class AllocationBreakdownDto
{
    public int Stocks { get; set; }
    public int Bonds { get; set; }
    public int RealEstate { get; set; }
    public int Crypto { get; set; }
    public int Cash { get; set; }
    public List<RecommendationAllocationItemDto> Details { get; set; } = new();
}

/// <summary>
/// Individual allocation item with rationale
/// </summary>
public class RecommendationAllocationItemDto
{
    public string Category { get; set; } = string.Empty;
    public int Percentage { get; set; }
    public string Rationale { get; set; } = string.Empty;
}

/// <summary>
/// Suggested coaching goal derived from recommendation
/// </summary>
public class CoachingGoalSuggestionDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CoachingGoal { get; set; } = string.Empty;
}
