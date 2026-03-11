using Prospera.Contracts.Enums;

namespace Prospera.Contracts.DTOs.Liability;

public class LiabilityDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public decimal Amount { get; set; }
    public LiabilityType Type { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
