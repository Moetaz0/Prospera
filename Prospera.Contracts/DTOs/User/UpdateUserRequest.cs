using Prospera.Contracts.Enums;

namespace Prospera.Contracts.DTOs.User;

public class UpdateUserRequest
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public RiskProfile? RiskProfile { get; set; }
}
