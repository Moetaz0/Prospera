using Prospera.Domain.Common;
using Prospera.Domain.Enums;

namespace Prospera.Domain.Entities;

public class Liability : BaseEntity
{
    public string Name { get; private set; }
    public decimal Amount { get; private set; }
    public LiabilityType Type { get; private set; }

    public Guid UserId { get; private set; }

    public Liability(string name, decimal amount, LiabilityType type, Guid userId)
    {
        Name = name;
        Amount = amount;
        Type = type;
        UserId = userId;
    }

    public void UpdateDetails(string name, decimal amount, LiabilityType type)
    {
        Name = name;
        Amount = amount;
        Type = type;
    }
}
