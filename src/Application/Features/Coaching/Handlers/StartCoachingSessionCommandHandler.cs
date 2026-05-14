using MediatR;
using AutoMapper;
using Prospera.Domain.Entities;
using Prospera.Domain.Interfaces;
using Prospera.Domain.Common;
using Prospera.Application.DTOs.Coaching;
using Prospera.Application.Common.Interfaces;
using Prospera.Application.Features.Coaching.Commands;
using System.Text.RegularExpressions;

namespace Prospera.Application.Features.Coaching.Handlers;

/// <summary>
/// Handler for coaching session commands
/// Provides data-driven financial coaching with:
/// - Real market data (inflation, exchange rates, real estate)
/// - Economic predictions (GDP, FX, RE forecasts)
/// - Personalized assessment and strategy
/// - Specific action items with market accountability
/// - Progress tracking and milestones
/// </summary>
public class StartCoachingSessionCommandHandler : IRequestHandler<StartCoachingSessionCommand, CoachingSessionDto>
{
    private readonly ICoachingSessionRepository _coachingSessionRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMarketDataService _marketDataService;
    private readonly IFinancialDataService _financialDataService;
    private readonly IOllamaConfiguration _ollamaConfig;
    private readonly ILlmProviderFactory _llmProviderFactory;

    public StartCoachingSessionCommandHandler(
        ICoachingSessionRepository coachingSessionRepository,
        IApplicationDbContext dbContext,
        IMapper mapper,
        IMarketDataService marketDataService,
        IFinancialDataService financialDataService,
        IOllamaConfiguration ollamaConfig,
        ILlmProviderFactory llmProviderFactory)
    {
        _coachingSessionRepository = coachingSessionRepository;
        _dbContext = dbContext;
        _mapper = mapper;
        _marketDataService = marketDataService;
        _financialDataService = financialDataService;
        _ollamaConfig = ollamaConfig;
        _llmProviderFactory = llmProviderFactory;
    }

    public async Task<CoachingSessionDto> Handle(StartCoachingSessionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get user's financial profile
            var userAssets = _dbContext.Assets.Where(a => a.UserId == request.UserId).ToList();
            var userLiabilities = _dbContext.Liabilities.Where(l => l.UserId == request.UserId).ToList();
            var userTransactions = _dbContext.Transactions.Where(t => t.UserId == request.UserId).ToList();

            // Calculate financial metrics
            var totalAssets = userAssets.Sum(a => a.CurrentValue);
            var totalLiabilities = userLiabilities.Sum(l => l.Amount);
            var netWorth = totalAssets - totalLiabilities;

            // Extract country code for targeted financial data
            var countryCode = ExtractCountryCodeFromPreferences(request.Preferences) ?? "TN";

            // Gather ALL real-time financial data in PARALLEL for coaching enrichment
            var inflationTask = _financialDataService.GetInflationDataAsync(countryCode, cancellationToken);
            var exchangeTask = countryCode == "TN" 
                ? _financialDataService.GetTndExchangeRatesAsync(cancellationToken)
                : _financialDataService.GetCurrentExchangeRatesAsync("USD", cancellationToken);
            var gdpTask = _financialDataService.GetGdpPredictionAsync(countryCode, cancellationToken);
            var carCategoriesTask = _financialDataService.GetCarCategoriesAsync(cancellationToken);
            var locationsTask = _financialDataService.GetRealEstateLocationsAsync(cancellationToken);

            await Task.WhenAll(inflationTask, exchangeTask, gdpTask, carCategoriesTask, locationsTask);

            var inflation = await inflationTask;
            var exchangeRates = await exchangeTask;
            var gdpPrediction = await gdpTask;
            var carCategories = await carCategoriesTask;

            // Get real estate data if location is mentioned
            RealEstateData? realEstate = null;
            var locations = await locationsTask;
            if (locations.Any())
            {
                var locationFromPrefs = ExtractLocationFromPreferences(request.Preferences);
                var targetLocation = locationFromPrefs ?? locations.FirstOrDefault();
                if (!string.IsNullOrEmpty(targetLocation))
                {
                    realEstate = await _financialDataService.GetTunisianRealEstateAsync(targetLocation, cancellationToken);
                }
            }

            // Generate coaching assessment and action plan with ACTUAL market data
            var coachingPrompt = BuildDataDrivenCoachingPrompt(
                request.CurrentSituation,
                request.Goal,
                request.Preferences,
                totalAssets,
                totalLiabilities,
                netWorth,
                inflation,
                exchangeRates,
                gdpPrediction,
                realEstate,
                carCategories,
                userAssets,
                userLiabilities);

            // Get LLM provider
            var provider = await _llmProviderFactory.GetProviderAsync((LlmProvider)request.Provider);
            var model = !string.IsNullOrWhiteSpace(request.ModelName) 
                ? request.ModelName 
                : GetDefaultModelForProvider((LlmProvider)request.Provider);

            // Generate coaching session via LLM with market data
            string coachingResponse;
            try
            {
                coachingResponse = await provider.GenerateResponseAsync(coachingPrompt, model, cancellationToken);
            }
            catch (Exception ex)
            {
                coachingResponse = GenerateTemplateCoachingWithMarketData(
                    request.Goal, 
                    totalAssets, 
                    totalLiabilities,
                    inflation,
                    exchangeRates,
                    realEstate);
            }

            // Parse coaching response
            var coaching = ParseCoachingResponse(coachingResponse, request.Goal);

            // Create coaching session entity with market context
            var session = new CoachingSession(request.UserId, request.Goal)
            {
                Assessment = coaching.Assessment,
                CoachingMessage = coaching.CoachingMessage,
                ModelUsed = model,
                Provider = provider.GetProviderName()
            };

            // Add action items from coaching
            foreach (var item in coaching.ActionItems)
            {
                session.AddActionItem(item.Title, item.Description, item.PriorityLevel);
            }

            // Add milestones from coaching
            foreach (var milestone in coaching.Milestones)
            {
                session.AddMilestone(milestone.Name, milestone.TargetProgressPercentage, milestone.Description);
            }

            // Save to repository
            await _coachingSessionRepository.AddAsync(session);

            // Map and return
            var dto = _mapper.Map<CoachingSessionDto>(session);
            return dto;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    /// <summary>
    /// Build coaching prompt enriched with REAL financial market data from all 21 endpoints
    /// Includes per-type asset and liability breakdown with individual item names
    /// </summary>
    private string BuildDataDrivenCoachingPrompt(
        string situation,
        string goal,
        string? preferences,
        decimal totalAssets,
        decimal totalLiabilities,
        decimal netWorth,
        InflationData? inflation,
        ExchangeRateData? exchangeRates,
        PredictionData? gdpPrediction,
        RealEstateData? realEstate,
        List<CarCategory>? carCategories,
        List<Asset> userAssets,
        List<Liability> userLiabilities)
    {
        // Build portfolio breakdown by asset type
        var assetsByType = userAssets
            .GroupBy(a => a.Type)
            .OrderByDescending(g => g.Sum(a => a.CurrentValue))
            .ToList();

        var assetDetail = new System.Text.StringBuilder();
        if (assetsByType.Any())
        {
            assetDetail.AppendLine("PORTFOLIO DETAIL:");
            foreach (var group in assetsByType)
            {
                var typeTotal = group.Sum(a => a.CurrentValue);
                var percentage = totalAssets > 0 ? (typeTotal / totalAssets * 100) : 0;
                var names = string.Join(", ", group.Select(a => a.Name).Take(5));
                if (group.Count() > 5)
                    names += $" +{group.Count() - 5} more";
                assetDetail.AppendLine($"  {group.Key}: {names} → ${typeTotal:F2} ({percentage:F1}% of portfolio)");
            }
        }

        // Build liabilities breakdown by type
        var liabilitiesByType = userLiabilities
            .GroupBy(l => l.Type)
            .OrderByDescending(g => g.Sum(l => l.Amount))
            .ToList();

        var liabilityDetail = new System.Text.StringBuilder();
        if (liabilitiesByType.Any())
        {
            liabilityDetail.AppendLine("DEBTS DETAIL:");
            foreach (var group in liabilitiesByType)
            {
                var typeTotal = group.Sum(l => l.Amount);
                var names = string.Join(", ", group.Select(l => l.Name).Take(5));
                if (group.Count() > 5)
                    names += $" +{group.Count() - 5} more";
                liabilityDetail.AppendLine($"  {group.Key}: {names} → ${typeTotal:F2}");
            }
        }

        // Calculate financial ratios and metrics
        var debtToAssetRatio = totalAssets > 0 ? totalLiabilities / totalAssets : 0;
        var netWorthPercentage = totalAssets > 0 ? (netWorth / totalAssets * 100) : 0;
        var debtHealthStatus = debtToAssetRatio switch
        {
            < 0.3m => "Healthy debt level",
            < 0.5m => "Moderate debt load",
            < 0.7m => "Higher debt levels requiring attention",
            _ => "Critical debt situation requiring action"
        };

        var prompt = $@"You are an EXPERIENCED FINANCIAL COACH with 15+ years mentoring people to financial freedom.

CLIENT PROFILE:
- Current Situation: {situation}
- Financial Goal: {goal}
- Total Assets: ${totalAssets:F2}
- Total Liabilities: ${totalLiabilities:F2}
- Net Worth: ${netWorth:F2}
- Debt-to-Asset Ratio: {debtToAssetRatio:P1} ({debtHealthStatus})
- Net Worth as % of Assets: {netWorthPercentage:F1}%
- Notes: {preferences ?? "None"}

{assetDetail}{liabilityDetail}

=== REAL MARKET DATA TODAY ===";

        // Add inflation data with strategy implications
        if (inflation != null && inflation.CurrentRate >= 0)
        {
            var cashAssets = userAssets.Where(a => a.Type.ToString() == "Cash").Sum(a => a.CurrentValue);
            var inflationImpact = inflation.CurrentRate > 0 && cashAssets > 0
                ? $"Your cash of ${cashAssets:F2} is losing {inflation.CurrentRate:F1}% in purchasing power annually."
                : "Inflation is currently low - favorable for savings and fixed income.";

            prompt += $@"
INFLATION RATE: {inflation.CurrentRate:F2}% ({inflation.CountryName})
IMPACT: {inflationImpact}
COACHING STRATEGY: ";
            if (inflation.CurrentRate > 5)
            {
                prompt += "HIGH inflation - MUST prioritize wealth protection & growth assets. Recommend: inflation-protected bonds, real assets, diversification. Increase investment rate to outpace inflation.";
            }
            else if (inflation.CurrentRate > 2)
            {
                prompt += $"Moderate inflation - Balance defensive and growth positions. Ensure investment returns exceed {inflation.CurrentRate:F1}% to preserve purchasing power.";
            }
            else
            {
                prompt += "Low inflation - Favorable environment for planning. Consider longer-term fixed income alongside growth investments.";
            }
        }

        // Add exchange rate data for diversification
        if (exchangeRates != null && exchangeRates.Rates.Any())
        {
            prompt += $@"

EXCHANGE RATES ({exchangeRates.BaseCurrency}):";
            foreach (var rate in exchangeRates.Rates.Take(4))
            {
                prompt += $" {rate.Key}={rate.Value:F4},";
            }
            prompt += @" 
DIVERSIFICATION: Consider currency hedging if international exposure. Monitor FX trends for retirement planning.";
        }

        // Add GDP outlook for risk assessment
        if (gdpPrediction != null && !string.IsNullOrEmpty(gdpPrediction.Reasoning))
        {
            prompt += $@"

ECONOMIC OUTLOOK: {gdpPrediction.Reasoning}
IMPLICATION: {(gdpPrediction.PredictedValue > 2 ? "Strong growth - consider growth investments" : "Weak growth - focus on stability & income")}";
        }

        // Add real estate market insights
        if (realEstate != null && realEstate.AveragePricePerSqm > 0)
        {
            prompt += $@"

REAL ESTATE MARKET ({realEstate.Location}):
- Average Price: ${realEstate.AveragePricePerSqm:F2}/sqm
- Rental Yield: {realEstate.RentalYield:F2}%
If property investment planned: Current market shows " +
                (realEstate.AveragePricePerSqm > 5000 ? "high prices - evaluate rent vs buy" : "moderate prices - good entry window");
        }

        prompt += $@"

=== COACHING TASK ===
Create a personalized, data-driven coaching session with these sections:

1. ASSESSMENT - Reference actual market data to show relevance (2-3 sentences)
2. COACHING MESSAGE - Motivational & reality-based given market conditions (3-5 sentences)
3. ACTION ITEMS - 4-6 specific items prioritized by market conditions:
   - If inflation high: Protect wealth first
   - If GDP weak: Focus on income/stability
   - If real estate market active: Include property considerations
   - Include specific numbers from market data
4. MILESTONES - 4 milestones with market context
5. MARKET-AWARE ADVICE - Explain WHY based on today's indicators

FORMAT OUTPUT EXACTLY:
=== ASSESSMENT START ===
[Your assessment]
=== ASSESSMENT END ===

=== COACHING MESSAGE START ===
[Motivational message with market context]
=== COACHING MESSAGE END ===

=== ACTION ITEMS START ===
ACTION: [Specific title] | PRIORITY: [1/2/3] | DESCRIPTION: [Details with specific numbers/timeframes]
=== ACTION ITEMS END ===

=== MILESTONES START ===
MILESTONE: [Name] at 25% | [Why this matters in current market]
MILESTONE: [Name] at 50% | [Market checkpoint]
MILESTONE: [Name] at 75% | [Momentum checkpoint]
MILESTONE: [Name] at 100% | [Success in context of market outlook]
=== MILESTONES END ===

Be a REAL coach. Be specific. Quote actual market data. Connect actions to economic realities.";

        return prompt;
    }

    /// <summary>
    /// Generate template coaching when LLM fails, still using real market data
    /// </summary>
    private string GenerateTemplateCoachingWithMarketData(
        string goal,
        decimal totalAssets,
        decimal totalLiabilities,
        InflationData? inflation,
        ExchangeRateData? exchangeRates,
        RealEstateData? realEstate)
    {
        var assessment = "You're at a critical point in your financial journey. ";
        var actionItems = "ACTION: Review current savings strategy | PRIORITY: 1 | DESCRIPTION: Audit all savings accounts\n";

        if (inflation != null && inflation.CurrentRate > 3)
        {
            assessment += $"With inflation at {inflation.CurrentRate:F1}%, protecting your wealth is urgent. ";
            actionItems += $"ACTION: Shift to inflation-protected investments | PRIORITY: 1 | DESCRIPTION: Move savings earning < {inflation.CurrentRate:F1}% to inflation-linked bonds\n";
        }

        assessment += $"Your goal '{goal}' is achievable with the right strategy.";
        actionItems += "ACTION: Create written financial plan | PRIORITY: 1 | DESCRIPTION: Document specific steps and timeline\n";
        actionItems += "ACTION: Automate savings transfers | PRIORITY: 2 | DESCRIPTION: Set up automatic monthly transfers\n";

        var response = $@"=== ASSESSMENT START ===
{assessment}
=== ASSESSMENT END ===

=== COACHING MESSAGE START ===
You have ${totalAssets:F2} in assets and strong potential. The market conditions today require active management, not passive waiting. Let's build a plan that works WITH the economy, not against it.
=== COACHING MESSAGE END ===

=== ACTION ITEMS START ===
{actionItems}=== ACTION ITEMS END ===

=== MILESTONES START ===
MILESTONE: First 25% - Quick wins | Build momentum with early actions
MILESTONE: 50% - Midpoint review | Adjust strategy based on market changes
MILESTONE: 75% - Home stretch | Reinforce habits and increase pace
MILESTONE: 100% - Goal achieved | Celebrate and plan next level
=== MILESTONES END ===";

        return response;
    }

    /// <summary>
    /// Extract country code from preferences (looking for country codes or names)
    /// </summary>
    private string? ExtractCountryCodeFromPreferences(string? preferences)
    {
        if (string.IsNullOrWhiteSpace(preferences))
            return null;

        var upper = preferences.ToUpper();
        var countryCodes = new[] { "TN", "FR", "US", "GB", "DE", "IT", "ES", "NL", "BE", "CH", "AT", "PL", "CZ", "SE", "NO", "DK", "FI", "PT", "GR", "TR" };
        var countryNames = new[] { "TUNISIA", "FRANCE", "UNITED STATES", "UNITED KINGDOM", "GERMANY", "ITALY", "SPAIN", "NETHERLANDS", "BELGIUM", "SWITZERLAND", "AUSTRIA", "POLAND", "CZECH", "SWEDEN", "NORWAY", "DENMARK", "FINLAND", "PORTUGAL", "GREECE", "TURKEY" };

        foreach (var code in countryCodes)
        {
            if (upper.Contains(code))
                return code;
        }

        for (int i = 0; i < countryNames.Length; i++)
        {
            if (upper.Contains(countryNames[i]))
                return countryCodes[i];
        }

        return null;
    }

    /// <summary>
    /// Extract location/city name from preferences
    /// </summary>
    private string? ExtractLocationFromPreferences(string? preferences)
    {
        if (string.IsNullOrWhiteSpace(preferences))
            return null;

        var upper = preferences.ToUpper();
        var locations = new[] { "TUNIS", "SFAX", "SOUSSE", "MONASTIR", "BIZERTE", "KAIROUAN", "GABÈS", "TOZEUR", "GAFSA", "HAMMAMET" };

        foreach (var location in locations)
        {
            if (upper.Contains(location))
                return location.ToLower();
        }

        return null;
    }

    private string GenerateTemplateCoaching(string goal, decimal totalAssets, decimal totalLiabilities)
    {
        return $@"=== ASSESSMENT START ===
You have strong fundamentals with ${totalAssets:F2} in assets. Your goal to {goal} is achievable.
=== ASSESSMENT END ===

=== COACHING MESSAGE START ===
This is your moment. You have the financial foundation and the motivation. Now it's about having the right plan and taking consistent action.
=== COACHING MESSAGE END ===

=== ACTION ITEMS START ===
ACTION: Review all accounts and investments | PRIORITY: 1 | DESCRIPTION: Document everything you own and owe
ACTION: Create a specific financial plan | PRIORITY: 1 | DESCRIPTION: Write down exact steps to reach {goal}
ACTION: Automate your savings | PRIORITY: 2 | DESCRIPTION: Set up automatic monthly transfers
=== ACTION ITEMS END ===

=== MILESTONES START ===
MILESTONE: First Steps at 25% | You're on your way
MILESTONE: Halfway There at 50% | Momentum building
MILESTONE: Home Stretch at 75% | Nearly there
MILESTONE: Success at 100% | Goal achieved
=== MILESTONES END ===";
    }

    private CoachingParsedResponse ParseCoachingResponse(string response, string goal)
    {
        var parsed = new CoachingParsedResponse();

        // Extract assessment
        var assessmentMatch = Regex.Match(
            response,
            @"=== ASSESSMENT START ===(.*?)=== ASSESSMENT END ===",
            RegexOptions.Singleline);
        parsed.Assessment = assessmentMatch.Success ? assessmentMatch.Groups[1].Value.Trim() : "Coaching assessment ready";

        // Extract coaching message
        var messageMatch = Regex.Match(
            response,
            @"=== COACHING MESSAGE START ===(.*?)=== COACHING MESSAGE END ===",
            RegexOptions.Singleline);
        parsed.CoachingMessage = messageMatch.Success ? messageMatch.Groups[1].Value.Trim() : "Your coaching session is ready";

        // Extract action items
        var actionItemsMatch = Regex.Match(
            response,
            @"=== ACTION ITEMS START ===(.*?)=== ACTION ITEMS END ===",
            RegexOptions.Singleline);

        if (actionItemsMatch.Success)
        {
            var actionItemsText = actionItemsMatch.Groups[1].Value;
            var actionLines = actionItemsText.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in actionLines)
            {
                if (line.StartsWith("ACTION:"))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 3)
                    {
                        var title = parts[0].Replace("ACTION:", "").Trim();
                        var priority = parts[1].Contains("1") ? 1 : parts[1].Contains("2") ? 2 : 3;
                        var description = parts[2].Replace("DESCRIPTION:", "").Trim();

                        parsed.ActionItems.Add(new CoachingActionItemInfo
                        {
                            Title = title,
                            Description = description,
                            PriorityLevel = priority
                        });
                    }
                }
            }
        }

        // Extract milestones
        var milestonesMatch = Regex.Match(
            response,
            @"=== MILESTONES START ===(.*?)=== MILESTONES END ===",
            RegexOptions.Singleline);

        if (milestonesMatch.Success)
        {
            var milestonesText = milestonesMatch.Groups[1].Value;
            var milestoneLines = milestonesText.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in milestoneLines)
            {
                if (line.StartsWith("MILESTONE:"))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 2)
                    {
                        var firstPart = parts[0].Replace("MILESTONE:", "").Trim();
                        var atIndex = firstPart.IndexOf(" at ");
                        if (atIndex > 0)
                        {
                            var name = firstPart.Substring(0, atIndex).Trim();
                            var percentStr = firstPart.Substring(atIndex + 4).Replace("%", "").Trim();
                            var description = parts[1].Trim();

                            if (decimal.TryParse(percentStr, out var percent))
                            {
                                parsed.Milestones.Add(new CoachingMilestoneInfo
                                {
                                    Name = name,
                                    TargetProgressPercentage = percent,
                                    Description = description
                                });
                            }
                        }
                    }
                }
            }
        }

        return parsed;
    }

    private class CoachingParsedResponse
    {
        public string Assessment { get; set; } = string.Empty;
        public string CoachingMessage { get; set; } = string.Empty;
        public List<CoachingActionItemInfo> ActionItems { get; set; } = new();
        public List<CoachingMilestoneInfo> Milestones { get; set; } = new();
    }

    private class CoachingActionItemInfo
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PriorityLevel { get; set; } = 1;
    }

    private class CoachingMilestoneInfo
    {
        public string Name { get; set; } = string.Empty;
        public decimal TargetProgressPercentage { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    private string GetDefaultModelForProvider(LlmProvider provider)
    {
        return provider switch
        {
            LlmProvider.Ollama => "neural-chat",
            LlmProvider.OpenRouter => "mistralai/mistral-7b-instruct:free",
            _ => "neural-chat"
        };
    }
}
