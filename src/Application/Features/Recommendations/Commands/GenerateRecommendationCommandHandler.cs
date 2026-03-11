using MediatR;
using AutoMapper;
using Prospera.Domain.Entities;
using Prospera.Domain.Interfaces;
using Prospera.Application.DTOs;
using Prospera.Application.Common.Interfaces;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Prospera.Application.Features.Recommendations.Commands;

/// <summary>
/// Handler for GenerateRecommendationCommand - generates AI-powered recommendations
/// Integrates with:
/// - Ollama AI (llama3.2 model) for intelligent allocation strategy
/// - User financial data for personalized recommendations
/// </summary>
public class GenerateRecommendationCommandHandler : IRequestHandler<GenerateRecommendationCommand, InvestmentRecommendationDto>
{
    private readonly IInvestmentRecommendationRepository _recommendationRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly HttpClient _httpClient;

    public GenerateRecommendationCommandHandler(
        IInvestmentRecommendationRepository recommendationRepository,
        IApplicationDbContext dbContext,
        IMapper mapper,
        HttpClient httpClient)
    {
        _recommendationRepository = recommendationRepository;
        _dbContext = dbContext;
        _mapper = mapper;
        _httpClient = httpClient;
    }

    public async Task<InvestmentRecommendationDto> Handle(GenerateRecommendationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get user's financial profile from database
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

            // Build context for AI with user financial data
            var aiPrompt = BuildAIPrompt(request.AnalysisContext, netWorth, totalAssets, recentIncome);

            // Call Ollama AI for intelligent recommendation
            var suggestedAllocation = await GetOllamaAllocation(aiPrompt, cancellationToken);
            var explanation = await GetOllamaExplanation(suggestedAllocation, request.AnalysisContext, cancellationToken);

            // Create recommendation entity
            var recommendation = new InvestmentRecommendation(
                request.UserId,
                suggestedAllocation,
                explanation);

            // Save to repository
            await _recommendationRepository.AddAsync(recommendation);

            // Map and return
            return _mapper.Map<InvestmentRecommendationDto>(recommendation);
        }
        catch (Exception)
        {
            // Fallback to default if AI fails
            return await GenerateFallbackRecommendation(request);
        }
    }

    /// <summary>
    /// Build AI prompt with user's financial data
    /// </summary>
    private string BuildAIPrompt(string userContext, decimal netWorth, decimal totalAssets, decimal monthlyIncome)
    {
        return $@"You are a professional financial advisor. Analyze this investor profile and suggest a portfolio allocation.

INVESTOR PROFILE:
- Investment Goal: {userContext}
- Net Worth: ${netWorth:F2}
- Total Assets: ${totalAssets:F2}
- Monthly Income: ${monthlyIncome:F2}

ALLOCATION TASK:
Suggest a specific portfolio allocation across these asset types:
1. Stocks (equities, growth)
2. Bonds (fixed income, stability)
3. Real Estate (property, tangible assets)
4. Crypto (digital assets, high risk/reward)
5. Cash (emergency fund, liquidity)

RESPONSE FORMAT:
Respond ONLY with allocation percentages in this exact format:
[STOCKS]% Stocks, [BONDS]% Bonds, [REALESTATE]% Real Estate, [CRYPTO]% Crypto, [CASH]% Cash

Example: 40% Stocks, 30% Bonds, 15% Real Estate, 10% Crypto, 5% Cash

Consider the investor's goals, net worth, and risk tolerance. All percentages must sum to 100%.";
    }

    /// <summary>
    /// Call Ollama AI to generate allocation strategy
    /// </summary>
    private async Task<string> GetOllamaAllocation(string prompt, CancellationToken cancellationToken)
    {
        try
        {
            var ollamaRequest = new OllamaRequest
            {
                Model = "llama3.2",
                Prompt = prompt,
                Stream = false
            };

            var response = await _httpClient.PostAsJsonAsync(
                "http://localhost:11434/api/generate",
                ollamaRequest,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return GetDefaultAllocation();
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<OllamaResponse>(content);
            
            if (jsonResponse?.Response == null)
            {
                return GetDefaultAllocation();
            }

            var allocation = ExtractAllocation(jsonResponse.Response);
            return string.IsNullOrWhiteSpace(allocation) ? GetDefaultAllocation() : allocation;
        }
        catch (HttpRequestException)
        {
            // Ollama not running
            return GetDefaultAllocation();
        }
        catch (Exception)
        {
            return GetDefaultAllocation();
        }
    }

    /// <summary>
    /// Call Ollama AI to explain the recommendation
    /// </summary>
    private async Task<string> GetOllamaExplanation(string allocation, string userContext, CancellationToken cancellationToken)
    {
        try
        {
            var prompt = $@"Based on this portfolio allocation: {allocation}

For an investor with this goal: {userContext}

Provide a 2-3 sentence explanation of why this allocation is suitable. Be professional and concise.";

            var ollamaRequest = new OllamaRequest
            {
                Model = "llama3:latest",
                Prompt = prompt,
                Stream = false
            };

            var response = await _httpClient.PostAsJsonAsync(
                "http://localhost:11434/api/generate",
                ollamaRequest,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return GetDefaultExplanation(allocation);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<OllamaResponse>(content);
            
            return jsonResponse?.Response?.Trim() ?? GetDefaultExplanation(allocation);
        }
        catch (Exception)
        {
            return GetDefaultExplanation(allocation);
        }
    }

    /// <summary>
    /// Extract allocation from AI response (handles various formats)
    /// </summary>
    private string ExtractAllocation(string response)
    {
        var lines = response.Split('\n');
        foreach (var line in lines)
        {
            // Look for percentage signs and asset types
            if (line.Contains("%") && (line.Contains("Stocks") || line.Contains("Bonds") || line.Contains("Crypto") || line.Contains("Real Estate")))
            {
                return line.Trim();
            }
        }
        return GetDefaultAllocation();
    }

    /// <summary>
    /// Default allocation (fallback)
    /// </summary>
    private string GetDefaultAllocation()
    {
        return "40% Stocks, 30% Bonds, 15% Real Estate, 10% Crypto, 5% Cash";
    }

    /// <summary>
    /// Default explanation (fallback)
    /// </summary>
    private string GetDefaultExplanation(string allocation)
    {
        return $"This allocation ({allocation}) balances growth with stability through diversification. " +
               "The mix provides exposure to equities, fixed income, real assets, and digital currencies while maintaining liquidity. " +
               "Adjust based on market conditions and your changing financial goals.";
    }

    /// <summary>
    /// Generate fallback recommendation if Ollama unavailable
    /// </summary>
    private async Task<InvestmentRecommendationDto> GenerateFallbackRecommendation(GenerateRecommendationCommand request)
    {
        var allocation = GetDefaultAllocation();
        var explanation = GetDefaultExplanation(allocation);

        var recommendation = new InvestmentRecommendation(
            request.UserId,
            allocation,
            explanation);

        await _recommendationRepository.AddAsync(recommendation);
        return _mapper.Map<InvestmentRecommendationDto>(recommendation);
    }
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
