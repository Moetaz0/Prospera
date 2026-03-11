using Prospera.Contracts.Enums;

namespace Prospera.Contracts.DTOs.Asset;

public class AddAssetRequest
{
    public string? Name { get; set; }
    public decimal CurrentValue { get; set; }
    public AssetType Type { get; set; }
}
