using OutOut.Constants.Enums;
using System;
using System.Collections.Generic;

namespace OutOut.ViewModels.Responses.VenueBooking
{
    public class VenueBookingSummaryResponse
    {
        public string Id { get; set; }
        public int BookingNumber { get; set; }
        public int PeopleNumber { get; set; }
        public DateTime Date { get; set; }
        public VenueBookingStatus Status { get; set; }
        public List<ReminderType> RemindersTypes { get; set; }
    }
}
