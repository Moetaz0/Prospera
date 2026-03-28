using Prospera.Domain.Entities;

namespace Prospera.Infrastructure.Identity;

public record AuthResult(
    string Token,
    Guid UserId,
    string Email,
    string FullName,
    string Role)
{
    public static AuthResult FromUser(string token, User user) =>
        new(token, user.Id, user.Email, user.FullName, user.Role);
}
