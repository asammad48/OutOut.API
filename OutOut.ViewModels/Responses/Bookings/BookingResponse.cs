using OutOut.Constants.Enums;
using System;

namespace OutOut.ViewModels.Responses.Bookings
{
    public class BookingResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public TypeFor Type { get; set; }
        public string City { get; set; }
        public long BookingsCount { get; set; }
        public DateTime LastBookingDate { get; set; }
    }
}
