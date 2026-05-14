using Prospera.Contracts.Enums;

namespace Prospera.Contracts.DTOs.User;

public class UpdateUserRequest
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public RiskProfile? RiskProfile { get; set; }
    public bool? IsAdmin { get; set; }
}
