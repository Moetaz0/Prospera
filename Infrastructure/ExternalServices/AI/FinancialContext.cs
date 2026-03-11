namespace Prospera.Infrastructure.ExternalServices.AI;

public class FinancialContext
{
    public decimal MonthlyIncome { get; set; }
    public decimal MonthlyExpenses { get; set; }
    public decimal SavingsRate { get; set; }
    public decimal TotalSavings { get; set; }
    public decimal TotalDebt { get; set; }
    public string RiskProfile { get; set; } = string.Empty;
    public int Age { get; set; }
    public List<string> Goals { get; set; } = new();
}
