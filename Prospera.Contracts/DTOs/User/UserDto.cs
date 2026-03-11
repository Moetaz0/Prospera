using Prospera.Contracts.Enums;

namespace Prospera.Contracts.DTOs.User;

public class UserDto
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public RiskProfile RiskProfile { get; set; }
    public decimal NetWorth { get; set; }
    public DateTime CreatedAt { get; set; }
}
