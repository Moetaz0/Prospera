namespace Prospera.Contracts.DTOs.Recommendations;

public class InvestmentRecommendationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? SuggestedAllocation { get; set; }
    public string? Explanation { get; set; }
    public DateTime CreatedAt { get; set; }
}
