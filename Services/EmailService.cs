using System.Net;
using System.Net.Mail;

namespace CheckCarsAPI.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration configuration)
        {
            _config = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
    {
        var smtpClient = new SmtpClient
        {
            Host = _config["Smtp:Host"],
            Port = int.Parse(_config["Smtp:Port"]),
            Credentials = new NetworkCredential(
                _config["Smtp:Username"], 
                _config["Smtp:Password"]),
            EnableSsl = true
        };

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(_config["Smtp:From"]),
            Subject = subject,
            Body = message,
            IsBodyHtml = true
        };

        mailMessage.To.Add(email);
        await smtpClient.SendMailAsync(mailMessage);
    }
    }
}