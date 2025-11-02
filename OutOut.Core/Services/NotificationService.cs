using AutoMapper;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using OutOut.Constants.Enums;
using OutOut.Constants.Errors;
using OutOut.Infrastructure.Services;
using OutOut.Models.Exceptions;
using OutOut.Models.Identity;
using OutOut.Models.Models;
using OutOut.Models.Models.Embedded;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Data;
using OutOut.Persistence.Extensions;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Providers;
using OutOut.ViewModels.Responses.Notifications;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Core.Services
{
    public class NotificationService
    {
        private readonly IUserRepository _userRepository;
        private readonly NotificationComposerService _notificationComposerService;
        private readonly INotificationRepository _notificationRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationNonSqlDbContext _dbContext;
        private readonly IMapper _mapper;
        private IMongoCollection<ApplicationUser> _usersCollection;
        private readonly IUserDetailsProvider _userDetailsProvider;

        public NotificationService(IUserRepository userRepository,
                                   NotificationComposerService notificationComposerService,
                                   INotificationRepository notificationRepository,
                                   UserManager<ApplicationUser> userManager,
                                   ApplicationNonSqlDbContext dbContext,
                                   IMapper mapper,
                                   IUserDetailsProvider userDetailsProvider)
        {
            _userRepository = userRepository;
            _notificationComposerService = notificationComposerService;
            _notificationRepository = notificationRepository;
            _userManager = userManager;
            _dbContext = dbContext;
            _mapper = mapper;
            _usersCollection = _dbContext.GetCollection<ApplicationUser>();
            _userDetailsProvider = userDetailsProvider;
        }

        public async Task<CustomNotificationPage<NotificationResponse>> GetMyNotifications(PaginationRequest paginationRequest)
        {
            var user = await _userManager.FindByIdAsync(_userDetailsProvider.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var notifications = await _notificationRepository.GetMyNotifications(paginationRequest, user.Id);
            var unReadNotificationsCount = await _notificationRepository.GetCustomerUnReadNotificationsCount(user.Id);
            var notificationsResult = _mapper.Map<Page<NotificationResponse>>(notifications);

            return new CustomNotificationPage<NotificationResponse>(notificationsResult.Records, notificationsResult.PageNumber, notificationsResult.PageSize, notificationsResult.RecordsTotalCount, unReadNotificationsCount);
        }


        public async Task<long> GetUserUnReadNotificationsCount()
        {
            var user = await _userManager.FindByIdAsync(_userDetailsProvider.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var unReadNotificationsCount = await _notificationRepository.GetCustomerUnReadNotificationsCount(user.Id);

            return unReadNotificationsCount;

        }

        public async Task<bool> MarkNotificationsAsRead(List<string> notificationIds)
        {
            if (notificationIds == null || !notificationIds.Any())
                throw new OutOutException(ErrorCodes.InvalidNullParameters);

            var result = await _notificationRepository.MarkNotificationAsRead(notificationIds);
            return result;
        }

        public async Task<bool> MarkAllNotificationsAsRead()
        {
            var result = await _notificationRepository.MarkAllNotificationsAsRead(_userDetailsProvider.UserId);
            return result;
        }

        public async Task<CustomNotificationPage<NotificationAdminResponse>> GetMyNotificationsForAdmin(PaginationRequest paginationRequest)
        {
            var user = await _userManager.FindByIdAsync(_userDetailsProvider.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var notifications = await _notificationRepository.GetMyNotifications(paginationRequest, user.Id);
            var unreadCount = await _notificationRepository.GetAdminUnReadNotificationsCount(user.Id);

            var notificationsWithUnread = new CustomNotificationPage<Notification>(notifications.Records, notifications.PageNumber, notifications.PageSize, notifications.RecordsTotalCount, unreadCount);
            return _mapper.Map<CustomNotificationPage<NotificationAdminResponse>>(notificationsWithUnread);
        }

        public async Task SendUseAppReminder()
        {
            var users = await _userRepository.GetInactiveUsers();
            foreach (var user in users)
            {
                if ((user.LastUsage.LastNotificationSentDate.Date - DateTime.UtcNow.AddDays(-30).Date).TotalDays % 30 != 0 && user.LastUsage.LastNotificationSentDate.Date != DateTime.MinValue)
                    continue;

                var notification = new Notification(NotificationType.Notification,
                                                    user.Id,
                                                    "Missed you",
                                                    "Hey, you haven’t used OutOut in a while, why don’t you see what offers we have for you?",
                                                    "venue.png",
                                                    NotificationAction.NotUsingAppInAWhile);
                await _notificationRepository.Create(notification);

                await _notificationComposerService.SendNotification(notification, user);

                if (notification.NotificationStatus == NotificationStatus.Sent)
                {
                    user.LastUsage.LastNotificationSentDate = DateTime.UtcNow.Date;
                    await _userManager.UpdateAsync(user);
                }
            }
        }

        public async Task SendNewVenueNearYouNotifications(Venue venue)
        {
            var notificationTasks = new List<Task>();
            await _usersCollection.ParallelForEachAsync(_userRepository.GetDistance(venue.Location), async user =>
            {
                var notification = new Notification(NotificationType.Notification, user.Id, "New venue", "New Venue in your area", "venue.png", NotificationAction.NewVenueInYourArea, venue: _mapper.Map<VenueSummary>(venue));
                await _notificationRepository.Create(notification);
                notificationTasks.Add(_notificationComposerService.SendNotification(notification, user, NotificationAction.NewVenueInYourArea, venue.Id));
            });
            await Task.WhenAll(notificationTasks);
        }

        public async Task SendNewOfferNearYouNotifications(Offer offer, Venue venue)
        {
            var notificationTasks = new List<Task>();
            var offerSummary = new VenueOfferSummary(venue.Id, venue.Name, offer);
            await _usersCollection.ParallelForEachAsync(_userRepository.GetDistance(venue.Location), async user =>
            {
                var notification = new Notification(NotificationType.Notification, user.Id, "New Offer", $"New Offer in your area  '{offer?.Type?.Name}'", offer.Image, NotificationAction.NewOffer, offer: offerSummary, affectedId: venue.Id);
                await _notificationRepository.Create(notification);
                notificationTasks.Add(_notificationComposerService.SendNotification(
                                                                            notification,
                                                                            user,
                                                                            NotificationAction.NewOffer,
                                                                            venue.Id));
            });
            await Task.WhenAll(notificationTasks);
        }

        public async Task SendNewEventNearYouNotifications(Event newEvent)
        {
            try
            {
                var notificationTasks = new List<Task>();
                await _usersCollection.ParallelForEachAsync(_userRepository.GetDistance(newEvent.Location), async user =>
                {
                    var notification = new Notification(NotificationType.Notification, user.Id, "New event", "New Event in your area", "event.png", NotificationAction.NewEvent, eventSummary: _mapper.Map<EventSummary>(newEvent));
                    await _notificationRepository.Create(notification);
                    notificationTasks.Add(_notificationComposerService.SendNotification(notification, user, NotificationAction.NewEvent, newEvent.Occurrences.FirstOrDefault().Id));
                });
                await Task.WhenAll(notificationTasks);
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
