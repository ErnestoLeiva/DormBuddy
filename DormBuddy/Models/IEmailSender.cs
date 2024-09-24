namespace DormBuddy.Models
{
    public interface IEmailSender
    {
    Task SendEmailAsync(string toEmail, string subject, string message);
    Task<bool> SendActivationEmail(ApplicationUser user, string link);
    }
}