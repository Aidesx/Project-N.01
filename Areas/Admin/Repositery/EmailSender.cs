using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Project_385.Areas.Admin.Repositery
{
    public class EmailSender: IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential("hphanngoc120@gmail.com", "ahlqxhlxjenbxdca") //khoa truy cap

            };

            var mailMessage = new MailMessage(
                from: "hphanngoc120@gmail.com",
                to: email,
                subject: subject,
                body: message
            );
            mailMessage.IsBodyHtml = true;
            return client.SendMailAsync(mailMessage);
        }
    }
}
