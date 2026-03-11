using Prospera.Domain.Entities;

namespace Prospera.Domain.Events;

public class InvestmentRecommendationGeneratedEvent : DomainEvent
{
    public Guid RecommendationId { get; }
    public Guid UserId { get; }

    public InvestmentRecommendationGeneratedEvent(InvestmentRecommendation recommendation)
    {
        RecommendationId = recommendation.Id;
        UserId = recommendation.UserId;
    }
}
