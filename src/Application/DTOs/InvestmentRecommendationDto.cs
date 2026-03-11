namespace Prospera.Application.DTOs;

public class InvestmentRecommendationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string SuggestedAllocation { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
