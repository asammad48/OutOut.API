using Microsoft.AspNetCore.SignalR;
using OutOut.Constants;
using OutOut.Constants.Enums;
using OutOut.Models.Identity;
using OutOut.Models.Models;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOut.Infrastructure.Services
{
    public class NotificationComposerService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly NotificationSenderService _notificationSenderService;
        private readonly IUserRepository _userRepository;
        private readonly IUserDetailsProvider _userDetailsProvider;
        private readonly IHubContext<NotificationHub, INotificationHub> _hub;

        public NotificationComposerService(INotificationRepository notificationRepnository,
                                           NotificationSenderService notificationSenderService,
                                           IUserRepository userRepository,
                                           IHubContext<NotificationHub, 
                                           INotificationHub> hub, 
                                           IUserDetailsProvider userDetailsProvider)
        {
            _notificationRepository = notificationRepnository;
            _notificationSenderService = notificationSenderService;
            _userRepository = userRepository;
            _hub = hub;
            _userDetailsProvider = userDetailsProvider;
        }

        public async Task SendNotification(Notification notification, ApplicationUser user, NotificationAction notificationPayload = 0, string payloadParameter = null)
        {
            if (user == null) return;
            var fcmTokens = await _userRepository.GetFirebaseMessagingTokens(user.Id);
            var data = new Dictionary<string, string>
                    {
                        { "payload", $"{(int)notificationPayload}" },
                        { "payload_parameter", $"{payloadParameter}" },
                    };

            if (!user.NotificationsAllowed || fcmTokens == null || !fcmTokens.Any())
            {
                notification.NotificationStatus = NotificationStatus.SentSilent;
                notification.SentDate = DateTime.UtcNow;
                await _notificationRepository.Update(notification);
                return;
            }

            var response = await _notificationSenderService.Send(fcmTokens, notification.Title, notification.Body, data, null);

            response?.Responses.ToList().ForEach(a =>
            {
                notification?.Log.Add(new Log
                {
                    IsSuccess = a.IsSuccess,
                    Message = a.Exception?.Message,
                });
            });

            if (response != null)
            {
                if (response.Responses.All(r => !r.IsSuccess))
                    notification.NotificationStatus = NotificationStatus.Failed;

                else
                {
                    notification.NotificationStatus = NotificationStatus.Sent;
                    notification.SentDate = DateTime.UtcNow;
                }
            }
            await _notificationRepository.Update(notification);
        }

        public async Task SendSignalRNotification(NotificationAction action, string body, string affectedId, List<string> roles, string accessibleVenue = null, string accessibleEvent = null)
        {
            foreach (var role in roles)
                await SendSignalRNotification(action, body, affectedId, role, accessibleVenue, accessibleEvent);
        }
        public async Task SendSignalRNotification(NotificationAction action, string body, string affectedId,  List<string> roles, string exceptedUserId, string accessibleVenue = null, string accessibleEvent = null)
        {
            foreach (var role in roles)
                await SendSignalRNotification(action, body, affectedId, role, exceptedUserId, accessibleVenue, accessibleEvent);
        }
        public async Task SendSignalRNotification(NotificationAction action, string body, string affectedId, string role, string accessibleVenue = null, string accessibleEvent = null)
        {
            var userIds = GetUserIds(role, accessibleVenue, accessibleEvent);
            if(!userIds.Any() || userIds == null) return;
            var notificationTasks = new List<Task>();
            foreach (var userId in userIds)
            {
                var notification = new Notification(userId, action, affectedId, body);
                await _notificationRepository.Create(notification);
                notificationTasks.Add(_hub.Clients.Group(userId).ReceiveNotification(notification));
            }
            await Task.WhenAll(notificationTasks);
        }
        public async Task SendSignalRNotification(NotificationAction action, string body, string affectedId, string role,string exceptedUserId, string accessibleVenue = null, string accessibleEvent = null)
        {
            var userIds = GetUserIds(role, accessibleVenue, accessibleEvent);
            userIds.Remove(exceptedUserId);
            if (!userIds.Any() || userIds == null) return;
            var notificationTasks = new List<Task>();
            foreach (var userId in userIds)
            {
                var notification = new Notification(userId, action, affectedId, body);
                await _notificationRepository.Create(notification);
                notificationTasks.Add(_hub.Clients.Group(userId).ReceiveNotification(notification));
            }
            await Task.WhenAll(notificationTasks);
        }
        public async Task SendSignalRNotificationToUser(NotificationAction action, string body, string affectedId, string userId)
        {
            var notification = new Notification(userId, action, affectedId, body);
            await _notificationRepository.Create(notification);
            await _hub.Clients.Group(userId).ReceiveNotification(notification);
        }

        private List<string> GetUserIds(string role, string accessibleVenue = null, string accessibleEvent = null)
        {
            var userIds = new List<string>();

            if (role == Roles.SuperAdmin)
                userIds = _userDetailsProvider.GetSuperAdmins();

            else if (role == Roles.VenueAdmin && accessibleVenue == null && accessibleEvent == null)
                userIds = _userDetailsProvider.GetVenueAdmins();

            else if (role == Roles.EventAdmin &&  accessibleEvent == null)
                userIds = _userDetailsProvider.GetEventAdmins();

            else if (role == Roles.VenueAdmin && accessibleVenue != null)
                userIds = _userDetailsProvider.GetVenueAdminsWithAccessibleVenue(accessibleVenue);

            else if (role == Roles.VenueAdmin && accessibleEvent != null)
                userIds = _userDetailsProvider.GetVenueAdminsWithAccessibleEvent(accessibleEvent);

            else if (role == Roles.EventAdmin && accessibleEvent != null)
                userIds = _userDetailsProvider.GetEventAdminsWithAccessibleEvent(accessibleEvent);
           
            return userIds;
        }
    }
}
