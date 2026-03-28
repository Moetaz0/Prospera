using MediatR;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Users.Queries;

public class GetUserByIdQuery : IRequest<UserDto?>
{
    public Guid UserId { get; set; }
}
