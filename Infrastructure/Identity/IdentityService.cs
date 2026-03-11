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

    public async Task<Result<string>> RegisterAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        int age)
    {
        try
        {
            var user = new User(firstName + " " + lastName, email);
            await _userRepository.AddAsync(user);

            var token = _tokenGenerator.GenerateToken(user);
            return Result<string>.Success(token);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }
    }

    public async Task<Result<string>> LoginAsync(string email, string password)
    {
        try
        {
            return Result<string>.Failure("Login not implemented");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }
    }
}