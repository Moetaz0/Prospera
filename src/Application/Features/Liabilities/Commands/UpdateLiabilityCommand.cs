using MediatR;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Liabilities.Commands;

public class UpdateLiabilityCommand : IRequest<LiabilityDto?>
{
    public Guid UserId { get; set; }
    public Guid LiabilityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
}
