using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CosmeticShopAPI.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
    }

    public class EmailService : IEmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPass;
        private readonly string _fromEmail;

        public EmailService(IConfiguration configuration)
        {
            _smtpHost = configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = configuration.GetValue<int>("Email:SmtpPort", 587);
            _smtpUser = configuration["Email:SmtpUser"] ?? "katelol129ioi@gmail.com";
            _smtpPass = configuration["Email:SmtpPass"] ?? "skmr vfal rgnx gcsb";
            _fromEmail = configuration["Email:FromEmail"] ?? "katelol129ioi@gmail.com";
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            using var smtpClient = new SmtpClient(_smtpHost, _smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_smtpUser, _smtpPass),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}