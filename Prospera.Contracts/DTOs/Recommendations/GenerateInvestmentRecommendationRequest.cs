using Prospera.Domain.Common;

namespace Prospera.Contracts.DTOs.Recommendations;

/// <summary>
/// Request DTO for generating investment recommendations
/// </summary>
public class GenerateInvestmentRecommendationRequest
{
    /// <summary>
    /// The user's investment analysis context or goal
    /// </summary>
    public string? AnalysisContext { get; set; }

    /// <summary>
    /// Optional: The AI model to use for recommendation generation
    /// If not specified, the default model from the selected provider will be used.
    /// Examples for Ollama: "llama3.2", "mistral", "neural-chat"
    /// Examples for OpenRouter: "openai/gpt-4", "anthropic/claude-3-opus", "mistralai/mistral-7b"
    /// </summary>
    public string? ModelName { get; set; }

    /// <summary>
    /// Optional: Which LLM provider to use (Ollama or OpenRouter)
    /// Defaults to Ollama if not specified
    /// </summary>
    public LlmProvider Provider { get; set; } = LlmProvider.Ollama;

    /// <summary>
    /// Optional: Custom API endpoint (if using custom Ollama or OpenRouter instance)
    /// </summary>
    public string? CustomEndpoint { get; set; }
}
