using Prospera.Domain.Common;

using Prospera.Domain.Enums;

namespace Prospera.Domain.Entities;

public class Asset : BaseEntity
{
    public string Name { get; private set; }
    public decimal CurrentValue { get; private set; }
    public AssetType Type { get; private set; }

    public Guid UserId { get; private set; }

    public Asset(string name, decimal currentValue, AssetType type, Guid userId)
    {
        Name = name;
        CurrentValue = currentValue;
        Type = type;
        UserId = userId;
    }

    public void UpdateDetails(string name, decimal currentValue, AssetType type)
    {
        Name = name;
        CurrentValue = currentValue;
        Type = type;
    }
}
