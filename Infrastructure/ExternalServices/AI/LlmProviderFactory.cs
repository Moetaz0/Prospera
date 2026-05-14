using Prospera.Domain.Common;
using Prospera.Application.Common.Interfaces;

namespace Prospera.Infrastructure.ExternalServices.AI;

/// <summary>
/// Implementation of LLM provider factory
/// </summary>
public class LlmProviderFactory : ILlmProviderFactory
{
    private readonly OllamaProviderService _ollamaProvider;
    private readonly OpenRouterService _openRouterProvider;

    public LlmProviderFactory(
        OllamaProviderService ollamaProvider,
        OpenRouterService openRouterProvider)
    {
        _ollamaProvider = ollamaProvider;
        _openRouterProvider = openRouterProvider;
    }

    public async Task<ILlmProviderService> GetProviderAsync(LlmProvider provider)
    {
        return provider switch
        {
            LlmProvider.Ollama => _ollamaProvider,
            LlmProvider.OpenRouter => _openRouterProvider,
            _ => _ollamaProvider
        };
    }

    public ILlmProviderService GetDefaultProvider()
    {
        return _ollamaProvider;
    }

    public async Task<Dictionary<string, List<LlmModelInfo>>> GetAllProvidersWithModelsAsync(CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, List<LlmModelInfo>>();

        try
        {
            var ollamaModels = await _ollamaProvider.GetAvailableModelsAsync(cancellationToken);
            result["Ollama"] = ollamaModels;
        }
        catch
        {
            result["Ollama"] = new List<LlmModelInfo>();
        }

        try
        {
            var openRouterModels = await _openRouterProvider.GetAvailableModelsAsync(cancellationToken);
            result["OpenRouter"] = openRouterModels;
        }
        catch
        {
            result["OpenRouter"] = new List<LlmModelInfo>();
        }

        return result;
    }
}
