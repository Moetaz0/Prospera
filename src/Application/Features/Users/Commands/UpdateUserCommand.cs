using MediatR;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Users.Commands;

public class UpdateUserCommand : IRequest<UserDto?>
{
    public Guid UserId { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? RiskProfile { get; set; }
}
