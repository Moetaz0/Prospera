namespace Prospera.Infrastructure.Identity;

public interface IIdentityService
{
    Task<Result<AuthResult>> RegisterAsync(string email, string password, string firstName, string lastName, int age);
    Task<Result<AuthResult>> LoginAsync(string email, string password);
}
