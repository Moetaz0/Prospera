namespace Prospera.Infrastructure.Identity;

public interface IIdentityService
{
    Task<Result<AuthResult>> RegisterAsync(string email, string password, string firstName, string lastName, int age);
    Task<Result<AuthResult>> LoginAsync(string email, string password);
    Task<Result<AuthResult>> RefreshTokenAsync(string refreshToken);
    Task<Result<string>> ForgotPasswordAsync(string email);
    Task<Result<string>> ResetPasswordAsync(string email, string token, string newPassword);
    Task<Result<AuthResult>> GetCurrentUserAsync(Guid userId);
}
