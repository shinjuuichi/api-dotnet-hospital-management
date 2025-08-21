using System.Net;
using System.Net.Mail;

namespace WebAPI.Utils;

public interface IEmailSender
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var fromEmail = _configuration["Email:FromEmail"] ?? "noreply@clinic.com";
        var fromPassword = _configuration["Email:FromPassword"] ?? "";

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(fromEmail, fromPassword),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmail),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        mailMessage.To.Add(toEmail);

        try
        {
            await client.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to send email: {ex.Message}");
        }
    }
}
