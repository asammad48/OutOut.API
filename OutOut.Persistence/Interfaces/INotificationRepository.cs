using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Interfaces
{
    public interface INotificationRepository : IGenericNonSqlRepository<Notification>
    {
        Task<List<Notification>> GetPendingReminders();
        Task<List<Notification>> GetByVenueBookingId(string venueBookingId);
        Task<bool> DeleteReminders(List<Notification> notifications);
        Task<Page<Notification>> GetMyNotifications(PaginationRequest paginationRequest, string userId);
        Task<bool> MarkNotificationAsRead(List<string> notificationIds);
        Task<List<Notification>> GetByEventBookingId(string eventBookingId, string userId);
        Task<bool> DeleteRemindersWithDeactivatedVenues(List<string> venueIds);
        Task<bool> DeleteRemindersWithDeactivatedEvent(List<string> eventsIds);
        Task<bool> DeleteRemindersForRejectedBooking(string bookingId, string userId);
        Task<long> GetCustomerUnReadNotificationsCount(string userId);
        Task<long> GetAdminUnReadNotificationsCount(string userId);
        Task<bool> MarkAllNotificationsAsRead(string userId);
    }
}
