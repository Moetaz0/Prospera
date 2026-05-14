namespace Prospera.Application.Common.Interfaces;

/// <summary>
/// Configuration for Ollama AI service
/// </summary>
public interface IOllamaConfiguration
{
    string Url { get; }
    string Model { get; }
}

/// <summary>
/// Default implementation of Ollama configuration
/// </summary>
public class OllamaConfiguration : IOllamaConfiguration
{
    public string Url { get; }
    public string Model { get; }

    public OllamaConfiguration(string url = "http://localhost:11434", string model = "llama3.2")
    {
        Url = url;
        Model = model;
    }
}
