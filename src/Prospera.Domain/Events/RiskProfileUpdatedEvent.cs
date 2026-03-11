

using Prospera.Domain.Enums;

namespace Prospera.Domain.Events
{
    internal class RiskProfileUpdatedEvent : DomainEvent

    {
        public Guid UserId { get; }
        public RiskProfile NewProfile { get; }

        public RiskProfileUpdatedEvent(Guid userId, RiskProfile newProfile)
        {
            UserId = userId;
            NewProfile = newProfile;
        }
    }
}
