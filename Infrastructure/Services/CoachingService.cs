using Prospera.Application.Common.Interfaces;
using Prospera.Application.DTOs;

namespace Prospera.Infrastructure.Services;

/// <summary>
/// Implementation of ICoachingService - provides personalized financial coaching
/// Uses psychology-based coaching techniques combined with financial analysis
/// Falls back to template-based coaching if AI is unavailable
/// </summary>
public class CoachingService : ICoachingService
{
    private readonly IAiRecommendationService? _aiRecommendationService;

    public CoachingService(IAiRecommendationService? aiRecommendationService = null)
    {
        _aiRecommendationService = aiRecommendationService;
    }

    public async Task<CoachingAdviceDto> GenerateAssetCoachingAdviceAsync(
        Guid assetId,
        string assetName,
        string assetType,
        decimal currentValue,
        decimal projectedValue,
        string? projectionSummary,
        decimal riskTolerance,
        decimal portfolioAllocationPercentage,
        CancellationToken cancellationToken = default)
    {
        // Calculate performance metrics
        var valueChange = projectedValue - currentValue;
        var percentageChange = currentValue > 0 ? (valueChange / currentValue) * 100 : 0;
        var isGrowing = valueChange >= 0;

        // If AI is available, use it for better coaching
        if (_aiRecommendationService != null)
        {
            var coachingPrompt = $@"You are a financial coach providing personalized guidance for an asset. Be motivational, supportive, and actionable.

ASSET DETAILS:
- Name: {assetName}
- Type: {assetType}
- Current Value: ${currentValue:F2}
- Projected Value (1 year): ${projectedValue:F2}
- Change: {(isGrowing ? "+" : "")}{percentageChange:F2}% (${valueChange:F2})
- Portfolio Allocation: {portfolioAllocationPercentage:F1}%
- Risk Tolerance Score: {riskTolerance:P0}

Provide coaching advice structured as follows:
MOTIVATION: [An encouraging statement about this asset]
ACTIONS: [3-5 bullet points of specific actions to take]
STRATEGIES: [3-4 strategies for optimizing this asset]
BEHAVIORAL_TIPS: [2-3 mindset/habit tips]
SUCCESS_TRAITS: [2-3 traits of successful investors with this asset]
PITFALLS: [2-3 common mistakes to avoid]
GOALS_REMINDER: [How this asset helps reach financial goals]
NEXT_STEPS: [2-3 immediate next steps]";

            try
            {
                var coachingResponse = await _aiRecommendationService.GenerateRecommendationAsync(coachingPrompt, cancellationToken);
                return ParseCoachingResponse(coachingResponse);
            }
            catch
            {
                // Fall back to template if AI fails
            }
        }

        // Template-based coaching when AI is unavailable
        return GenerateTemplateAssetCoaching(assetName, assetType, currentValue, projectedValue, percentageChange, isGrowing);
    }

    public async Task<CoachingAdviceDto> GenerateLiabilityCoachingAdviceAsync(
        Guid liabilityId,
        string liabilityName,
        string liabilityType,
        decimal currentAmount,
        decimal? interestRate,
        decimal? monthlyPayment,
        decimal netWorth,
        decimal totalLiabilities,
        decimal debtToIncomeRatio,
        CancellationToken cancellationToken = default)
    {
        var liabilityPercentageOfNetWorth = netWorth > 0 ? (currentAmount / netWorth) * 100 : 0;

        // If AI is available, use it for better coaching
        if (_aiRecommendationService != null)
        {
            var coachingPrompt = $@"You are a debt coaching specialist helping someone manage their liabilities. Be empathetic, supportive, and focused on liberation from debt.

LIABILITY DETAILS:
- Name: {liabilityName}
- Type: {liabilityType}
- Current Amount Owed: ${currentAmount:F2}
- Interest Rate: {(interestRate.HasValue ? $"{interestRate:F2}%" : "N/A")}
- Monthly Payment: {(monthlyPayment.HasValue ? $"${monthlyPayment:F2}" : "N/A")}
- % of Net Worth: {liabilityPercentageOfNetWorth:F1}%
- Total Liabilities: ${totalLiabilities:F2}
- Debt-to-Income Ratio: {debtToIncomeRatio:F2}

Provide debt coaching advice structured as follows:
MOTIVATION: [Empathetic and motivating statement about managing this debt]
ACTIONS: [3-5 specific steps to reduce/manage this liability]
STRATEGIES: [3-4 strategies for debt reduction (payoff, refinance, etc.)]
BEHAVIORAL_TIPS: [2-3 behavioral tips for staying on track]
SUCCESS_TRAITS: [2-3 traits of people who successfully eliminate debt]
PITFALLS: [2-3 common debt management mistakes to avoid]
GOALS_REMINDER: [How eliminating this debt helps financial freedom]
NEXT_STEPS: [2-3 immediate next steps]";

            try
            {
                var coachingResponse = await _aiRecommendationService.GenerateRecommendationAsync(coachingPrompt, cancellationToken);
                return ParseCoachingResponse(coachingResponse);
            }
            catch
            {
                // Fall back to template if AI fails
            }
        }

        // Template-based coaching when AI is unavailable
        return GenerateTemplateLiabilityCoaching(liabilityName, liabilityType, currentAmount, interestRate);
    }

    public string GenerateCoachingPromptForRecommendations(
        decimal netWorth,
        decimal totalAssets,
        decimal totalLiabilities,
        int assetCount,
        int liabilityCount,
        decimal averageCoachingScore,
        string riskProfile)
    {
        var debtRatio = totalAssets > 0 ? totalLiabilities / totalAssets : 0;
        var diversificationLevel = assetCount > 5 ? "Excellent" : assetCount > 2 ? "Good" : "Limited";

        return $@"You are a comprehensive financial coach creating personalized investment recommendations. 
Focus on building wealth systematically, managing risk appropriately, and creating sustainable financial habits.

HOLISTIC FINANCIAL COACHING CONTEXT:
- Net Worth: ${netWorth:F2}
- Total Assets: ${totalAssets:F2} ({assetCount} assets)
- Total Liabilities: ${totalLiabilities:F2} ({liabilityCount} liabilities)
- Debt-to-Asset Ratio: {debtRatio:P0}
- Portfolio Diversification: {diversificationLevel}
- Overall Coaching Health Score: {averageCoachingScore:F0}/100
- Risk Profile: {riskProfile}

COACHING RECOMMENDATIONS SHOULD:
1. Balance asset growth with liability reduction
2. Consider the user's complete financial picture, not just investments
3. Provide motivational and educational context
4. Include specific milestones and timelines
5. Address both immediate actions and long-term strategy
6. Acknowledge progress and celebrate wins
7. Build sustainable financial habits

Please create recommendations that feel like personalized coaching from a financial mentor, 
not just algorithmic suggestions.";
    }

    /// <summary>
    /// Generate template-based asset coaching when AI is unavailable
    /// </summary>
    private CoachingAdviceDto GenerateTemplateAssetCoaching(
        string assetName,
        string assetType,
        decimal currentValue,
        decimal projectedValue,
        decimal percentageChange,
        bool isGrowing)
    {
        var advice = new CoachingAdviceDto();

        advice.Motivation = isGrowing
            ? $"Great job building your {assetType} position! {assetName} is positioned to grow over the next year."
            : $"Now is a good time to review your {assetType} strategy. Understanding how {assetName} fits your portfolio is key.";

        advice.Actions = new List<string>
        {
            $"Review your {assetType} holdings regularly",
            "Consider your overall portfolio allocation",
            "Align this asset with your financial goals",
            "Monitor market conditions and adjust as needed"
        };

        advice.Strategies = new List<string>
        {
            $"Diversify within {assetType} if concentrated",
            "Rebalance annually to maintain targets",
            "Dollar-cost average into positions over time",
            "Keep costs low with efficient investments"
        };

        advice.BehavioralTips = new List<string>
        {
            "Avoid emotional decision-making during volatility",
            "Stay focused on your long-term goals, not short-term noise",
            "Review progress quarterly, not daily"
        };

        advice.SuccessTraits = new List<string>
        {
            "Patience - wealth compounds over time",
            "Discipline - stick to your investment plan",
            "Diversification - don't put all eggs in one basket"
        };

        advice.PitfallsToAvoid = new List<string>
        {
            "Chasing performance or recent winners",
            "Abandoning your strategy during downturns",
            "Overconcentration in a single asset"
        };

        advice.GoalsReminder = $"This {assetType} helps you build wealth systematically. Stay the course!";
        advice.NextSteps = new List<string>
        {
            "Schedule a portfolio review",
            "Assess if this asset aligns with your goals",
            "Commit to a long-term holding period"
        };

        return advice;
    }

    /// <summary>
    /// Generate template-based liability coaching when AI is unavailable
    /// </summary>
    private CoachingAdviceDto GenerateTemplateLiabilityCoaching(
        string liabilityName,
        string liabilityType,
        decimal currentAmount,
        decimal? interestRate)
    {
        var advice = new CoachingAdviceDto();

        advice.Motivation = $"You can eliminate this {liabilityType}! With focus and a solid plan, freedom from {liabilityName} is within reach.";

        advice.Actions = new List<string>
        {
            $"Create a payoff plan for this {liabilityType}",
            "Make at least minimum payments on time",
            "Look for ways to accelerate payoff",
            "Build an emergency fund to prevent more debt"
        };

        advice.Strategies = new List<string>
        {
            "Use the debt snowball method (pay smallest first)",
            "Or debt avalanche method (highest interest first)",
            "Consider refinancing to lower interest rates",
            "Negotiate with creditors for better terms"
        };

        advice.BehavioralTips = new List<string>
        {
            "Celebrate small wins - every payment matters",
            "Track your progress visually to stay motivated",
            "Address the root cause to prevent re-accumulation"
        };

        advice.SuccessTraits = new List<string>
        {
            "Determination - commit to becoming debt-free",
            "Honesty - face the debt head-on",
            "Consistency - make payments reliably"
        };

        advice.PitfallsToAvoid = new List<string>
        {
            "Taking on new debt while paying off this one",
            "Making only minimum payments",
            "Ignoring the debt - it won't go away"
        };

        advice.GoalsReminder = "Becoming debt-free will increase your financial flexibility and reduce stress!";
        advice.NextSteps = new List<string>
        {
            "Calculate your payoff date with current pace",
            "Make one extra payment this month if possible",
            "Tell someone about your goal for accountability"
        };

        return advice;
    }

    /// <summary>
    /// Parse structured coaching response from AI into CoachingAdviceDto
    /// </summary>
    private CoachingAdviceDto ParseCoachingResponse(string response)
    {
        var coaching = new CoachingAdviceDto();

        // Parse each section from the response
        coaching.Motivation = ExtractSection(response, "MOTIVATION");
        coaching.GoalsReminder = ExtractSection(response, "GOALS_REMINDER");
        
        // Parse list items
        coaching.Actions = ExtractListItems(response, "ACTIONS");
        coaching.Strategies = ExtractListItems(response, "STRATEGIES");
        coaching.BehavioralTips = ExtractListItems(response, "BEHAVIORAL_TIPS");
        coaching.SuccessTraits = ExtractListItems(response, "SUCCESS_TRAITS");
        coaching.PitfallsToAvoid = ExtractListItems(response, "PITFALLS");
        coaching.NextSteps = ExtractListItems(response, "NEXT_STEPS");

        return coaching;
    }

    private string ExtractSection(string content, string sectionName)
    {
        var pattern = $@"{sectionName}:\s*(.+?)(?=\n[A-Z_]+:|$)";
        var match = System.Text.RegularExpressions.Regex.Match(
            content, 
            pattern, 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
        return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
    }

    private List<string> ExtractListItems(string content, string sectionName)
    {
        var items = new List<string>();
        var section = ExtractSection(content, sectionName);

        if (string.IsNullOrEmpty(section))
            return items;

        var lines = section.Split('\n');
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("-") || trimmed.StartsWith("•") || trimmed.StartsWith("*"))
            {
                items.Add(trimmed.TrimStart('-', '•', '*', ' '));
            }
        }

        return items;
    }
}
