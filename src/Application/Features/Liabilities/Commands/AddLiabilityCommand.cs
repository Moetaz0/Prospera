using MediatR;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Liabilities.Commands;

/// <summary>
/// Command to add a new liability (debt) to a user's profile
/// </summary>
public class AddLiabilityCommand : IRequest<LiabilityDto>
{
    public required Guid UserId { get; set; }
    public required string Name { get; set; }
    public required decimal Amount { get; set; }
    public required string Type { get; set; } // LiabilityType as string
}
