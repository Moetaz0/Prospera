using Prospera.Contracts.Enums;

namespace Prospera.Contracts.DTOs.Liability;

public class AddLiabilityRequest
{
    public string? Name { get; set; }
    public decimal Amount { get; set; }
    public LiabilityType Type { get; set; }
}
