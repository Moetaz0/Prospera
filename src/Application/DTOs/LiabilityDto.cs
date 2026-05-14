namespace Prospera.Application.DTOs;

public class LiabilityDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public Guid UserId { get; set; }

    /// <summary>
    /// Optional: Interest rate if applicable (e.g., loan or credit card)
    /// </summary>
    public decimal? InterestRate { get; set; }

    /// <summary>
    /// Optional: Monthly payment amount
    /// </summary>
    public decimal? MonthlyPayment { get; set; }

    /// <summary>
    /// Optional: Projected value/amount after 1 year
    /// </summary>
    public decimal? ProjectedAmount { get; set; }

    /// <summary>
    /// Comprehensive coaching advice for this liability
    /// Includes debt reduction strategies, motivation, and action plans
    /// </summary>
    public CoachingAdviceDto? CoachingAdvice { get; set; }
}
