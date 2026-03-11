
using Prospera.Domain.Entities;

namespace Prospera.Domain.Events
{
    internal class LiabilityAddedEvent : DomainEvent
    {
        public Liability Liability { get; }
        public LiabilityAddedEvent(Liability liability)
        {
            Liability = liability;
        }
    }
}
