using Prospera.Domain.Entities;

namespace Prospera.Infrastructure.Identity;

public record AuthResult(
    string Token,
    string RefreshToken,
    Guid UserId,
    string Email,
    string FullName,
    string Role)
{
    public static AuthResult FromUser(string token, string refreshToken, User user) =>
        new(token, refreshToken, user.Id, user.Email, user.FullName, user.Role);
}
