using Prospera.Domain.Common;
using Prospera.Domain.Events;

namespace Prospera.Application.Common.Interfaces;

public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches domain events to registered handlers
    /// </summary>
    Task DispatchAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
