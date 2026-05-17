using BackendService.Configurations;
using BackendService.Services.Interface;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BackendService.Services
{
    public class SMTPEmailService : IEmailService
    {
        private readonly EmailOptions _emailOptions;

        public SMTPEmailService(IOptions<EmailOptions> emailOptions)
        {
            _emailOptions = emailOptions.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_emailOptions.Sender.Email);
            email.Sender.Name = _emailOptions.Sender.Name;
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlMessage };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_emailOptions.Credential.SmtpServer, _emailOptions.Credential.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_emailOptions.Credential.Username, _emailOptions.Credential.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
