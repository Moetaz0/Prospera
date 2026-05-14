using MediatR;
using Prospera.Application.DTOs;
using Prospera.Domain.Common;

namespace Prospera.Application.Features.Recommendations.Commands;

/// <summary>
/// Command to generate AI-powered investment recommendations for a user
/// Supports multiple LLM providers and models
/// </summary>
public class GenerateRecommendationCommand : IRequest<InvestmentRecommendationDto>
{
    public required Guid UserId { get; set; }
    public required string AnalysisContext { get; set; }

    /// <summary>
    /// Optional: The AI model to use for generation. If not provided, uses default from configuration.
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
