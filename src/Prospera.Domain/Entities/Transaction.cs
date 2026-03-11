using Prospera.Domain.Common;
using Prospera.Domain.Enums;

namespace Prospera.Domain.Entities;

public class Transaction : BaseEntity
{
    public decimal Amount { get; private set; }
    public DateTime Date { get; private set; }
    public TransactionType Type { get; private set; }
    public string Description { get; private set; }

    public Guid UserId { get; private set; }

    public Transaction(decimal amount, TransactionType type, string description, Guid userId)
    {
        Amount = amount;
        Type = type;
        Description = description;
        Date = DateTime.UtcNow;
        UserId = userId;
    }
}
