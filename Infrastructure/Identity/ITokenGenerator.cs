using Prospera.Domain.Entities;

namespace Prospera.Infrastructure.Identity;

public interface ITokenGenerator
{
    string GenerateToken(User user);
}
