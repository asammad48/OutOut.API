using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using OutOut.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

namespace OutOut.Infrastructure.Services
{
    public class EmailSenderService
    {
        private readonly SMTPConfiguration _smtpConfig;
        private readonly ILogger<EmailSenderService> _logger;
        private readonly IWebHostEnvironment _env;
        public EmailSenderService(IOptions<AppSettings> appSettings, IWebHostEnvironment env, ILogger<EmailSenderService> logger)
        {
            _smtpConfig = appSettings.Value.SMTPConfiguration;
            _logger = logger;
            _env = env;
        }

        public async Task<bool> SendEmail(string toEmail, string toDisplayName, string subject, string htmlBody)
        {
            MimeMessage msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_smtpConfig.DisplayName, _smtpConfig.Mail));
            msg.To.Add(new MailboxAddress(toDisplayName, toEmail));

            msg.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };

            msg.Body = bodyBuilder.ToMessageBody();

            try
            {
                //TODO: remove smtp.html before going live
                //await File.AppendAllTextAsync(_env.WebRootPath + "/" + "smtp.html", htmlBody + "\n\n\n\n\n\n");

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_smtpConfig.Host, _smtpConfig.Port, SecureSocketOptions.StartTls);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(_smtpConfig.Mail, _smtpConfig.Password);
                    await client.SendAsync(msg);
                    await client.DisconnectAsync(true);
                };
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while trying to send an Email.");
            }
            return false;
        }
    }
}
