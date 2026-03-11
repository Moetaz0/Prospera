namespace Prospera.Application.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RiskProfile { get; set; } = string.Empty;
    public decimal NetWorth { get; set; }
    public ICollection<AssetDto> Assets { get; set; } = new List<AssetDto>();
    public ICollection<LiabilityDto> Liabilities { get; set; } = new List<LiabilityDto>();
    public ICollection<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
}
