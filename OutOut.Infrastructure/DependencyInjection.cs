using Microsoft.Extensions.DependencyInjection;
using OutOut.Infrastructure.Services;
using OutOut.Models;

namespace OutOut.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfraStructure(this IServiceCollection services, AppSettings appSettings)
        {
            services.AddSingleton<OTPService>();
            services.AddSingleton<EmailSenderService>();
            services.AddSingleton<EmailComposerService>();
            services.AddSingleton<FileUploaderService>();
            services.AddSingleton<GoogleAuthenticator>();
            services.AddSingleton<FacebookAuthenticator>();
            services.AddSingleton<AppleAuthenticator>();
            services.AddSingleton<NotificationSenderService>();
            services.AddScoped<NotificationComposerService>();
            services.AddScoped<PaymentService>();
            services.AddScoped<StringLockProvider>();
            return services;
        }
    }
}
