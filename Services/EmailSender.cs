using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace Project_385.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            var host = _configuration["EmailSettings:Host"] ?? "smtp.gmail.com";
            var portString = _configuration["EmailSettings:Port"] ?? "587";
            int.TryParse(portString, out int port);
            if (port <= 0) port = 587;

            var senderEmail = _configuration["EmailSettings:Email"];
            var password = _configuration["EmailSettings:Password"];

            var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(senderEmail, password)
            };

            var mailMessage = new MailMessage(
                from: senderEmail,
                to: email,
                subject: subject,
                body: message
            )
            {
                IsBodyHtml = true
            };

            return client.SendMailAsync(mailMessage);
        }
    }
}
