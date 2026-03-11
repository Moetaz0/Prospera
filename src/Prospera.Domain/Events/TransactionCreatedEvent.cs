

using Prospera.Domain.Entities;

namespace Prospera.Domain.Events
{
    internal class TransactionCreatedEvent : DomainEvent
    {
        public Transaction Transaction { get; }

        public TransactionCreatedEvent(Transaction transaction)
        {
            Transaction = transaction;
        }

    }
}
