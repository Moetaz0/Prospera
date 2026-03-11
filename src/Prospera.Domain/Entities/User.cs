using Prospera.Domain.Common;
using Prospera.Domain.Enums;
using Prospera.Domain.Events;

namespace Prospera.Domain.Entities;

public class User : BaseEntity , IAggregateRoot
{
    public string FullName { get; private set; }
    public string Email { get; private set; }

    public RiskProfile RiskProfile { get; private set; }

    private readonly List<Asset> _assets = new();
    public IReadOnlyCollection<Asset> Assets => _assets.AsReadOnly();

    private readonly List<Liability> _liabilities = new();
    public IReadOnlyCollection<Liability> Liabilities => _liabilities.AsReadOnly();

    private readonly List<Transaction> _transactions = new();
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    public User(string fullName, string email)
    {
        FullName = fullName;
        Email = email;
        RiskProfile = RiskProfile.Moderate;
    }

    public void AddAsset(Asset asset)
    {
        _assets.Add(asset);
        AddDomainEvent(new AssetAddedEvent(asset));
    }

    public decimal CalculateNetWorth()
    {
        var totalAssets = _assets.Sum(a => a.CurrentValue);
        var totalLiabilities = _liabilities.Sum(l => l.Amount);
        return totalAssets - totalLiabilities;
    }
}
