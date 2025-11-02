using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using OutOut.Models;

namespace OutOut.Infrastructure.Services
{
    public class EmailComposerService
    {
        private readonly EmailSenderService _emailSender;
        private readonly AppSettings _appSettings;
        private readonly string _pathToEmailTemplates;

        public EmailComposerService(IOptions<AppSettings> appSettingsProperty, IWebHostEnvironment env, EmailSenderService emailSender)
        {
            _emailSender = emailSender;
            _appSettings = appSettingsProperty.Value;
            _pathToEmailTemplates = env.WebRootPath
                            + Path.DirectorySeparatorChar.ToString()
                            + "email-templates"
                            + Path.DirectorySeparatorChar.ToString();
        }

        private string GetMessageBodyFromHtml(string fileName)
        {
            string messageBody;
            using (StreamReader SourceReader = File.OpenText(_pathToEmailTemplates + fileName))
            {
                messageBody = SourceReader.ReadToEnd();
            }
            return messageBody.Replace("{images}", _appSettings.BackendOrigin + "/email-templates/images");
        }

        private string ReplaceFromBody(string templateName, Dictionary<string, string> toBeReplaced)
        {
            string messageBody = GetMessageBodyFromHtml($"{templateName}.html");

            foreach (KeyValuePair<string, string> entry in toBeReplaced)
            {
                messageBody = messageBody.Replace(entry.Key, entry.Value);
            }

            return messageBody;
        }

        public Task SendSuccessRegistrationMail(string email, string fullName, string code)
        {
            var htmlBody = ReplaceFromBody("SuccessRegisterEmail", 
                new Dictionary<string, string> { 
                    { "{User.FullName}", fullName } ,
                    { "{VerificationOTP}", code } ,
                });

            return _emailSender.SendEmail(email, fullName, "Successful registration and validation", htmlBody);
        }

        public Task SendResetPasswordEmail(string email, string fullName, string code)
        {
            var htmlBody = ReplaceFromBody("ResetPasswordEmail",
                new Dictionary<string, string> {
                    { "{User.FullName}", fullName } ,
                    { "{ResetOTP}", code } ,
                });

            return _emailSender.SendEmail(email, fullName, "Reset Password", htmlBody);
        }

        public Task SendExternalSuccessRegistrationMail(string email, string fullName)
        {
            var htmlBody = ReplaceFromBody("ExternalSuccessRegisterEmail",
                new Dictionary<string, string> {
                    { "{User.FullName}", fullName }
                });

            return _emailSender.SendEmail(email, fullName, "Successful Registration", htmlBody);
        }

        public Task SendCustomMail(string email, string fullName,string subJect,string htmlBody)
        {

            return _emailSender.SendEmail(email, fullName, subJect, htmlBody);
        }
    }
}
