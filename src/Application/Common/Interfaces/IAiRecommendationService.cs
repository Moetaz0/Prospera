namespace Prospera.Application.Common.Interfaces;

public interface IAiRecommendationService
{
    /// <summary>
    /// Generates investment recommendations using AI/LLM based on user's financial profile
    /// </summary>
    Task<string> GenerateRecommendationAsync(string prompt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes financial patterns and provides insights
    /// </summary>
    Task<string> AnalyzePatternAsync(string financialSummary, CancellationToken cancellationToken = default);
}
