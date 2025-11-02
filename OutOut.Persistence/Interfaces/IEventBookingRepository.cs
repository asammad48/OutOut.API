using OutOut.Models.Domains;
using OutOut.Models.Identity;
using OutOut.Models.Models;
using OutOut.Models.Models.Embedded;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.ViewModels.Requests.Bookings;
using OutOut.ViewModels.Requests.Customers;
using OutOut.ViewModels.Requests.EventBooking;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Requests.Ticket;
using OutOut.ViewModels.Requests.Venues;
using OutOut.ViewModels.Responses.Bookings;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Interfaces
{
    public interface IEventBookingRepository : IGenericNonSqlRepository<EventBooking>
    {
        Task<List<ApplicationUserSummary>> GetUsersByEventIds(List<string> eventIds, FilterationRequest filterRequest = null);
        Task<EventBooking> UpdateEventBooking(EventBooking eventBooking);
        Task<Page<EventBooking>> GetMyBooking(PaginationRequest paginationRequest, MyBookingFilterationRequest filterRequest, string userId);
        Task<EventBooking> GetEventBooking(string userId, string eventBookingId);
        Task<EventBooking> GetEventBookingByTicketId(string ticketId);
        Task<List<EventBooking>> GetStalePendingBooking();
        Task<Page<SingleEventBookingTicket>> GetMySharedTickets(PaginationRequest paginationRequest, MyBookingFilterationRequest filterRequest, List<SharedTicket> receivedTickets);
        Task<EventBooking> GetEventBookingByTicket(string ticketId, string ticketSecret);
        long GetPaidTicketsCount(string eventId, string occurrenceId = null);
        double GetRevenueForEvent(string eventId);
        Task<List<EventBooking>> GetBookingsByEventId(string eventId, FilterationRequest filterRequest = null);
        Task<List<EventBooking>> GetCustomerAttendedEvents(string userId, SearchFilterationRequest searchFilterationRequest, List<string> accessibleEvents, bool isSuperAdmin);
        Task<List<BookingResponse>> GetAllPaidAndRejectedBookings(BookingFilterationRequest filterationRequest, List<string> accessibleEvents, bool isSuperAdmin);
        long GetPaidBookingsCount(string eventId);
        long GetCustomersRedeemedTicketsCountPerOccurrence(string userId, string eventId);
        Task<bool> DeleteBookingRemindersForDeactivatedEvent(List<string> eventsIds);
        long GetAttendeesCountForEvent(string eventId);
        long GetAbsenteesCountForEvent(string eventId);
        Task<List<EventBooking>> GetEventBookingDetailedReport(string eventId, EventBookingReportFilterRequest filterRequest, List<string> bookingsIds = null);
        long GetAttendeesCountForBooking(string bookingId);
        long GetAbsenteesCountForBooking(string bookingId);
        long GetBookedTicketsCountPerPackage(string eventId, string packageId, EventBookingReportFilterRequest filterRequest);
        long GetRejectedTicketsCountPerPackage(string eventId, string packageId, EventBookingReportFilterRequest filterRequest);
        double GetTotalSalesPerPackage(string eventId, string packageId, EventBookingReportFilterRequest filterRequest);
        long GetPendingTicketsCount(string eventId);
        Task SyncUserWithEventBookings(ApplicationUser oldUser, ApplicationUser newUser);
        Task<bool> DeleteBookingsForDeletedEvent(string eventId);
        Task<Page<SingleEventBookingTicket>> GetTicketsPage(string bookingId, PaginationRequest paginationRequest);
        Task<Page<SingleEventBookingTicket>> GetTicketsRedeemedByUser(PaginationRequest pageRequest, TicketFilterationRequest request, string userId);
        Task<SingleEventBookingTicket> GetTicketDetails(string ticketId);

    }
}
