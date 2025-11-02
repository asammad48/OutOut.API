using OutOut.Constants.Enums;
using OutOut.ViewModels.Responses.Users;
using System;

namespace OutOut.ViewModels.Responses.VenueBooking
{
    public class VenueBookingDetailedReportResponse
    {
        public string Id { get; set; }
        public int BookingNumber { get; set; }
        public ApplicationUserSummaryResponse User { get; set; }
        public long TotalReservations { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ReservationDate { get; set; } 
        public string Status { get; set; }
    }
}
