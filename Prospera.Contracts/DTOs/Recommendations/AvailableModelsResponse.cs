using Prospera.Contracts.DTOs.Recommendations;

/// <summary>
/// Response DTO containing available LLM models and providers
/// </summary>
public class AvailableModelsResponse
{
    /// <summary>
    /// Dictionary of providers and their available models
    /// Key: Provider name (e.g., "Ollama", "OpenRouter")
    /// Value: List of available models for that provider
    /// </summary>
    public Dictionary<string, List<ModelInfo>> Providers { get; set; } = new();

    /// <summary>
    /// Currently configured default provider
    /// </summary>
    public string DefaultProvider { get; set; } = "Ollama";

    /// <summary>
    /// Recommended models for beginners
    /// </summary>
    public List<RecommendedModel> RecommendedModels { get; set; } = new();

    /// <summary>
    /// All free models across all providers (Ollama + free OpenRouter models)
    /// </summary>
    public List<ModelInfo> FreeModels { get; set; } = new();

    /// <summary>
    /// Timestamp of when this data was retrieved
    /// </summary>
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Information about a single LLM model
/// </summary>
public class ModelInfo
{
    /// <summary>
    /// Unique model identifier
    /// </summary>
    public string ModelId { get; set; } = string.Empty;

    /// <summary>
    /// User-friendly model name
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Provider this model is from
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Model description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether model is currently available
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// Whether this model is free to use (local or zero-cost cloud)
    /// </summary>
    public bool IsFree { get; set; }

    /// <summary>
    /// Estimated cost per 1K tokens (null if free/local)
    /// </summary>
    public decimal? CostPer1kTokens { get; set; }

    /// <summary>
    /// Maximum context window size
    /// </summary>
    public int ContextWindow { get; set; }
}

/// <summary>
/// Recommended model for specific use case
/// </summary>
public class RecommendedModel
{
    /// <summary>
    /// Use case or recommendation category
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Model ID to use
    /// </summary>
    public string ModelId { get; set; } = string.Empty;

    /// <summary>
    /// Display name
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Provider name
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Why this model is recommended
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
