using Prospera.Domain.Entities;
using Prospera.Domain.Interfaces;
using Prospera.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace Prospera.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public IdentityService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenGenerator tokenGenerator,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _emailService = emailService;
        _configuration = configuration;
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

            var refreshToken = _tokenGenerator.GenerateRefreshToken();
            user.SetRefreshToken(refreshToken);

            await _userRepository.AddAsync(user);

            // Send welcome email
            await _emailService.SendWelcomeEmailAsync(email, firstName);

            var token = _tokenGenerator.GenerateToken(user);
            return Result<AuthResult>.Success(AuthResult.FromUser(token, refreshToken, user));
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

            var refreshToken = _tokenGenerator.GenerateRefreshToken();
            user.SetRefreshToken(refreshToken);
            await _userRepository.UpdateAsync(user);

            var token = _tokenGenerator.GenerateToken(user);
            return Result<AuthResult>.Success(AuthResult.FromUser(token, refreshToken, user));
        }
        catch (Exception ex)
        {
            return Result<AuthResult>.Failure(ex.Message);
        }
    }

    public async Task<Result<AuthResult>> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return Result<AuthResult>.Failure("Refresh token is required");
            }

            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.RefreshToken == refreshToken);

            if (user is null)
            {
                return Result<AuthResult>.Failure("Invalid refresh token");
            }

            if (!user.IsRefreshTokenValid())
            {
                return Result<AuthResult>.Failure("Refresh token has expired");
            }

            var newAccessToken = _tokenGenerator.GenerateToken(user);
            var newRefreshToken = _tokenGenerator.GenerateRefreshToken();
            user.SetRefreshToken(newRefreshToken);
            await _userRepository.UpdateAsync(user);

            return Result<AuthResult>.Success(AuthResult.FromUser(newAccessToken, newRefreshToken, user));
        }
        catch (Exception ex)
        {
            return Result<AuthResult>.Failure(ex.Message);
        }
    }

    public async Task<Result<string>> ForgotPasswordAsync(string email)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user is null)
            {
                return Result<string>.Failure("User not found");
            }

            var resetToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
            user.GeneratePasswordResetToken(resetToken);
            await _userRepository.UpdateAsync(user);

            // Send password reset email
            var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
            var resetLink = $"{frontendUrl}/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(resetToken)}";
            
            var emailBody = $@"
                <h2>Password Reset Request</h2>
                <p>Hello {user.FullName},</p>
                <p>We received a request to reset your password. Click the link below to proceed:</p>
                <p><a href='{resetLink}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>Reset Password</a></p>
                <p>This link will expire in 15 minutes.</p>
                <p>If you didn't request a password reset, please ignore this email.</p>
                <p>Best regards,<br/>The Prospera Team</p>
            ";

            await _emailService.SendEmailAsync(email, "Password Reset Request", emailBody);

            return Result<string>.Success(resetToken);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }
    }

    public async Task<Result<string>> ResetPasswordAsync(string email, string token, string newPassword)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user is null)
            {
                return Result<string>.Failure("User not found");
            }

            if (!user.IsPasswordResetTokenValid() || user.PasswordResetToken != token)
            {
                return Result<string>.Failure("Invalid or expired reset token");
            }

            var passwordHash = _passwordHasher.HashPassword(newPassword);
            user.SetPasswordHash(passwordHash);
            user.ClearPasswordResetToken();
            await _userRepository.UpdateAsync(user);

            return Result<string>.Success("Password reset successful");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }
    }

    public async Task<Result<AuthResult>> GetCurrentUserAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
            {
                return Result<AuthResult>.Failure("User not found");
            }

            var refreshToken = user.RefreshToken ?? _tokenGenerator.GenerateRefreshToken();
            if (string.IsNullOrWhiteSpace(user.RefreshToken))
            {
                user.SetRefreshToken(refreshToken);
                await _userRepository.UpdateAsync(user);
            }
            
            var token = _tokenGenerator.GenerateToken(user);
            return Result<AuthResult>.Success(AuthResult.FromUser(token, refreshToken, user));
        }
        catch (Exception ex)
        {
            return Result<AuthResult>.Failure(ex.Message);
        }
    }
}