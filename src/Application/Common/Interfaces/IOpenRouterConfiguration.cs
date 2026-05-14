namespace Prospera.Application.Common.Interfaces;

/// <summary>
/// Configuration for OpenRouter LLM service
/// </summary>
public interface IOpenRouterConfiguration
{
    /// <summary>
    /// OpenRouter API endpoint URL
    /// </summary>
    string Url { get; }

    /// <summary>
    /// OpenRouter API key for authentication
    /// </summary>
    string ApiKey { get; }

    /// <summary>
    /// Application name for OpenRouter headers
    /// </summary>
    string AppName { get; }

    /// <summary>
    /// Application version for OpenRouter headers
    /// </summary>
    string AppVersion { get; }
}

/// <summary>
/// Implementation of OpenRouter configuration
/// </summary>
public class OpenRouterConfiguration : IOpenRouterConfiguration
{
    public string Url { get; }
    public string ApiKey { get; }
    public string AppName { get; }
    public string AppVersion { get; }

    public OpenRouterConfiguration(string url, string apiKey, string appName = "Prospera", string appVersion = "1.0.0")
    {
        Url = url ?? "https://openrouter.ai/api/v1";
        ApiKey = apiKey;
        AppName = appName;
        AppVersion = appVersion;
    }
}
