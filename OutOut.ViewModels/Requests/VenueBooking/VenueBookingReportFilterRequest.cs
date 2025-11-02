using OutOut.ViewModels.Requests.Fitlers;
using System;

namespace OutOut.ViewModels.Requests.VenueBooking
{
    public class VenueBookingReportFilterRequest
    {
        public string SearchQuery { get; set; }
        public VenueBookingReportSort Sort { get; set; }
        public FilteredField? FilteredField { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
    public enum VenueBookingReportSort
    {
        Newest, Alphabetical, Approved, Cancelled
    }
}
