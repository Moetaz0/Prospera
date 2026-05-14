using Prospera.Application.Common.Interfaces;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Prospera.Infrastructure.ExternalServices.AI;

/// <summary>
/// OpenRouter LLM service provider
/// Integrates with OpenRouter to access multiple LLMs (GPT-4, Claude, Mistral, etc.)
/// </summary>
public class OpenRouterService : ILlmProviderService
{
    private readonly HttpClient _httpClient;
    private readonly IOpenRouterConfiguration _config;

    public OpenRouterService(HttpClient httpClient, IOpenRouterConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<List<LlmModelInfo>> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_config.Url}/models", cancellationToken);
            if (!response.IsSuccessStatusCode)
                return GetFallbackModels();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var models = System.Text.Json.JsonSerializer.Deserialize<OpenRouterModelsResponse>(json);

            if (models?.Data == null || models.Data.Length == 0)
                return GetFallbackModels();

            return models.Data
                .Where(m => m.Id != null)
                .Select(m => new LlmModelInfo
                {
                    ModelId = m.Id,
                    DisplayName = FormatModelName(m.Id),
                    Provider = "OpenRouter",
                    Description = m.Description ?? "No description available",
                    IsAvailable = true,
                    ContextWindow = m.ContextLength ?? 4096,
                    CostPer1kTokens = m.PricingInput
                })
                .ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching OpenRouter models: {ex.Message}");
            return GetFallbackModels();
        }
    }

    public async Task<string> GenerateResponseAsync(
        string prompt,
        string modelName,
        CancellationToken cancellationToken = default)
    {
        var request = new OpenRouterRequest
        {
            Model = modelName,
            Messages = new[]
            {
                new OpenRouterMessage
                {
                    Role = "user",
                    Content = prompt
                }
            },
            Temperature = 0.7m,
            MaxTokens = 2000
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_config.Url}/chat/completions")
        {
            Content = JsonContent.Create(request)
        };

        // Log the request details for debugging
        var apiKeyPrefix = string.IsNullOrEmpty(_config.ApiKey) ? "EMPTY" : _config.ApiKey.Substring(0, Math.Min(10, _config.ApiKey.Length)) + "...";
        System.Diagnostics.Debug.WriteLine($"OpenRouter Request - URL: {_config.Url}/chat/completions, Model: {modelName}, ApiKey: {apiKeyPrefix}");

        httpRequest.Headers.Add("Authorization", $"Bearer {_config.ApiKey}");
        httpRequest.Headers.Add("HTTP-Referer", "https://prospera-app.com");
        httpRequest.Headers.Add("X-Title", $"Prospera/{_config.AppVersion}");

        try
        {
            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                System.Diagnostics.Debug.WriteLine($"OpenRouter error: {response.StatusCode} - {errorContent}");
                throw new HttpRequestException($"OpenRouter error: {response.StatusCode} - {errorContent}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = System.Text.Json.JsonSerializer.Deserialize<OpenRouterResponse>(content);

            return result?.Choices?.FirstOrDefault()?.Message?.Content ?? "No response generated";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OpenRouter error: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_config.Url}/models", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public string GetProviderName() => "OpenRouter";

    /// <summary>
    /// Format model ID to display name (e.g., "openai/gpt-4" -> "GPT-4")
    /// </summary>
    private string FormatModelName(string modelId)
    {
        if (string.IsNullOrEmpty(modelId))
            return modelId;

        var parts = modelId.Split('/');
        var name = parts.Length > 1 ? parts[1] : parts[0];
        
        // Convert kebab-case to Title Case
        return System.Text.RegularExpressions.Regex.Replace(
            name,
            @"(?:^|-)(.)",
            m => m.Groups[1].Value.ToUpper()
        );
    }

    /// <summary>
    /// Get fallback models when API is unavailable
    /// Prioritizes FREE models from OpenRouter that don't require credits
    /// </summary>
    private List<LlmModelInfo> GetFallbackModels()
    {
        return new List<LlmModelInfo>
        {
            // ✅ FREE Models (Currently Available on OpenRouter)
            new()
            {
                ModelId = "openrouter/owl-alpha",
                DisplayName = "Owl Alpha (FREE)",
                Provider = "OpenRouter",
                Description = "Free model available on OpenRouter",
                ContextWindow = 8192,
                CostPer1kTokens = 0m
            },
            new()
            {
                ModelId = "inclusionai/ring-2.6-1t:free",
                DisplayName = "Ring-2.6-1T (FREE - Thinking)",
                Provider = "OpenRouter",
                Description = "Advanced thinking model - free tier",
                ContextWindow = 262144,
                CostPer1kTokens = 0m
            },
            new()
            {
                ModelId = "baidu/cobuddy:free",
                DisplayName = "CoBuddy (FREE)",
                Provider = "OpenRouter",
                Description = "Baidu's free model",
                ContextWindow = 8192,
                CostPer1kTokens = 0m
            },
            // 💰 PAID Models (Low Cost)
            new()
            {
                ModelId = "google/gemini-3.1-flash-lite",
                DisplayName = "Gemini 3.1 Flash Lite",
                Provider = "OpenRouter",
                Description = "Google's fast and efficient model",
                ContextWindow = 1048576,
                CostPer1kTokens = 0.00000025m
            },
            new()
            {
                ModelId = "anthropic/claude-haiku-latest",
                DisplayName = "Claude Haiku (Latest)",
                Provider = "OpenRouter",
                Description = "Fast and affordable Claude model",
                ContextWindow = 200000,
                CostPer1kTokens = 0.00008m
            },
            new()
            {
                ModelId = "openai/gpt-4o-mini-latest",
                DisplayName = "GPT-4o Mini (Latest)",
                Provider = "OpenRouter",
                Description = "Latest OpenAI mini model",
                ContextWindow = 128000,
                CostPer1kTokens = 0.00015m
            },
            new()
            {
                ModelId = "mistralai/mistral-medium-3-5",
                DisplayName = "Mistral Medium 3.5",
                Provider = "OpenRouter",
                Description = "Mistral's capable model",
                ContextWindow = 32000,
                CostPer1kTokens = 0.00027m
            },
            new()
            {
                ModelId = "openai/gpt-chat-latest",
                DisplayName = "GPT Chat Latest",
                Provider = "OpenRouter",
                Description = "Latest OpenAI GPT model",
                ContextWindow = 128000,
                CostPer1kTokens = 0.00050m
            }
        };
    }
}

/// <summary>
/// OpenRouter API request model
/// </summary>
public class OpenRouterRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public OpenRouterMessage[] Messages { get; set; } = Array.Empty<OpenRouterMessage>();

    [JsonPropertyName("temperature")]
    public decimal Temperature { get; set; } = 0.7m;

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; } = 2000;
}

/// <summary>
/// OpenRouter message model
/// </summary>
public class OpenRouterMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// OpenRouter API response model
/// </summary>
public class OpenRouterResponse
{
    [JsonPropertyName("choices")]
    public OpenRouterChoice[]? Choices { get; set; }

    [JsonPropertyName("usage")]
    public OpenRouterUsage? Usage { get; set; }
}

/// <summary>
/// OpenRouter choice model
/// </summary>
public class OpenRouterChoice
{
    [JsonPropertyName("message")]
    public OpenRouterMessage? Message { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}

/// <summary>
/// OpenRouter usage model
/// </summary>
public class OpenRouterUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

/// <summary>
/// OpenRouter models list response
/// </summary>
public class OpenRouterModelsResponse
{
    [JsonPropertyName("data")]
    public OpenRouterModelData[]? Data { get; set; }
}

/// <summary>
/// OpenRouter model data
/// </summary>
public class OpenRouterModelData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("context_length")]
    public int? ContextLength { get; set; }

    [JsonPropertyName("pricing")]
    public OpenRouterPricing? Pricing { get; set; }

    [JsonPropertyName("pricing_input")]
    public decimal? PricingInput => Pricing?.Input;
}

/// <summary>
/// OpenRouter pricing model
/// </summary>
public class OpenRouterPricing
{
    [JsonPropertyName("prompt")]
    public decimal? Input { get; set; }

    [JsonPropertyName("completion")]
    public decimal? Output { get; set; }
}
