

using Prospera.Domain.Entities;

namespace Prospera.Domain.Events
{
    internal class AssetAddedEvent : DomainEvent
    {
        public Asset Asset { get; }

        public AssetAddedEvent(Asset asset)
        {
            Asset = asset;
        }
    }
}
