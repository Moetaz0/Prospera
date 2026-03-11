using Prospera.Contracts.DTOs.Asset;
using Prospera.Contracts.DTOs.Liability;
using Prospera.Contracts.DTOs.FinancialMetrics;
using Prospera.Contracts.Enums;

namespace Prospera.Contracts.DTOs.User;

public class UserDashboardDto
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public RiskProfile RiskProfile { get; set; }
    public decimal NetWorth { get; set; }
    public List<AssetDto> Assets { get; set; } = new();
    public List<LiabilityDto> Liabilities { get; set; } = new();
    public FinancialMetricsDto? FinancialMetrics { get; set; }
    public DateTime CreatedAt { get; set; }
}
