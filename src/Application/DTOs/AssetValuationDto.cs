namespace Prospera.Application.DTOs;

/// <summary>
/// Asset with projected valuations including inflation, amortization, and growth
/// </summary>
public class AssetValuationDto
{
    /// <summary>
    /// Asset ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Asset name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Asset type (Cash, Stock, Car, RealEstate, etc.)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Current value of the asset today
    /// </summary>
    public decimal CurrentValue { get; set; }

    /// <summary>
    /// Projected value 1 year ahead after inflation adjustment
    /// (Adjusted for inflation - money value)
    /// </summary>
    public decimal ProjectedValueAfterInflation { get; set; }

    /// <summary>
    /// Inflation loss (how much purchasing power is lost)
    /// </summary>
    public decimal InflationLoss { get; set; }

    /// <summary>
    /// Projected value 1 year ahead for Cars (after depreciation/amortization)
    /// </summary>
    public decimal? ProjectedValueCar { get; set; }

    /// <summary>
    /// Car depreciation amount (if applicable)
    /// </summary>
    public decimal? CarDepreciation { get; set; }

    /// <summary>
    /// Annual depreciation rate for car (%)
    /// </summary>
    public decimal? CarDepreciationRate { get; set; }

    /// <summary>
    /// Projected value 1 year ahead for Real Estate (after appreciation)
    /// </summary>
    public decimal? ProjectedValueRealEstate { get; set; }

    /// <summary>
    /// Real estate appreciation amount (if applicable)
    /// </summary>
    public decimal? RealEstateAppreciation { get; set; }

    /// <summary>
    /// Annual appreciation rate for real estate (%)
    /// </summary>
    public decimal? RealEstateAppreciationRate { get; set; }

    /// <summary>
    /// Summary explanation of the projection
    /// </summary>
    public string ProjectionSummary { get; set; } = string.Empty;

    /// <summary>
    /// Calculation date (when this projection was calculated)
    /// </summary>
    public DateTime CalculatedAt { get; set; }

    // ========== ENHANCED INSIGHTS FOR WOW FACTOR ==========

    /// <summary>
    /// Multi-year projections showing growth trajectory
    /// </summary>
    public MultiYearProjectionDto? MultiYearProjection { get; set; }

    /// <summary>
    /// Growth metrics and performance indicators
    /// </summary>
    public GrowthMetricsDto? GrowthMetrics { get; set; }

    /// <summary>
    /// Asset performance score (0-100) - higher is better
    /// </summary>
    public decimal PerformanceScore { get; set; }

    /// <summary>
    /// Comparison vs inflation - shows if asset beats inflation
    /// </summary>
    public ComparisonMetricsDto? ComparisonMetrics { get; set; }

    /// <summary>
    /// Investment insights and recommendations
    /// </summary>
    public List<InsightDto> Insights { get; set; } = new();

    /// <summary>
    /// Wealth-building potential indicator
    /// </summary>
    public string WealthBuildingPotential { get; set; } = string.Empty;

    /// <summary>
    /// Risk level assessment
    /// </summary>
    public string RiskLevel { get; set; } = string.Empty;

    /// <summary>
    /// Key action items for optimization
    /// </summary>
    public List<string> Recommendations { get; set; } = new();

    // ========== COACHING MODE - BE A REAL COACH ==========

    /// <summary>
    /// Coaching insights - motivational guidance from your financial coach
    /// </summary>
    public CoachingInsightDto? CoachingInsight { get; set; }

    /// <summary>
    /// Comprehensive coaching advice for this asset
    /// Includes motivational guidance, action items, and personalized strategies
    /// </summary>
    public CoachingAdviceDto? CoachingAdvice { get; set; }

    /// <summary>
    /// Detailed information about rates used in this calculation
    /// Shows source (API, Override, Default) for transparency
    /// </summary>
    public RateSourceInfoDto? RateSourceInfo { get; set; }
}

/// <summary>
/// Coaching insights - transform from advisor to coach
/// Provides motivational guidance, behavioral coaching, and personalized strategies
/// </summary>
public class CoachingInsightDto
{
    /// <summary>
    /// Main coaching message - the "coach talk"
    /// Similar to what a real financial coach would say
    /// </summary>
    public string CoachMessage { get; set; } = string.Empty;

    /// <summary>
    /// What the user is doing well - celebrate wins!
    /// </summary>
    public List<string> WinsToRecognize { get; set; } = new();

    /// <summary>
    /// Areas where the user needs to focus/improve
    /// Presented as challenges, not criticisms
    /// </summary>
    public List<CoachingChallengeDto> Challenges { get; set; } = new();

    /// <summary>
    /// Smart, personalized action steps (not just generic advice)
    /// These are things the coach is asking them to do THIS WEEK
    /// </summary>
    public List<CoachingActionDto> ThisWeekActions { get; set; } = new();

    /// <summary>
    /// Longer-term goals to work toward
    /// What should they be thinking about for the next 6-12 months
    /// </summary>
    public List<string> QuarterlyGoals { get; set; } = new();

    /// <summary>
    /// Mindset shift the coach is encouraging
    /// Psychology/behavioral aspect of investing
    /// </summary>
    public string MindsetShift { get; set; } = string.Empty;

    /// <summary>
    /// Motivational quote tailored to their situation
    /// </summary>
    public string MotivationalQuote { get; set; } = string.Empty;

    /// <summary>
    /// Coaching rating (1-5) - how confident is the coach in this asset
    /// </summary>
    public decimal CoachRating { get; set; }

    /// <summary>
    /// Emotional tone for this asset (Bullish, Cautious, Excited, Concerned, etc.)
    /// </summary>
    public string CoachTone { get; set; } = string.Empty;

    /// <summary>
    /// Risk warning if needed - coach's honest assessment
    /// </summary>
    public string? CoachWarning { get; set; }

    /// <summary>
    /// Success indicator - how likely is user to reach goals with this asset
    /// </summary>
    public string SuccessProbability { get; set; } = string.Empty;

    /// <summary>
    /// Coach's signature sign-off
    /// </summary>
    public string CoachSignOff { get; set; } = string.Empty;
}

/// <summary>
/// A specific challenge the coach wants to address
/// </summary>
public class CoachingChallengeDto
{
    /// <summary>
    /// The challenge (presented positively, not negatively)
    /// </summary>
    public string Challenge { get; set; } = string.Empty;

    /// <summary>
    /// Why this matters
    /// </summary>
    public string Why { get; set; } = string.Empty;

    /// <summary>
    /// How to overcome it
    /// </summary>
    public string Solution { get; set; } = string.Empty;

    /// <summary>
    /// Impact if they solve this
    /// </summary>
    public string Impact { get; set; } = string.Empty;
}

/// <summary>
/// A specific action the coach wants them to take
/// </summary>
public class CoachingActionDto
{
    /// <summary>
    /// The action (specific and doable)
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Why they should do it
    /// </summary>
    public string Why { get; set; } = string.Empty;

    /// <summary>
    /// How to do it (step by step if needed)
    /// </summary>
    public string HowTo { get; set; } = string.Empty;

    /// <summary>
    /// Urgency level: Critical, High, Medium, Low
    /// </summary>
    public string Urgency { get; set; } = string.Empty;

    /// <summary>
    /// Expected result/benefit
    /// </summary>
    public string ExpectedBenefit { get; set; } = string.Empty;
}

/// <summary>
/// Information about the rates used in the valuation
/// Shows where each rate came from (API, admin override, or hardcoded default)
/// </summary>
public class RateSourceInfoDto
{
    /// <summary>
    /// Inflation rate used
    /// </summary>
    public decimal InflationRate { get; set; }

    /// <summary>
    /// Source of inflation rate: "World Bank API", "Admin Override", or "Default"
    /// </summary>
    public string InflationRateSource { get; set; } = string.Empty;

    /// <summary>
    /// Car depreciation rate used
    /// </summary>
    public decimal CarDepreciationRate { get; set; }

    /// <summary>
    /// Source of car depreciation rate
    /// </summary>
    public string CarDepreciationRateSource { get; set; } = string.Empty;

    /// <summary>
    /// Real estate appreciation rate used
    /// </summary>
    public decimal RealEstateAppreciationRate { get; set; }

    /// <summary>
    /// Source of real estate appreciation rate
    /// </summary>
    public string RealEstateAppreciationRateSource { get; set; } = string.Empty;

    /// <summary>
    /// User's country (used to fetch country-specific rates)
    /// </summary>
    public string? UserCountry { get; set; }

    /// <summary>
    /// Timestamp when rates were fetched/calculated
    /// </summary>
    public DateTime RatesFetchedAt { get; set; }

    /// <summary>
    /// Summary of rate sources
    /// </summary>
    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Multi-year projection scenarios
/// </summary>
public class MultiYearProjectionDto
{
    /// <summary>
    /// 5-year projection
    /// </summary>
    public ProjectionYearDto? Year5 { get; set; }

    /// <summary>
    /// 10-year projection
    /// </summary>
    public ProjectionYearDto? Year10 { get; set; }

    /// <summary>
    /// 20-year projection
    /// </summary>
    public ProjectionYearDto? Year20 { get; set; }
}

/// <summary>
/// Single year projection data
/// </summary>
public class ProjectionYearDto
{
    public int Year { get; set; }
    public decimal ProjectedValue { get; set; }
    public decimal TotalGain { get; set; }
    public decimal GainPercentage { get; set; }
    public decimal InflationLoss { get; set; }
}

/// <summary>
/// Growth metrics and performance indicators
/// </summary>
public class GrowthMetricsDto
{
    /// <summary>
    /// One-year growth percentage
    /// </summary>
    public decimal OneYearGrowthPercent { get; set; }

    /// <summary>
    /// Real return (accounting for inflation)
    /// </summary>
    public decimal RealReturnPercent { get; set; }

    /// <summary>
    /// Inflation beating indicator (true if asset beats inflation)
    /// </summary>
    public bool BeatsInflation { get; set; }

    /// <summary>
    /// Total growth amount in dollars
    /// </summary>
    public decimal GrowthAmount { get; set; }

    /// <summary>
    /// Compound growth rate if applicable
    /// </summary>
    public decimal? CompoundGrowthRate { get; set; }
}

/// <summary>
/// Comparison metrics against benchmarks
/// </summary>
public class ComparisonMetricsDto
{
    /// <summary>
    /// Inflation rate used for comparison
    /// </summary>
    public decimal InflationRate { get; set; }

    /// <summary>
    /// Performance vs inflation (gain/loss)
    /// </summary>
    public decimal PerformanceVsInflation { get; set; }

    /// <summary>
    /// Is asset maintaining purchasing power?
    /// </summary>
    public bool MaintainsPurchasingPower { get; set; }

    /// <summary>
    /// Trend description (Beating Inflation, Losing Value, etc.)
    /// </summary>
    public string Trend { get; set; } = string.Empty;
}

/// <summary>
/// Individual insight or recommendation
/// </summary>
public class InsightDto
{
    /// <summary>
    /// Type of insight (Opportunity, Warning, Strength, etc.)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The insight message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Impact level (High, Medium, Low)
    /// </summary>
    public string Impact { get; set; } = string.Empty;
}

/// <summary>
/// Coaching advice - comprehensive guidance from your financial coach
/// </summary>
public class CoachingAdviceDto
{
    /// <summary>
    /// Motivational guidance - the "coach talk"
    /// </summary>
    public string Motivation { get; set; } = string.Empty;

    /// <summary>
    /// Action items for the user to implement
    /// </summary>
    public List<string> Actions { get; set; } = new();

    /// <summary>
    /// Personalized strategies for optimizing this asset
    /// </summary>
    public List<string> Strategies { get; set; } = new();

    /// <summary>
    /// Behavioral coaching tips - mindset and habits
    /// </summary>
    public List<string> BehavioralTips { get; set; } = new();

    /// <summary>
    /// Success traits - characteristics of users who succeed with this asset
    /// </summary>
    public List<string> SuccessTraits { get; set; } = new();

    /// <summary>
    /// Common pitfalls to avoid
    /// </summary>
    public List<string> PitfallsToAvoid { get; set; } = new();

    /// <summary>
    /// Reminder of the user's goals and how this asset helps
    /// </summary>
    public string GoalsReminder { get; set; } = string.Empty;

    /// <summary>
    /// Next steps - what to focus on immediately
    /// </summary>
    public List<string> NextSteps { get; set; } = new();
}
