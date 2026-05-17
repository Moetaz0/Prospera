namespace Prospera.Infrastructure.Email;

public class EmailOptions
{
    public const string Section = "Email";

    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Prospera";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSSL { get; set; } = true;
}
