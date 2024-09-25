using System.Net;
using System.IO;                 
using System.Net.Mail;            
using System.Threading.Tasks;     
using Microsoft.Extensions.Configuration;


namespace DormBuddy.Models {
    public class Smtp : IEmailSender
    {
        private readonly IConfiguration _configuration;

        private readonly IConfigurationSection smtpSettings;

        public Smtp(IConfiguration configuration)
        {
            _configuration = configuration;
            smtpSettings = _configuration.GetSection("SmtpSettings");
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {

            using (var client = new SmtpClient(smtpSettings["Server"], int.Parse(smtpSettings["Port"] ?? "25")))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]);
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSettings["SenderEmail"] ?? throw new ArgumentNullException("Email address cannot be null"), smtpSettings["SenderName"]),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
        }

        // Method to generate email content
        public string GenerateEmailContent(ApplicationUser user, string activationLink, string fileName)
        {
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            var template = File.ReadAllText(templatePath);

            // Replace placeholders with actual values
            template = template.Replace("{UserName}", user.UserName);
            template = template.Replace("{ActivationLink}", activationLink);

            return template;
        }

        public async Task<bool> SendActivationEmail(ApplicationUser user, string link)
        {
            try
            {
                string subject = "Account Activation";
                string message = GenerateEmailContent(user, link, "activationemail.html");
                if (!string.IsNullOrEmpty(user.Email))
                    await SendEmailAsync(user.Email, subject, message); // Just await, no need for a result
                else
                    Console.WriteLine("Email is null! Not Sent!");
                Console.WriteLine("Account activation email has been sent for " + user.Email);

                return true; // Email sent successfully
            }
            catch (Exception)
            {
                // Handle the error (log it, etc.)
                return false; // Email sending failed
            }
        }

        public async Task<bool> SendPasswordResetEmail(ApplicationUser user, string link)
        {
            try
            {
                string subject = "Reset Password";
                string message = GenerateEmailContent(user, link, "passwordresetemail.html");
                if (!string.IsNullOrEmpty(user.Email))
                    await SendEmailAsync(user.Email, subject, message); // Just await, no need for a result
                else
                    Console.WriteLine("Password reset email is null! Not Sent!");
                Console.WriteLine("Password reset email has been sent for " + user.Email);

                return true; // Email sent successfully
            }
            catch (Exception)
            {
                // Handle the error (log it, etc.)
                return false; // Email sending failed
            }
        }

    }
}
