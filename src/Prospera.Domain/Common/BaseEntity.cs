using Prospera.Domain.Events;

namespace Prospera.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    private readonly List<DomainEvent> _domainEvents = new();
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(DomainEvent eventItem)
        => _domainEvents.Add(eventItem);

    public void ClearDomainEvents()
        => _domainEvents.Clear();
}
