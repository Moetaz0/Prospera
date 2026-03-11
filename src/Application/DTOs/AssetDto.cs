namespace Prospera.Application.DTOs;

public class AssetDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal CurrentValue { get; set; }
    public string Type { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}
