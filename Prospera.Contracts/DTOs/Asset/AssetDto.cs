using Prospera.Contracts.Enums;

namespace Prospera.Contracts.DTOs.Asset;

public class AssetDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public decimal CurrentValue { get; set; }
    public AssetType Type { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
