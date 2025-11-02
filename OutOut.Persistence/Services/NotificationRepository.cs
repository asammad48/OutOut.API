using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OutOut.Constants.Enums;
using OutOut.Models;
using OutOut.Models.Models;
using OutOut.Models.Utils;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Data;
using OutOut.Persistence.Extensions;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.Persistence.Services.Basic;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Services
{
    public class NotificationRepository : GenericNonSqlRepository<Notification>, INotificationRepository
    {
        private readonly int _remindersMinutesDelay;

        public NotificationRepository(IOptions<AppSettings> appSettingsOptions, ApplicationNonSqlDbContext dbContext, IEnumerable<ISyncRepository<Notification>> syncRepositories) : base(dbContext, syncRepositories)
        {
            _remindersMinutesDelay = appSettingsOptions.Value.RemindersMinutesDelay + 5;
        }

        public async Task<List<Notification>> GetPendingReminders()
        {
            var filters = Builders<Notification>.Filter.Eq(a => a.NotificationStatus, NotificationStatus.Pending) &
                          Builders<Notification>.Filter.Lte(a => a.ToBeSentDate, UAEDateTime.Now);
            //&
              //            Builders<Notification>.Filter.Gte(a => a.ToBeSentDate, UAEDateTime.Now.AddMinutes(-_remindersMinutesDelay));
            return await Find(filters);
        }

        public async Task<List<Notification>> GetByVenueBookingId(string venueBookingId)
        {
            var venueBookingFilter = Builders<Notification>.Filter.Eq(a => a.VenueBooking.Id, venueBookingId);
            return await Find(venueBookingFilter);
        }

        public async Task<List<Notification>> GetByEventBookingId(string eventBookingId, string userId)
        {
            var eventBookingFilter = Builders<Notification>.Filter.Eq(a => a.EventBooking.Id, eventBookingId) &
                                     Builders<Notification>.Filter.Eq(a => a.UserId, userId);
            return await Find(eventBookingFilter);
        }

        public async Task<bool> DeleteReminders(List<Notification> notifications)
        {
            var filter = Builders<Notification>.Filter.In(a => a.Id, notifications.Select(a => a.Id));
            return await DeleteMany(filter);
        }

        public Task<Page<Notification>> GetMyNotifications(PaginationRequest paginationRequest, string userId)
        {
            var userFilter = Builders<Notification>.Filter.Eq(a => a.UserId, userId);
            var filters = Builders<Notification>.Filter.Eq(a => a.NotificationStatus, NotificationStatus.Sent) |
                          Builders<Notification>.Filter.Eq(a => a.NotificationStatus, NotificationStatus.SentSilent);

            var records = _collection.Find(userFilter & filters).SortByDescending(a => a.SentDate);

            return records.GetPaged(paginationRequest);
        }

        public async Task<bool> DeleteRemindersWithDeactivatedVenues(List<string> venueIds)
        {
            var filter = Builders<Notification>.Filter.Eq(a => a.NotificationStatus, NotificationStatus.Pending) &
                         Builders<Notification>.Filter.Eq(a => a.NotificationType, NotificationType.Reminder) &
                         Builders<Notification>.Filter.In(a => a.VenueBooking.VenueId, venueIds);
            return await DeleteMany(filter);
        }

        public async Task<bool> DeleteRemindersWithDeactivatedEvent(List<string> eventsIds)
        {
            var filter = Builders<Notification>.Filter.Eq(a => a.NotificationStatus, NotificationStatus.Pending) &
                         Builders<Notification>.Filter.Eq(a => a.NotificationType, NotificationType.Reminder) &
                         Builders<Notification>.Filter.In(a => a.EventBooking.EventId, eventsIds);
            return await DeleteMany(filter);
        }

        public async Task<bool> DeleteRemindersForRejectedBooking(string bookingId, string userId)
        {
            var filter = Builders<Notification>.Filter.Eq(a => a.NotificationType, NotificationType.Reminder) &
                         Builders<Notification>.Filter.Eq(a => a.Action, NotificationAction.VenueBookingReminder) &
                         Builders<Notification>.Filter.Eq(a => a.NotificationStatus, NotificationStatus.Pending) &
                         Builders<Notification>.Filter.Eq(a => a.UserId, userId) &
                         Builders<Notification>.Filter.Eq(a => a.VenueBooking.Id, bookingId);
            return await DeleteMany(filter);
        }

        public async Task<bool> MarkNotificationAsRead(List<string> notificationIds)
        {
            var filter = Builders<Notification>
                          .Filter
                              .In(x => x.Id, notificationIds);

            var update = Builders<Notification>
                         .Update
                      .Set(x => x.IsRead, true);

            return await UpdateMany(filter, update);
        }

        public async Task<bool> MarkAllNotificationsAsRead(string userId)
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.UserId, userId);
            var update=Builders<Notification>.Update.Set(n => n.IsRead, true);
            return await UpdateMany(filter, update);
        }

        public async Task<long> GetCustomerUnReadNotificationsCount(string userId)
        {
            var filter = Builders<Notification>.Filter.Eq(a => a.UserId, userId) &
                         Builders<Notification>.Filter.Eq(a => a.IsRead, false) &
                         Builders<Notification>.Filter.Ne(a => a.SentDate, null);
            return await Count(filter);
        }

        public async Task<long> GetAdminUnReadNotificationsCount(string userId)
        {
            var filter = Builders<Notification>.Filter.Eq(a => a.UserId, userId) &
                         (Builders<Notification>.Filter.Eq(a => a.IsRead, false) |
                         !Builders<Notification>.Filter.Exists(a => a.IsRead)) &
                         Builders<Notification>.Filter.Ne(a => a.SentDate, null);
            return await Count(filter);
        }
    }
}
