using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Prospera.Domain.Entities;
using Prospera.Domain.Enums;
using Prospera.Domain.Interfaces;
using Prospera.Domain.Common;
using Prospera.Application.DTOs;
using Prospera.Application.Common.Interfaces;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace Prospera.Application.Features.Recommendations.Commands;

/// <summary>
/// Handler for GenerateRecommendationCommand - generates AI-powered recommendations with structured portfolio analysis
/// Includes asset/liability breakdown, risks, opportunities, and coaching goal suggestions
/// Supports multiple LLM providers (Ollama, OpenRouter) with provider-aware fallbacks
/// </summary>
public class GenerateRecommendationCommandHandler : IRequestHandler<GenerateRecommendationCommand, InvestmentRecommendationDto>
{
    private readonly IInvestmentRecommendationRepository _recommendationRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMarketDataService _marketDataService;
    private readonly IFinancialDataService _financialDataService;
    private readonly IOllamaConfiguration _ollamaConfig;
    private readonly ICoachingService _coachingService;
    private readonly ILlmProviderFactory _llmProviderFactory;
    private readonly ILogger<GenerateRecommendationCommandHandler> _logger;

    public GenerateRecommendationCommandHandler(
        IInvestmentRecommendationRepository recommendationRepository,
        IApplicationDbContext dbContext,
        IMapper mapper,
        IMarketDataService marketDataService,
        IFinancialDataService financialDataService,
        IOllamaConfiguration ollamaConfig,
        ICoachingService coachingService,
        ILlmProviderFactory llmProviderFactory,
        ILogger<GenerateRecommendationCommandHandler> logger)
    {
        _recommendationRepository = recommendationRepository;
        _dbContext = dbContext;
        _mapper = mapper;
        _marketDataService = marketDataService;
        _financialDataService = financialDataService;
        _ollamaConfig = ollamaConfig;
        _coachingService = coachingService;
        _llmProviderFactory = llmProviderFactory;
        _logger = logger;
    }

    public async Task<InvestmentRecommendationDto> Handle(GenerateRecommendationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("=== Starting recommendation generation for user {UserId} ===", request.UserId);

            // === PHASE 1: Load financial data ===
            var userAssets = _dbContext.Assets.Where(a => a.UserId == request.UserId).ToList();
            var userLiabilities = _dbContext.Liabilities.Where(l => l.UserId == request.UserId).ToList();
            var userTransactions = _dbContext.Transactions.Where(t => t.UserId == request.UserId).ToList();

            // Calculate financial metrics
            var totalAssets = userAssets.Sum(a => a.CurrentValue);
            var totalLiabilities = userLiabilities.Sum(l => l.Amount);
            var netWorth = totalAssets - totalLiabilities;

            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var recentIncome = userTransactions
                .Where(t => t.Date >= thirtyDaysAgo && t.Type.ToString() == "Income")
                .Sum(t => t.Amount);
            var recentExpenses = userTransactions
                .Where(t => t.Date >= thirtyDaysAgo && t.Type.ToString() == "Expense")
                .Sum(t => t.Amount);

            var riskTolerance = CalculateRiskTolerance(netWorth, totalAssets, totalLiabilities, recentIncome, recentExpenses);

            // === PHASE 2: Compute portfolio breakdowns ===
            var assetsByType = ComputeAssetGroupings(userAssets, totalAssets);
            var liabilitiesByType = ComputeLiabilityGroupings(userLiabilities);
            var portfolioSummaryMarkdown = BuildPortfolioSummaryTable(assetsByType, liabilitiesByType, totalAssets, totalLiabilities);

            // === PHASE 3: Get market data ===
            var marketData = await _marketDataService.GetMarketTrendDataAsync(cancellationToken);
            if (marketData == null)
                marketData = new MarketTrendData { FetchedAt = DateTime.UtcNow };

            var recommendedTickers = new List<string> { "AAPL", "MSFT", "GOOGL", "AMZN", "TSLA", "BRK.B", "JNJ", "V", "WMT", "DIS" };
            List<StockValuationData> valuationData = new();
            InflationData inflationData = null;
            ExchangeRateData exchangeRates = null;
            PredictionData gdpPrediction = null;
            RealEstateData realEstateData = null;

            try
            {
                valuationData = await _financialDataService.GetMultipleStockValuationsAsync(recommendedTickers, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not fetch valuation data");
            }

            // Fetch financial API data for comprehensive analysis
            try
            {
                var countryCode = ExtractCountryCode(recentIncome) ?? "US";
                var inflationTask = _financialDataService.GetInflationDataAsync(countryCode, cancellationToken);
                var exchangeTask = _financialDataService.GetCurrentExchangeRatesAsync("USD", cancellationToken);
                var gdpTask = _financialDataService.GetGdpPredictionAsync(countryCode, cancellationToken);
                var reTask = countryCode == "US"
                    ? Task.FromResult<RealEstateData>(null)
                    : _financialDataService.GetGlobalRealEstateAsync(countryCode, cancellationToken);

                await Task.WhenAll(inflationTask, exchangeTask, gdpTask, reTask);

                inflationData = await inflationTask;
                exchangeRates = await exchangeTask;
                gdpPrediction = await gdpTask;
                realEstateData = await reTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not fetch additional financial data");
            }

            // === PHASE 4: Build enhanced prompt ===
            var aiPrompt = BuildPersonalizedAIPrompt(
                request.AnalysisContext,
                netWorth,
                totalAssets,
                totalLiabilities,
                recentIncome,
                riskTolerance,
                marketData,
                valuationData,
                assetsByType,
                liabilitiesByType,
                portfolioSummaryMarkdown,
                inflationData,
                exchangeRates,
                gdpPrediction,
                realEstateData);

            // === PHASE 5: Call LLM and parse response ===
            var provider = await _llmProviderFactory.GetProviderAsync(request.Provider);
            var model = !string.IsNullOrWhiteSpace(request.ModelName) ? request.ModelName : GetDefaultModelForProvider(request.Provider);

            string suggestedAllocation = GetDefaultAllocation();
            AllocationBreakdownDto allocationBreakdown = null;
            List<RecommendationAllocationItemDto> allocationItems = null;
            List<string> keyRisks = null;
            List<string> opportunities = null;
            List<string> suggestedActions = null;

            try
            {
                _logger.LogInformation("Calling {ProviderName} with enhanced prompt", provider.GetProviderName());
                var rawResponse = await provider.GenerateResponseAsync(aiPrompt, model, cancellationToken);

                // Parse richer response structure
                allocationBreakdown = ParseAllocationSection(rawResponse);
                suggestedAllocation = FormatAllocationString(allocationBreakdown);
                allocationItems = ParseRationaleSection(rawResponse, allocationBreakdown);
                keyRisks = ParseRisksSection(rawResponse);
                opportunities = ParseOpportunitiesSection(rawResponse);
                suggestedActions = ParseActionsSection(rawResponse);

                _logger.LogInformation("Parsed structured response - Allocation: {Allocation}", suggestedAllocation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing LLM response");
                allocationBreakdown = GetDefaultAllocationBreakdown();
                suggestedAllocation = GetDefaultAllocation();
                allocationItems = GetDefaultRationaleItems(allocationBreakdown);
                keyRisks = GetDefaultRisks();
                opportunities = GetDefaultOpportunities();
                suggestedActions = GetDefaultActions();
            }

            // === PHASE 6: Generate explanation ===
            var explanation = await GetExplanation(suggestedAllocation, request.AnalysisContext, riskTolerance, marketData, provider, model, cancellationToken);

            // === PHASE 7: Create and enrich recommendation ===
            var recommendation = new InvestmentRecommendation(request.UserId, suggestedAllocation, explanation, request.AnalysisContext, request.SessionId);

            var riskProfile = riskTolerance switch
            {
                < 0.33m => "Conservative (Low Risk)",
                < 0.66m => "Moderate (Balanced Risk)",
                _ => "Aggressive (High Risk)"
            };

            var allocationItemEntities = allocationItems
                .Select(item => new RecommendationAllocationItem
                {
                    Category = item.Category,
                    Percentage = item.Percentage,
                    Rationale = item.Rationale
                })
                .ToList();

            recommendation.Enrich(
                riskProfile: riskProfile,
                portfolioSummary: portfolioSummaryMarkdown,
                allocationItems: allocationItemEntities,
                suggestedActions: suggestedActions,
                keyRisks: keyRisks,
                opportunities: opportunities);

            // === PHASE 8: Save and map to DTO ===
            await _recommendationRepository.AddAsync(recommendation);

            var dto = _mapper.Map<InvestmentRecommendationDto>(recommendation);

            // Populate profile
            dto.Profile = new PortfolioProfileDto
            {
                RiskProfile = riskProfile,
                NetWorth = netWorth,
                TotalAssets = totalAssets,
                TotalLiabilities = totalLiabilities,
                AssetBreakdown = MapAssetGroupsToDto(assetsByType),
                LiabilityBreakdown = MapLiabilityGroupsToDto(liabilitiesByType)
            };

            // Populate allocation
            dto.Allocation = new AllocationBreakdownDto
            {
                Stocks = allocationBreakdown.Stocks,
                Bonds = allocationBreakdown.Bonds,
                RealEstate = allocationBreakdown.RealEstate,
                Crypto = allocationBreakdown.Crypto,
                Cash = allocationBreakdown.Cash,
                Details = allocationItems
            };

            // Populate structured lists
            dto.SuggestedActions = suggestedActions;
            dto.KeyRisks = keyRisks;
            dto.Opportunities = opportunities;
            dto.SuggestedCoachingGoals = GenerateCoachingGoals(suggestedAllocation, totalLiabilities, request.AnalysisContext);

            _logger.LogInformation("=== Recommendation generated successfully ===");
            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERROR in recommendation generation: {ErrorType}", ex.GetType().Name);
            return await GenerateFallbackRecommendation(request);
        }
    }

    #region Portfolio Grouping

    private List<AssetGroupSummary> ComputeAssetGroupings(List<Asset> userAssets, decimal totalAssets)
    {
        return userAssets
            .GroupBy(a => a.Type)
            .Select(group => new AssetGroupSummary
            {
                AssetType = group.Key,
                TotalValue = group.Sum(a => a.CurrentValue),
                Count = group.Count(),
                Names = group.Select(a => a.Name).ToList(),
                PortfolioPercentage = totalAssets > 0
                    ? (group.Sum(a => a.CurrentValue) / totalAssets) * 100m
                    : 0m
            })
            .OrderByDescending(a => a.TotalValue)
            .ToList();
    }

    private List<LiabilityGroupSummary> ComputeLiabilityGroupings(List<Liability> userLiabilities)
    {
        return userLiabilities
            .GroupBy(l => l.Type)
            .Select(group => new LiabilityGroupSummary
            {
                LiabilityType = group.Key,
                TotalAmount = group.Sum(l => l.Amount),
                Count = group.Count(),
                Names = group.Select(l => l.Name).ToList()
            })
            .OrderByDescending(l => l.TotalAmount)
            .ToList();
    }

    private string BuildPortfolioSummaryTable(
        List<AssetGroupSummary> assetsByType,
        List<LiabilityGroupSummary> liabilitiesByType,
        decimal totalAssets,
        decimal totalLiabilities)
    {
        var sb = new StringBuilder();

        sb.AppendLine("## CURRENT PORTFOLIO");
        sb.AppendLine($"**Total Assets:** ${totalAssets:F2}");
        sb.AppendLine($"**Total Liabilities:** ${totalLiabilities:F2}");
        sb.AppendLine($"**Net Worth:** ${totalAssets - totalLiabilities:F2}");
        sb.AppendLine();

        sb.AppendLine("### ASSETS BY TYPE");
        sb.AppendLine("| Type | Value | % of Portfolio | Count | Items |");
        sb.AppendLine("|------|-------|---|-------|-------|");

        foreach (var asset in assetsByType)
        {
            var names = string.Join(", ", asset.Names);
            if (names.Length > 50)
                names = names.Substring(0, 47) + "...";

            sb.AppendLine($"| {asset.AssetType} | ${asset.TotalValue:F2} | {asset.PortfolioPercentage:F1}% | {asset.Count} | {names} |");
        }

        sb.AppendLine();
        sb.AppendLine("### LIABILITIES BY TYPE");
        sb.AppendLine("| Type | Amount | Count | Items |");
        sb.AppendLine("|------|--------|-------|-------|");

        foreach (var liability in liabilitiesByType)
        {
            var names = string.Join(", ", liability.Names);
            if (names.Length > 50)
                names = names.Substring(0, 47) + "...";

            sb.AppendLine($"| {liability.LiabilityType} | ${liability.TotalAmount:F2} | {liability.Count} | {names} |");
        }

        return sb.ToString();
    }

    #endregion

    #region Prompt Building

    private string ExtractCountryCode(decimal income)
    {
        // Simple heuristic: for now default to US, could be enhanced with user profile data
        return "US";
    }

    private string BuildFinancialContextFromApis(
        InflationData inflationData,
        ExchangeRateData exchangeRates,
        PredictionData gdpPrediction,
        RealEstateData realEstateData)
    {
        var sb = new StringBuilder();

        // Inflation context
        if (inflationData != null && inflationData.CurrentRate >= 0)
        {
            sb.AppendLine($"INFLATION RATE: {inflationData.CurrentRate:F2}% ({inflationData.CountryName})");
            if (inflationData.CurrentRate > 5)
                sb.AppendLine("  → HIGH inflation: Prioritize inflation-protected assets and growth investments");
            else if (inflationData.CurrentRate > 2)
                sb.AppendLine("  → MODERATE inflation: Balance cash with growth-oriented allocations");
            else
                sb.AppendLine("  → LOW inflation: Good environment for fixed-income investments");
        }

        // Exchange rate context
        if (exchangeRates != null && exchangeRates.Rates.Any())
        {
            sb.AppendLine($"EXCHANGE RATES: Base currency {exchangeRates.BaseCurrency}");
            foreach (var rate in exchangeRates.Rates.Take(3))
            {
                sb.AppendLine($"  {rate.Key}: {rate.Value:F4}");
            }
            sb.AppendLine("  → Consider currency diversification if international exposure planned");
        }

        // GDP and economic outlook
        if (gdpPrediction != null && !string.IsNullOrEmpty(gdpPrediction.Reasoning))
        {
            sb.AppendLine($"ECONOMIC OUTLOOK: {gdpPrediction.Reasoning}");
            if (gdpPrediction.PredictedValue > 2)
                sb.AppendLine("  → Strong growth forecast: Consider increasing equity allocation");
            else if (gdpPrediction.PredictedValue < 0)
                sb.AppendLine("  → Weak growth forecast: Increase defensive positions and cash reserves");
            else
                sb.AppendLine("  → Moderate growth: Maintain balanced allocation");
        }

        // Real estate market context
        if (realEstateData != null && realEstateData.AveragePricePerSqm > 0)
        {
            sb.AppendLine($"REAL ESTATE MARKET ({realEstateData.Location}): ");
            sb.AppendLine($"  Average Price: ${realEstateData.AveragePricePerSqm:F2}/sqm");
            sb.AppendLine($"  Annual Growth: {realEstateData.AverageAnnualGrowth:F2}%");
            if (realEstateData.RentalYield.HasValue)
                sb.AppendLine($"  Rental Yield: {realEstateData.RentalYield:F2}%");
            sb.AppendLine("  → Real estate showing strong fundamentals for diversification");
        }

        return sb.ToString();
    }

    private string GetDefaultModelForProvider(LlmProvider provider)
    {
        return provider switch
        {
            LlmProvider.Ollama => _ollamaConfig.Model,
            LlmProvider.OpenRouter => "openai/gpt-3.5-turbo",
            _ => _ollamaConfig.Model
        };
    }

    private decimal CalculateRiskTolerance(decimal netWorth, decimal totalAssets, decimal totalLiabilities, decimal income, decimal expenses)
    {
        decimal riskScore = 0.5m;
        var debtRatio = totalAssets > 0 ? totalLiabilities / totalAssets : 1m;
        riskScore += (1m - Math.Min(debtRatio, 1m)) * 0.2m;

        if (income > 0)
        {
            var savingsRate = (income - expenses) / income;
            riskScore += Math.Max(0, Math.Min(savingsRate, 1m)) * 0.2m;
        }

        if (netWorth > 50000)
            riskScore += 0.1m;
        else if (netWorth < 0)
            riskScore -= 0.2m;

        return Math.Max(0m, Math.Min(riskScore, 1m));
    }

    private string BuildPersonalizedAIPrompt(
        string userContext,
        decimal netWorth,
        decimal totalAssets,
        decimal totalLiabilities,
        decimal monthlyIncome,
        decimal riskTolerance,
        MarketTrendData marketData,
        List<StockValuationData> valuationData,
        List<AssetGroupSummary> assetsByType,
        List<LiabilityGroupSummary> liabilitiesByType,
        string portfolioSummaryMarkdown,
        InflationData inflationData,
        ExchangeRateData exchangeRates,
        PredictionData gdpPrediction,
        RealEstateData realEstateData)
    {
        var riskProfile = riskTolerance switch
        {
            < 0.33m => "Conservative (Low Risk)",
            < 0.66m => "Moderate (Balanced Risk)",
            _ => "Aggressive (High Risk)"
        };

        var marketCondition = marketData.StockMarketTrend switch
        {
            > 0.05m => "bullish (positive momentum)",
            < -0.05m => "bearish (negative momentum)",
            _ => "neutral (stable)"
        };

        var valuationContext = BuildValuationContext(valuationData);

        // Build financial context from API data
        var financialContext = BuildFinancialContextFromApis(inflationData, exchangeRates, gdpPrediction, realEstateData);

        return $@"You are a professional financial COACH and mentor (not just an advisor).
Your recommendations should feel personalized, motivational, and focused on building sustainable wealth for the long term.

INVESTOR PROFILE:
- Investment Goal: {userContext}
- Net Worth: ${netWorth:F2}
- Total Assets: ${totalAssets:F2}
- Total Liabilities: ${totalLiabilities:F2}
- Monthly Income: ${monthlyIncome:F2}
- Risk Profile: {riskProfile} (Risk Tolerance Score: {riskTolerance:P0})
- Debt-to-Asset Ratio: {(totalAssets > 0 ? (totalLiabilities / totalAssets) : 0):P1}

{portfolioSummaryMarkdown}

CURRENT MARKET CONDITIONS:
- Stock Market: {marketCondition} (S&P 500 Change: {marketData.StockMarketTrend:P2})
- Bitcoin Price: ${marketData.CryptoPriceUSD:F2} USD
- Market Data as of: {marketData.FetchedAt:G}

MACROECONOMIC & FINANCIAL DATA:
{financialContext}

STOCK VALUATIONS (Data-Driven Insights):
{valuationContext}

COACHING PHILOSOPHY:
You are coaching this person to:
1. Build wealth systematically through diversified investments
2. Manage both assets AND liabilities holistically
3. Create sustainable financial habits that last
4. Make informed decisions aligned with their risk tolerance
5. Invest in quality stocks with good valuations
6. Take action with confidence and clear next steps

ALLOCATION TASK:
Suggest a portfolio allocation that will set this person up for long-term success:
1. Stocks (equities, growth)
2. Bonds (fixed income, stability)
3. Real Estate (property, tangible assets)
4. Crypto (digital assets, high risk/reward)
5. Cash (emergency fund, liquidity)

RESPONSE FORMAT:
Provide your analysis in these clear sections:

1. PORTFOLIO ASSESSMENT
[Your 2-3 sentence assessment of their current allocation health and what needs rebalancing]

2. RECOMMENDED ALLOCATION
Stocks: X%
Bonds: Y%
Real Estate: Z%
Crypto: W%
Cash: V%

3. RATIONALE
- Stocks: [Why X% makes sense for their situation and risk profile]
- Bonds: [Why Y% - explain the stability benefit]
- Real Estate: [Why Z% - tangible assets for diversification]
- Crypto: [Why W% - calculated risk for growth]
- Cash: [Why V% - emergency liquidity]

4. KEY RISKS
- [Risk 1: something about their current portfolio composition]
- [Risk 2: market-related risk based on current conditions]
- [Risk 3: specific to their financial situation]

5. OPPORTUNITIES
- [Opportunity 1: undervalued asset class they're missing]
- [Opportunity 2: specific allocation gap that could boost returns]
- [Opportunity 3: risk management improvement opportunity]

6. TOP ACTIONS
1. [Specific first action with amount/timeframe]
2. [Action 2]
3. [Action 3]
4. [Action 4]
5. [Action 5]
6. [Action 6 optional]

Remember: This is personal coaching with data-driven insights. All percentages must sum to 100%.";
    }

    private string BuildValuationContext(List<StockValuationData> valuationData)
    {
        if (valuationData == null || valuationData.Count == 0)
            return "Stock valuation data not available.";

        var undervalued = valuationData.Where(v => v.MarginOfSafety.HasValue && v.MarginOfSafety.Value > 0.2m).ToList();
        var fair = valuationData.Where(v => v.MarginOfSafety.HasValue && v.MarginOfSafety.Value >= 0.1m && v.MarginOfSafety.Value <= 0.2m).ToList();
        var overvalued = valuationData.Where(v => v.MarginOfSafety.HasValue && v.MarginOfSafety.Value < 0.1m).ToList();

        var context = new StringBuilder();
        context.AppendLine("Stock Market Analysis:");

        if (undervalued.Any())
        {
            context.AppendLine($"  UNDERVALUED OPPORTUNITIES ({undervalued.Count}): ");
            foreach (var stock in undervalued.Take(3))
            {
                context.AppendLine($"    - {stock.Ticker}: P/E {stock.PeRatio:F2}, Fair Value ${stock.FairValue:F2}, " +
                    $"Margin of Safety {stock.MarginOfSafety:P0}, Dividend Yield {stock.DividendYield:P2}");
            }
        }

        if (fair.Any())
        {
            context.AppendLine($"  FAIRLY VALUED STOCKS ({fair.Count}): ");
            foreach (var stock in fair.Take(3))
            {
                context.AppendLine($"    - {stock.Ticker}: P/E {stock.PeRatio:F2}, Price Target ${stock.PriceTarget:F2}, " +
                    $"Dividend Yield {stock.DividendYield:P2}");
            }
        }

        if (overvalued.Any())
            context.AppendLine($"  OVERVALUED STOCKS ({overvalued.Count}): Approach with caution or wait for better entry points.");

        context.AppendLine($"  RECOMMENDATION: Focus allocation on undervalued and fairly valued stocks for optimal risk/reward.");

        return context.ToString();
    }

    #endregion

    #region Response Parsing

    private AllocationBreakdownDto ParseAllocationSection(string response)
    {
        var breakdown = new AllocationBreakdownDto();

        try
        {
            var section2Start = response.IndexOf("2. RECOMMENDED ALLOCATION", StringComparison.OrdinalIgnoreCase);
            if (section2Start < 0)
                return GetDefaultAllocationBreakdown();

            var section3Start = response.IndexOf("3. RATIONALE", section2Start, StringComparison.OrdinalIgnoreCase);
            var section2Content = section3Start > 0
                ? response.Substring(section2Start, section3Start - section2Start)
                : response.Substring(section2Start);

            var regex = new Regex(@"(Stocks|Bonds|Real\s*Estate|Crypto|Cash)\s*:\s*(\d+)\s*%", RegexOptions.IgnoreCase);
            var matches = regex.Matches(section2Content);

            foreach (Match match in matches)
            {
                var category = match.Groups[1].Value.Trim();
                var percentage = int.Parse(match.Groups[2].Value);

                switch (category.ToLower().Replace(" ", ""))
                {
                    case "stocks":
                        breakdown.Stocks = percentage;
                        break;
                    case "bonds":
                        breakdown.Bonds = percentage;
                        break;
                    case "realestate":
                        breakdown.RealEstate = percentage;
                        break;
                    case "crypto":
                        breakdown.Crypto = percentage;
                        break;
                    case "cash":
                        breakdown.Cash = percentage;
                        break;
                }
            }

            return breakdown;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse allocation section, using fallback");
            return GetDefaultAllocationBreakdown();
        }
    }

    private List<RecommendationAllocationItemDto> ParseRationaleSection(
        string response,
        AllocationBreakdownDto allocation)
    {
        var items = new List<RecommendationAllocationItemDto>();

        try
        {
            var section3Start = response.IndexOf("3. RATIONALE", StringComparison.OrdinalIgnoreCase);
            if (section3Start < 0)
                return GetDefaultRationaleItems(allocation);

            var section4Start = response.IndexOf("4. KEY RISKS", section3Start, StringComparison.OrdinalIgnoreCase);
            var section3Content = section4Start > 0
                ? response.Substring(section3Start, section4Start - section3Start)
                : response.Substring(section3Start);

            var bulletPattern = new Regex(
                @"-\s*(Stocks|Bonds|Real\s*Estate|Crypto|Cash)\s*:\s*(.+?)(?=\n-|$)",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var matches = bulletPattern.Matches(section3Content);

            foreach (Match match in matches)
            {
                var category = match.Groups[1].Value.Trim();
                var rationale = match.Groups[2].Value.Trim();
                var percentage = GetPercentageForCategory(category, allocation);

                items.Add(new RecommendationAllocationItemDto
                {
                    Category = NormalizeCategory(category),
                    Percentage = percentage,
                    Rationale = rationale
                });
            }

            if (items.Count < 5)
                items = GetDefaultRationaleItems(allocation);

            return items;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse rationale section");
            return GetDefaultRationaleItems(allocation);
        }
    }

    private List<string> ParseRisksSection(string response)
    {
        var risks = new List<string>();

        try
        {
            var section4Start = response.IndexOf("4. KEY RISKS", StringComparison.OrdinalIgnoreCase);
            if (section4Start < 0)
                return GetDefaultRisks();

            var section5Start = response.IndexOf("5. OPPORTUNITIES", section4Start, StringComparison.OrdinalIgnoreCase);
            var section4Content = section5Start > 0
                ? response.Substring(section4Start, section5Start - section4Start)
                : response.Substring(section4Start);

            var bulletPattern = new Regex(@"-\s*(.+?)(?=\n-|\n[0-9]|$)", RegexOptions.Singleline);
            var matches = bulletPattern.Matches(section4Content);

            foreach (Match match in matches)
            {
                var risk = match.Groups[1].Value.Trim().Replace("\n", " ");
                if (!string.IsNullOrWhiteSpace(risk) && risk.Length > 10)
                {
                    risks.Add(risk);
                    if (risks.Count >= 3)
                        break;
                }
            }

            while (risks.Count < 3)
                risks.AddRange(GetDefaultRisks().Skip(risks.Count));

            return risks.Take(3).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse risks section");
            return GetDefaultRisks();
        }
    }

    private List<string> ParseOpportunitiesSection(string response)
    {
        var opportunities = new List<string>();

        try
        {
            var section5Start = response.IndexOf("5. OPPORTUNITIES", StringComparison.OrdinalIgnoreCase);
            if (section5Start < 0)
                return GetDefaultOpportunities();

            var section6Start = response.IndexOf("6. TOP ACTIONS", section5Start, StringComparison.OrdinalIgnoreCase);
            var section5Content = section6Start > 0
                ? response.Substring(section5Start, section6Start - section5Start)
                : response.Substring(section5Start);

            var bulletPattern = new Regex(@"-\s*(.+?)(?=\n-|\n[0-9]|$)", RegexOptions.Singleline);
            var matches = bulletPattern.Matches(section5Content);

            foreach (Match match in matches)
            {
                var opportunity = match.Groups[1].Value.Trim().Replace("\n", " ");
                if (!string.IsNullOrWhiteSpace(opportunity) && opportunity.Length > 10)
                {
                    opportunities.Add(opportunity);
                    if (opportunities.Count >= 3)
                        break;
                }
            }

            while (opportunities.Count < 3)
                opportunities.AddRange(GetDefaultOpportunities().Skip(opportunities.Count));

            return opportunities.Take(3).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse opportunities section");
            return GetDefaultOpportunities();
        }
    }

    private List<string> ParseActionsSection(string response)
    {
        var actions = new List<string>();

        try
        {
            var section6Start = response.IndexOf("6. TOP ACTIONS", StringComparison.OrdinalIgnoreCase);
            if (section6Start < 0)
                return GetDefaultActions();

            var section6Content = response.Substring(section6Start);
            var numberedPattern = new Regex(
                @"^\s*[0-9]+\.\s*(.+?)(?=\n[0-9]+\.|\n\n|$)",
                RegexOptions.Multiline | RegexOptions.Singleline);

            var matches = numberedPattern.Matches(section6Content);

            foreach (Match match in matches)
            {
                var action = match.Groups[1].Value.Trim().Replace("\n", " ");
                if (!string.IsNullOrWhiteSpace(action) && action.Length > 5)
                {
                    actions.Add(action);
                    if (actions.Count >= 6)
                        break;
                }
            }

            if (actions.Count == 0)
                actions = GetDefaultActions();

            return actions;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse actions section");
            return GetDefaultActions();
        }
    }

    private int GetPercentageForCategory(string category, AllocationBreakdownDto allocation)
    {
        var normalized = category.ToLower().Replace(" ", "");
        return normalized switch
        {
            "stocks" => allocation.Stocks,
            "bonds" => allocation.Bonds,
            "realestate" => allocation.RealEstate,
            "crypto" => allocation.Crypto,
            "cash" => allocation.Cash,
            _ => 0
        };
    }

    private string NormalizeCategory(string category)
    {
        return category.ToLower().Replace(" ", "") switch
        {
            "stocks" => "Stocks",
            "bonds" => "Bonds",
            "realestate" => "Real Estate",
            "crypto" => "Crypto",
            "cash" => "Cash",
            _ => category
        };
    }

    #endregion

    #region Coaching Goals & Formatting

    private List<CoachingGoalSuggestionDto> GenerateCoachingGoals(
        string suggestedAllocation,
        decimal totalLiabilities,
        string? userContext)
    {
        var goals = new List<CoachingGoalSuggestionDto>
        {
            new() {
                Title = "Portfolio Rebalancing Plan",
                Description = "Align your current investments with recommended allocation",
                CoachingGoal = $"Rebalance to: {suggestedAllocation}"
            }
        };

        if (totalLiabilities > 0)
        {
            goals.Add(new CoachingGoalSuggestionDto
            {
                Title = "Debt Reduction Path",
                Description = "Create a strategic plan to eliminate liabilities",
                CoachingGoal = $"Reduce debt from ${totalLiabilities:F0}"
            });
        }

        if (!string.IsNullOrWhiteSpace(userContext))
        {
            goals.Add(new CoachingGoalSuggestionDto
            {
                Title = "Personal Goal",
                Description = "Your specific financial objective",
                CoachingGoal = userContext.Trim()
            });
        }

        return goals;
    }

    private string FormatAllocationString(AllocationBreakdownDto breakdown)
    {
        return $"{breakdown.Stocks}% Stocks, {breakdown.Bonds}% Bonds, {breakdown.RealEstate}% Real Estate, {breakdown.Crypto}% Crypto, {breakdown.Cash}% Cash";
    }

    private List<AssetGroupSummaryDto> MapAssetGroupsToDto(List<AssetGroupSummary> assetsByType)
    {
        return assetsByType
            .Select(group => new AssetGroupSummaryDto
            {
                Type = group.AssetType.ToString(),
                TotalValue = group.TotalValue,
                PortfolioPercentage = group.PortfolioPercentage,
                Count = group.Count,
                Names = group.Names
            })
            .ToList();
    }

    private List<LiabilityGroupSummaryDto> MapLiabilityGroupsToDto(List<LiabilityGroupSummary> liabilitiesByType)
    {
        return liabilitiesByType
            .Select(group => new LiabilityGroupSummaryDto
            {
                Type = group.LiabilityType.ToString(),
                TotalAmount = group.TotalAmount,
                Count = group.Count,
                Names = group.Names
            })
            .ToList();
    }

    #endregion

    #region Helper Methods & Fallbacks

    private async Task<string> GetExplanation(
        string allocation,
        string userContext,
        decimal riskTolerance,
        MarketTrendData marketData,
        ILlmProviderService provider,
        string model,
        CancellationToken cancellationToken)
    {
        try
        {
            var riskProfile = riskTolerance switch
            {
                < 0.33m => "conservative",
                < 0.66m => "moderate",
                _ => "aggressive"
            };

            var prompt = $@"You are a financial coach providing a personalized explanation for a portfolio allocation.

PORTFOLIO ALLOCATION: {allocation}

INVESTOR PROFILE:
- Investment Goal: {userContext}
- Risk Profile: {riskProfile}
- Current Market: Stock Market {(marketData.StockMarketTrend > 0 ? "bullish" : "bearish")}, Bitcoin at ${marketData.CryptoPriceUSD:F2}

Provide a coaching-style explanation (3-4 sentences) that validates their investment choices and motivates them to take action.";

            var response = await provider.GenerateResponseAsync(prompt, model, cancellationToken);
            return response?.Trim() ?? GetPersonalizedExplanation(allocation, riskTolerance, marketData);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error generating explanation");
            return GetPersonalizedExplanation(allocation, riskTolerance, marketData);
        }
    }

    private string GetPersonalizedExplanation(string allocation, decimal riskTolerance, MarketTrendData marketData)
    {
        var riskProfile = riskTolerance switch
        {
            < 0.33m => "conservative, safety-first approach",
            < 0.66m => "balanced approach to growth and protection",
            _ => "growth-focused, higher risk tolerance approach"
        };

        var marketOutlook = marketData.StockMarketTrend > 0
            ? "with positive market momentum in your favor"
            : "in a cautious market environment where stability matters";

        return $"This allocation ({allocation}) is your personalized roadmap for building wealth with a {riskProfile}. " +
               $"{marketOutlook}, this diversified mix positions you for success by balancing growth opportunities with protection. " +
               "This is designed for your unique financial situation and goals.";
    }

    private AllocationBreakdownDto GetDefaultAllocationBreakdown()
    {
        return new AllocationBreakdownDto
        {
            Stocks = 40,
            Bonds = 30,
            RealEstate = 15,
            Crypto = 10,
            Cash = 5
        };
    }

    private string GetDefaultAllocation()
    {
        return "40% Stocks, 30% Bonds, 15% Real Estate, 10% Crypto, 5% Cash";
    }

    private List<RecommendationAllocationItemDto> GetDefaultRationaleItems(AllocationBreakdownDto allocation)
    {
        return new List<RecommendationAllocationItemDto>
        {
            new() { Category = "Stocks", Percentage = allocation.Stocks, Rationale = "Growth-focused investments based on your risk tolerance and time horizon." },
            new() { Category = "Bonds", Percentage = allocation.Bonds, Rationale = "Stable income and portfolio stability to balance equity volatility." },
            new() { Category = "Real Estate", Percentage = allocation.RealEstate, Rationale = "Tangible assets for long-term value and diversification." },
            new() { Category = "Crypto", Percentage = allocation.Crypto, Rationale = "Strategic exposure to emerging asset class for growth potential." },
            new() { Category = "Cash", Percentage = allocation.Cash, Rationale = "Emergency fund and liquidity for opportunities and contingencies." }
        };
    }

    private List<string> GetDefaultRisks()
    {
        return new List<string>
        {
            "Market volatility and economic downturns could impact portfolio value.",
            "Concentration risk in specific asset classes requires active monitoring.",
            "Interest rate changes may affect bond and real estate valuations."
        };
    }

    private List<string> GetDefaultOpportunities()
    {
        return new List<string>
        {
            "Rebalance portfolio to match recommended allocations and capitalize on market movements.",
            "Explore low-cost index funds and ETFs to reduce fees and improve returns.",
            "Build emergency fund to 6 months of expenses for greater financial security."
        };
    }

    private List<string> GetDefaultActions()
    {
        return new List<string>
        {
            "Open or review investment accounts (brokerage, 401k, IRA)",
            "Set up automatic monthly contributions to match investment plan",
            "Implement recommended allocation across your accounts",
            "Review and reduce high-interest debt (credit cards, personal loans)",
            "Build emergency fund to cover 3-6 months of expenses",
            "Schedule quarterly review to monitor progress against goals"
        };
    }

    private async Task<InvestmentRecommendationDto> GenerateFallbackRecommendation(GenerateRecommendationCommand request)
    {
        _logger.LogInformation("Generating data-driven fallback recommendation for user {UserId}", request.UserId);

        // Load financial data for smarter fallback
        var userAssets = _dbContext.Assets.Where(a => a.UserId == request.UserId).ToList();
        var userLiabilities = _dbContext.Liabilities.Where(l => l.UserId == request.UserId).ToList();
        var userTransactions = _dbContext.Transactions.Where(t => t.UserId == request.UserId).ToList();

        var totalAssets = userAssets.Sum(a => a.CurrentValue);
        var totalLiabilities = userLiabilities.Sum(l => l.Amount);
        var netWorth = totalAssets - totalLiabilities;

        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var recentIncome = userTransactions
            .Where(t => t.Date >= thirtyDaysAgo && t.Type.ToString() == "Income")
            .Sum(t => t.Amount);

        var riskTolerance = CalculateRiskTolerance(netWorth, totalAssets, totalLiabilities, recentIncome, 0);

        // Generate data-driven allocation based on risk tolerance
        var allocationBreakdown = GenerateDataDrivenAllocation(riskTolerance, netWorth, totalAssets, totalLiabilities);
        var allocation = FormatAllocationString(allocationBreakdown);

        // Create intelligent explanation based on situation
        var explanation = GenerateSmartExplanation(allocationBreakdown, riskTolerance, netWorth, totalAssets, totalLiabilities);

        var recommendation = new InvestmentRecommendation(request.UserId, allocation, explanation, request.AnalysisContext, request.SessionId);

        var riskProfile = riskTolerance switch
        {
            < 0.33m => "Conservative (Low Risk)",
            < 0.66m => "Moderate (Balanced Risk)",
            _ => "Aggressive (High Risk)"
        };

        recommendation.Enrich(
            riskProfile: riskProfile,
            portfolioSummary: "",
            allocationItems: allocationBreakdown.Details
                .Select(d => new RecommendationAllocationItem
                {
                    Category = d.Category,
                    Percentage = d.Percentage,
                    Rationale = d.Rationale
                })
                .ToList(),
            suggestedActions: new List<string>
            {
                "Review current portfolio allocation",
                "Rebalance assets to match recommended allocation",
                "Set up automatic monthly savings/investments",
                "Evaluate and optimize liabilities",
                "Create long-term financial plan with milestones"
            },
            keyRisks: new List<string>
            {
                "Portfolio concentration risk in current holdings",
                $"Debt-to-asset ratio of {(totalAssets > 0 ? (totalLiabilities / totalAssets) : 0):P1}",
                "Market volatility exposure based on current allocation"
            },
            opportunities: new List<string>
            {
                "Diversification across underrepresented asset classes",
                "Tax-efficient rebalancing opportunities",
                "Opportunity to reduce debt burden systematically"
            });

        await _recommendationRepository.AddAsync(recommendation);
        return _mapper.Map<InvestmentRecommendationDto>(recommendation);
    }

    private AllocationBreakdownDto GenerateDataDrivenAllocation(decimal riskTolerance, decimal netWorth, decimal totalAssets, decimal totalLiabilities)
    {
        // Generate allocation based on risk profile and financial situation
        int stocks, bonds, realEstate, crypto, cash;

        if (riskTolerance < 0.33m) // Conservative
        {
            stocks = 30;
            bonds = 40;
            realEstate = 20;
            crypto = 0;
            cash = 10;
        }
        else if (riskTolerance < 0.66m) // Moderate
        {
            stocks = 45;
            bonds = 25;
            realEstate = 20;
            crypto = 5;
            cash = 5;
        }
        else // Aggressive
        {
            stocks = 60;
            bonds = 15;
            realEstate = 15;
            crypto = 7;
            cash = 3;
        }

        // Adjust for debt burden: if high debt, reduce risk assets
        if (totalAssets > 0 && totalLiabilities / totalAssets > 0.5m)
        {
            var reduction = (int)Math.Min(20, totalLiabilities / totalAssets * 30);
            stocks -= reduction;
            crypto -= reduction / 2;
            bonds += reduction;
            cash += reduction / 2;
        }

        // Ensure totals sum to 100
        var total = stocks + bonds + realEstate + crypto + cash;
        if (total != 100)
        {
            cash += (100 - total);
        }

        return new AllocationBreakdownDto
        {
            Stocks = stocks,
            Bonds = bonds,
            RealEstate = realEstate,
            Crypto = crypto,
            Cash = cash,
            Details = new List<RecommendationAllocationItemDto>
            {
                new() { Category = "Stocks", Percentage = stocks, Rationale = $"Growth-oriented position ({stocks}%) based on {(riskTolerance > 0.66m ? "aggressive" : riskTolerance > 0.33m ? "moderate" : "conservative")} risk tolerance" },
                new() { Category = "Bonds", Percentage = bonds, Rationale = $"Fixed income stability ({bonds}%) for consistent returns and downside protection" },
                new() { Category = "Real Estate", Percentage = realEstate, Rationale = $"Tangible asset diversification ({realEstate}%) for inflation hedge" },
                new() { Category = "Crypto", Percentage = crypto, Rationale = $"Alternative growth asset ({crypto}%) for portfolio diversification" },
                new() { Category = "Cash", Percentage = cash, Rationale = $"Emergency liquidity and flexibility ({cash}%) for financial security" }
            }
        };
    }

    private string GenerateSmartExplanation(AllocationBreakdownDto allocation, decimal riskTolerance, decimal netWorth, decimal totalAssets, decimal totalLiabilities)
    {
        var riskProfile = riskTolerance switch
        {
            < 0.33m => "conservative",
            < 0.66m => "balanced",
            _ => "growth-oriented"
        };

        var debtContext = totalAssets > 0 && totalLiabilities / totalAssets > 0.3m
            ? " Given your current debt level, this allocation emphasizes stability while building wealth."
            : " This allocation balances growth with stability for long-term wealth building.";

        return $"This {riskProfile} allocation ({allocation.Stocks}% stocks, {allocation.Bonds}% bonds, {allocation.RealEstate}% real estate, {allocation.Crypto}% crypto, {allocation.Cash}% cash) is designed for your financial situation.{debtContext} Focus on consistent rebalancing and maintaining discipline with your investment strategy.";
    }

    #endregion

    #region Internal Data Classes

    private class AssetGroupSummary
    {
        public AssetType AssetType { get; set; }
        public decimal TotalValue { get; set; }
        public decimal PortfolioPercentage { get; set; }
        public int Count { get; set; }
        public List<string> Names { get; set; } = new();
    }

    private class LiabilityGroupSummary
    {
        public LiabilityType LiabilityType { get; set; }
        public decimal TotalAmount { get; set; }
        public int Count { get; set; }
        public List<string> Names { get; set; } = new();
    }

    #endregion
}

/// <summary>
/// Ollama API Request Model
/// </summary>
public class OllamaRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; }

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }
}

/// <summary>
/// Ollama API Response Model
/// </summary>
public class OllamaResponse
{
    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("response")]
    public string Response { get; set; }

    [JsonPropertyName("done")]
    public bool Done { get; set; }
}
