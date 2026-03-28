using Prospera.Domain.Entities;
using Prospera.Domain.Interfaces;

namespace Prospera.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;

    public IdentityService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenGenerator tokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

public async Task<Result<AuthResult>> RegisterAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        int age)
    {
        try
        {
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser is not null)
            {
                return Result<AuthResult>.Failure("Email is already registered");
            }

            var passwordHash = _passwordHasher.HashPassword(password);
            var user = new User(firstName + " " + lastName, email, passwordHash);
            await _userRepository.AddAsync(user);

            var token = _tokenGenerator.GenerateToken(user);
            return Result<AuthResult>.Success(AuthResult.FromUser(token, user));
        }
        catch (Exception ex)
        {
            return Result<AuthResult>.Failure(ex.Message);
        }
    }

    public async Task<Result<AuthResult>> LoginAsync(string email, string password)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user is null)
            {
                return Result<AuthResult>.Failure("Invalid credentials");
            }

            if (!_passwordHasher.VerifyPassword(user.PasswordHash, password))
            {
                return Result<AuthResult>.Failure("Invalid credentials");
            }

            var token = _tokenGenerator.GenerateToken(user);
            return Result<AuthResult>.Success(AuthResult.FromUser(token, user));
        }
        catch (Exception ex)
        {
            return Result<AuthResult>.Failure(ex.Message);
        }
    }
}