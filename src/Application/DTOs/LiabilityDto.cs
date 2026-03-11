namespace Prospera.Application.DTOs;

public class LiabilityDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}
