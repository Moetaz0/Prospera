using MediatR;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Users.Commands;

/// <summary>
/// Command to create a new user
/// </summary>
public class CreateUserCommand : IRequest<UserDto>
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
}
