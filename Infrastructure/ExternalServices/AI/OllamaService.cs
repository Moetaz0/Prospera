using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Prospera.Infrastructure.ExternalServices.AI;




public class OllamaService : ILlmService
{
    private const string DefaultOllamaModel = "llama3.2";
    private const string DefaultOpenRouterModel = "openchat/openchat-3.5";
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OllamaService> _logger;
    private readonly string _ollamaUrl;
    private readonly string _modelName;

    public OllamaService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OllamaService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _ollamaUrl = configuration["Ollama:Url"] ?? "http://localhost:11434";
        _modelName = ResolveModelName(_ollamaUrl, configuration["Ollama:Model"]);
        _httpClient.BaseAddress = new Uri(_ollamaUrl);
    }

    public async Task<string> GenerateFinancialAdviceAsync(
        FinancialContext context,
        CancellationToken cancellationToken = default)
    {
        var prompt = BuildFinancialAdvicePrompt(context);

        try
        {
            // Call Ollama API (simplified - implement actual API call)
            var response = await _httpClient.PostAsJsonAsync(
                "/api/generate",
                new
                {
                    model = _modelName,
                    prompt = prompt,
                    stream = false
                },
                cancellationToken);

            response.EnsureSuccessStatusCode();

            // Parse response and return
            return "Financial advice from AI..."; // Implement actual parsing
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating financial advice");
            throw;
        }
    }

    public async Task<string> AnalyzeSpendingPatternsAsync(
        string spendingSummary,
        CancellationToken cancellationToken = default)
    {
        var prompt = $@"
You are a financial advisor. Analyze these spending patterns and provide insights:

{spendingSummary}

Please provide:
1. Key spending patterns identified
2. Areas of concern or overspending
3. 3 specific recommendations to improve financial health

Keep your response concise and actionable.
";

        try
        {
            // Call Ollama API
            return "Spending analysis..."; // Implement actual call
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing spending patterns");
            throw;
        }
    }

    private string BuildFinancialAdvicePrompt(FinancialContext context)
    {
        return $@"
You are a certified financial advisor. Based on the following user profile, provide personalized financial recommendations:

User Profile:
- Monthly Income: ${context.MonthlyIncome:N2}
- Monthly Expenses: ${context.MonthlyExpenses:N2}
- Savings Rate: {context.SavingsRate:F2}%
- Total Savings: ${context.TotalSavings:N2}
- Total Debt: ${context.TotalDebt:N2}
- Risk Profile: {context.RiskProfile}
- Age: {context.Age}
- Financial Goals: {string.Join(", ", context.Goals)}

Provide exactly 3 actionable financial recommendations tailored to this user. Each recommendation should:
1. Be specific and measurable
2. Align with their risk profile
3. Consider their current financial situation

Format your response as:
1. [Recommendation 1]
2. [Recommendation 2]
3. [Recommendation 3]
";
    }

    private string ResolveModelName(string baseUrl, string? configuredModel)
    {
        if (!IsOpenRouterUrl(baseUrl))
        {
            return string.IsNullOrWhiteSpace(configuredModel) ? DefaultOllamaModel : configuredModel;
        }

        if (string.IsNullOrWhiteSpace(configuredModel))
        {
            _logger.LogWarning(
                "No OpenRouter model configured. Falling back to '{FallbackModel}'.",
                DefaultOpenRouterModel);

            return DefaultOpenRouterModel;
        }

        if (configuredModel.Equals("llama3:latestt", StringComparison.OrdinalIgnoreCase) ||
            (configuredModel.StartsWith("llama", StringComparison.OrdinalIgnoreCase) &&
             configuredModel.Contains(':')))
        {
            _logger.LogWarning(
                "Invalid OpenRouter model '{ModelName}' configured. Falling back to '{FallbackModel}'.",
                configuredModel,
                DefaultOpenRouterModel);

            return DefaultOpenRouterModel;
        }

        return configuredModel;
    }

    private static bool IsOpenRouterUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return uri.Host.Equals("openrouter.ai", StringComparison.OrdinalIgnoreCase) ||
               uri.Host.EndsWith(".openrouter.ai", StringComparison.OrdinalIgnoreCase);
    }
}
