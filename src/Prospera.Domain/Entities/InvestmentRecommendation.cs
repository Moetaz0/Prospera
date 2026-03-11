using Prospera.Domain.Common;

namespace Prospera.Domain.Entities;

public class InvestmentRecommendation : BaseEntity
{
    public Guid UserId { get; private set; }
    public string SuggestedAllocation { get; private set; }
    public string Explanation { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public InvestmentRecommendation(Guid userId, string suggestedAllocation, string explanation)
    {
        UserId = userId;
        SuggestedAllocation = suggestedAllocation;
        Explanation = explanation;
        CreatedAt = DateTime.UtcNow;
    }
}
