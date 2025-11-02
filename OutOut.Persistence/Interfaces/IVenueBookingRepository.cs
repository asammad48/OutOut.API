using OutOut.Models.Identity;
using OutOut.Models.Models;
using OutOut.Models.Models.Embedded;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.ViewModels.Requests.Bookings;
using OutOut.ViewModels.Requests.Customers;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Requests.VenueBooking;
using OutOut.ViewModels.Responses.Bookings;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Interfaces
{
    public interface IVenueBookingRepository : IGenericNonSqlRepository<VenueBooking>
    {
        Task<List<ApplicationUserSummary>> GetUsersByVenueIds(List<string> venueIds, FilterationRequest filterRequest = null);
        Task<VenueBooking> UpdateVenueBooking(string id, VenueBooking venueBooking);
        Task<Page<VenueBooking>> GetMyBooking(PaginationRequest paginationRequest, MyBookingFilterationRequest filterRequest, string userId);
        long GetApprovedBookingsCount(string venueId);
        Task<List<VenueBooking>> RejectBookingsForDeactivatedVenues(List<string> venueIds);
        Task<bool> ApproveBooking(string bookingId);
        Task<bool> RejectBooking(string bookingId);
        long GetAllBookingsCount(string venueId);
        Task<List<BookingResponse>> GetAllBookings(BookingFilterationRequest filterRequest, List<string> accessibleVenues, bool isSuperAdmin);
        Task<List<VenueBooking>> GetBookingsByVenueId(string venueId, FilterationRequest filterRequest = null);
        long GetCancelledBookingsCount(string venueId);
        Task<List<VenueBooking>> GetVenueBookingDetailedReport(string venueId, VenueBookingReportFilterRequest filterRequest, List<string> bookingsIds = null);
        long GetBookingsCountPerVenueByUserId(string userId, string venueId);
        Task SyncUserWithVenueBookings(ApplicationUser oldUser, ApplicationUser newUser);
        Task<bool> DeleteBookingsForDeletedVenue(string venueId);
    }
}
