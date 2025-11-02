using OutOut.Constants.Enums;
using OutOut.ViewModels.Responses.Users;
using OutOut.ViewModels.Responses.Venues;
using System;

namespace OutOut.ViewModels.Responses.VenueBooking
{
    public class VenueBookingResponse
    {
        public string Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public int BookingNumber { get; set; }
        public int PeopleNumber { get; set; }
        public DateTime Date { get; set; }
        public VenueBookingStatus Status { get; set; }

        public VenueMiniSummaryResponse Venue { get; set; }
        public ApplicationUserSummaryResponse User { get; set; }
    }
}
