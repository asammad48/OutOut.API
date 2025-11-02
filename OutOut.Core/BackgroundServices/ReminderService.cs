using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutOut.Constants.Enums;
using OutOut.Infrastructure.Services;
using OutOut.Models;
using OutOut.Models.Identity;
using OutOut.Models.Models;
using OutOut.Persistence.Interfaces;

namespace OutOut.Core.BackgroundServices
{
    public class ReminderService : BackgroundService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly NotificationSenderService _notificationSenderService;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppSettings _appSettings;
        private readonly ILogger<ReminderService> _logger;

        public IServiceScopeFactory Services { get; }

        public ReminderService(IServiceScopeFactory services)
        {
            var sp = services.CreateScope().ServiceProvider;
            _notificationRepository = sp.GetRequiredService<INotificationRepository>();
            _notificationSenderService = sp.GetRequiredService<NotificationSenderService>();
            _userRepository = sp.GetRequiredService<IUserRepository>();
            _userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            _appSettings = sp.GetRequiredService<IOptions<AppSettings>>().Value;
            _logger = sp.GetRequiredService<ILogger<ReminderService>>();
            Services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting ReminderService...");
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var sp = Services.CreateScope().ServiceProvider;
                    var _notificationSenderService = sp.GetRequiredService<NotificationSenderService>();

                    var pendingReminders = await _notificationRepository.GetPendingReminders();

                    foreach (var reminder in pendingReminders)
                    {
                        var user = await _userManager.FindByIdAsync(reminder.UserId);
                        var fcmTokens = await _userRepository.GetFirebaseMessagingTokens(reminder?.UserId);

                        if (!user.RemindersAllowed || fcmTokens == null || !fcmTokens.Any())
                        {
                            reminder.NotificationStatus = NotificationStatus.SentSilent;
                            reminder.SentDate = DateTime.UtcNow;
                            await _notificationRepository.Update(reminder);
                            continue;
                        }

                        var data = new Dictionary<string, string>
                        {
                            { "payload", $"{(int)reminder.Action}" },
                            { "payload_parameter", $"{reminder.AffectedId}" },
                        };

                        var idd1 = reminder.EventBooking?.Id;
                        var idd2 = reminder.VenueBooking?.Id;
                        var response = await _notificationSenderService.Send(fcmTokens, reminder?.Title, reminder?.Body, data, null, notificationId: idd1 ?? idd2);
                        response?.Responses.ToList().ForEach(a =>
                        {
                            reminder?.Log.Add(new Log
                            {
                                IsSuccess = a.IsSuccess,
                                Message = a.Exception?.Message,
                            });
                        });
                        if (response != null)
                        {
                            if (response.Responses.All(r => !r.IsSuccess))
                                reminder.NotificationStatus = NotificationStatus.Failed;
                            else
                            {
                                reminder.NotificationStatus = NotificationStatus.Sent;
                                reminder.SentDate = DateTime.UtcNow;
                            }
                        }
                        await _notificationRepository.Update(reminder);
                        break;
                    }

                    if (pendingReminders.Any())
                    {
                        var minutesDelay = _appSettings.RemindersMinutesDelay;
                        await Task.Delay(4 * 1000);
                    }
                    else
                    {
                        await Task.Delay(30 * 1000);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Operation was cancelled");
            }
        }
    }
}
