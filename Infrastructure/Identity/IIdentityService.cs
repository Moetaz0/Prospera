namespace Prospera.Infrastructure.Identity;

public interface IIdentityService
{
    Task<Result<string>> RegisterAsync(string email, string password, string firstName, string lastName, int age);
    Task<Result<string>> LoginAsync(string email, string password);
}
