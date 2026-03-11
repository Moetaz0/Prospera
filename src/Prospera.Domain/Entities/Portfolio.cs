using Prospera.Domain.Common;

namespace Prospera.Domain.Entities;

public class Portfolio : BaseEntity
{
    public Guid UserId { get; private set; }

    private readonly List<Asset> _assets = new();
    public IReadOnlyCollection<Asset> Assets => _assets.AsReadOnly();

    private readonly List<Liability> _liabilities = new();
    public IReadOnlyCollection<Liability> Liabilities => _liabilities.AsReadOnly();

    public Portfolio(Guid userId)
    {
        UserId = userId;
    }

    public decimal GetTotalAssetsValue()
        => _assets.Sum(a => a.CurrentValue);

    public decimal GetTotalLiabilitiesValue()
        => _liabilities.Sum(l => l.Amount);

    public decimal GetNetWorth()
        => GetTotalAssetsValue() - GetTotalLiabilitiesValue();
}
