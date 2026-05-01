namespace Project_385.Areas.Admin.Repositery
{
    public interface IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message);
    }
}
