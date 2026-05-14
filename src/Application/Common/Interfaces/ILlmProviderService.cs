namespace Prospera.Application.Common.Interfaces;

/// <summary>
/// Service for interacting with multiple LLM providers (Ollama, OpenRouter, etc.)
/// Abstracts the complexity of different API formats and providers
/// </summary>
public interface ILlmProviderService
{
    /// <summary>
    /// Get available models from the provider
    /// </summary>
    Task<List<LlmModelInfo>> GetAvailableModelsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate a response using the specified model
    /// </summary>
    Task<string> GenerateResponseAsync(
        string prompt,
        string modelName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if provider is available/healthy
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get provider name (e.g., "Ollama", "OpenRouter")
    /// </summary>
    string GetProviderName();
}

/// <summary>
/// Information about an available LLM model
/// </summary>
public class LlmModelInfo
{
    /// <summary>
    /// Model identifier (e.g., "llama3.2", "gpt-4", "claude-3-opus")
    /// </summary>
    public string ModelId { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the model
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
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// Whether this model is free to use (local or zero-cost cloud)
    /// </summary>
    public bool IsFree { get; set; }

    /// <summary>
    /// Estimated cost per 1K tokens (if applicable)
    /// </summary>
    public decimal? CostPer1kTokens { get; set; }

    /// <summary>
    /// Maximum context window size
    /// </summary>
    public int ContextWindow { get; set; }
}
