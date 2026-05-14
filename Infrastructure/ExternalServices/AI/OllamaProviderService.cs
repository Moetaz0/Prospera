using Prospera.Application.Common.Interfaces;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Prospera.Infrastructure.ExternalServices.AI;

/// <summary>
/// Ollama LLM service provider
/// Integrates with local Ollama instance to access various open-source LLMs
/// </summary>
public class OllamaProviderService : ILlmProviderService
{
    private readonly HttpClient _httpClient;
    private readonly IOllamaConfiguration _config;

    public OllamaProviderService(HttpClient httpClient, IOllamaConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<List<LlmModelInfo>> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_config.Url}/api/tags", cancellationToken);
            if (!response.IsSuccessStatusCode)
                return GetDefaultModels();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var modelsResponse = System.Text.Json.JsonSerializer.Deserialize<OllamaTagsResponse>(json);

            if (modelsResponse?.Models == null || modelsResponse.Models.Length == 0)
                return GetDefaultModels();

            return modelsResponse.Models
                .Select(m => new LlmModelInfo
                {
                    ModelId = m.Name,
                    DisplayName = m.Name,
                    Provider = "Ollama (Local)",
                    Description = $"Model: {m.Name} | Size: {FormatBytes(m.Size)}",
                    IsAvailable = true,
                    ContextWindow = 4096,
                    CostPer1kTokens = 0 // Local, no cost
                })
                .ToList();
        }
        catch
        {
            return GetDefaultModels();
        }
    }

    public async Task<string> GenerateResponseAsync(
        string prompt,
        string modelName,
        CancellationToken cancellationToken = default)
    {
        var request = new OllamaGenerateRequest
        {
            Model = modelName,
            Prompt = prompt,
            Stream = false
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_config.Url}/api/generate",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                System.Diagnostics.Debug.WriteLine($"Ollama error: {response.StatusCode} - {errorContent}");
                throw new HttpRequestException($"Ollama error: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = System.Text.Json.JsonSerializer.Deserialize<OllamaGenerateResponse>(content);

            return result?.Response ?? "No response generated";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ollama error: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_config.Url}/api/tags", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public string GetProviderName() => "Ollama";

    private List<LlmModelInfo> GetDefaultModels()
    {
        return new List<LlmModelInfo>
        {
            new()
            {
                ModelId = "llama3:latest",
                DisplayName = "llama3:latest",
                Provider = "Ollama (Meta)",
                Description = "Latest Llama 3 model (local)",
                ContextWindow = 8192,
                CostPer1kTokens = 0
            },
            new()
            {
                ModelId = "mistral:latest",
                DisplayName = "mistral:latest",
                Provider = "Ollama (Mistral AI)",
                Description = "Efficient Mistral model (local)",
                ContextWindow = 32000,
                CostPer1kTokens = 0
            },
            new()
            {
                ModelId = "neural-chat:latest",
                DisplayName = "neural-chat:latest",
                Provider = "Ollama (Intel)",
                Description = "Chat-optimized model (local)",
                ContextWindow = 4096,
                CostPer1kTokens = 0
            }
        };
    }

    private string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

/// <summary>
/// Ollama tags (models) response
/// </summary>
public class OllamaTagsResponse
{
    [JsonPropertyName("models")]
    public OllamaModelInfo[]? Models { get; set; }
}

/// <summary>
/// Ollama model information
/// </summary>
public class OllamaModelInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("digest")]
    public string? Digest { get; set; }
}

/// <summary>
/// Ollama generate request
/// </summary>
public class OllamaGenerateRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }

    [JsonPropertyName("temperature")]
    public decimal Temperature { get; set; } = 0.7m;
}

/// <summary>
/// Ollama generate response
/// </summary>
public class OllamaGenerateResponse
{
    [JsonPropertyName("response")]
    public string Response { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("done")]
    public bool Done { get; set; }
}
  

