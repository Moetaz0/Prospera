using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace Prospera.Infrastructure.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    Task SendWelcomeEmailAsync(string to, string name, CancellationToken cancellationToken = default);
    Task SendBudgetAlertAsync(string to, string budgetName, decimal exceeded, CancellationToken cancellationToken = default);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        try
        {
            var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPassword = _configuration["Email:SmtpPassword"];
            var fromEmail = _configuration["Email:FromEmail"];
            var fromName = _configuration["Email:FromName"] ?? "Prospera";

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail!, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage, cancellationToken);

            _logger.LogInformation("Email sent successfully to {Email}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", to);
            throw;
        }
    }

    public async Task SendWelcomeEmailAsync(string to, string name, CancellationToken cancellationToken = default)
    {
        var subject = "Welcome to Prospera!";
        var body = $@"
            <h1>Welcome to Prospera, {name}!</h1>
            <p>We're excited to have you on board.</p>
            <p>Start managing your finances with AI-powered insights.</p>
            <p>Best regards,<br/>The Prospera Team</p>
        ";

        await SendEmailAsync(to, subject, body, cancellationToken);
    }

    public async Task SendBudgetAlertAsync(string to, string budgetName, decimal exceeded, CancellationToken cancellationToken = default)
    {
        var subject = $"Budget Alert: {budgetName}";
        var body = $@"
            <h2>Budget Alert</h2>
            <p>You have exceeded your <strong>{budgetName}</strong> budget by <strong>${exceeded:N2}</strong>.</p>
            <p>Please review your spending in the Prospera app.</p>
            <p>Best regards,<br/>The Prospera Team</p>
        ";

        await SendEmailAsync(to, subject, body, cancellationToken);
    }
}