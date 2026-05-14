namespace Prospera.Application.Common.Interfaces;

/// <summary>
/// Factory for creating LLM provider services
/// Handles routing between Ollama, OpenRouter, and other providers
/// </summary>
public interface ILlmProviderFactory
{
    /// <summary>
    /// Get the appropriate provider service based on the requested provider type
    /// </summary>
    Task<ILlmProviderService> GetProviderAsync(Prospera.Domain.Common.LlmProvider provider);

    /// <summary>
    /// Get the default provider service
    /// </summary>
    ILlmProviderService GetDefaultProvider();

    /// <summary>
    /// List all available providers and their models
    /// </summary>
    Task<Dictionary<string, List<LlmModelInfo>>> GetAllProvidersWithModelsAsync(CancellationToken cancellationToken = default);
}
