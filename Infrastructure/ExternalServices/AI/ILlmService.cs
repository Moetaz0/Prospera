namespace Prospera.Infrastructure.ExternalServices.AI;

public interface ILlmService
{
    Task<string> GenerateFinancialAdviceAsync(FinancialContext context, CancellationToken cancellationToken = default);
    Task<string> AnalyzeSpendingPatternsAsync(string spendingSummary, CancellationToken cancellationToken = default);
}
