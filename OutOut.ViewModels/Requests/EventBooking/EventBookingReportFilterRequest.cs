using OutOut.ViewModels.Requests.Fitlers;
using System;

namespace OutOut.ViewModels.Requests.EventBooking
{
    public class EventBookingReportFilterRequest
    {
        public string SearchQuery { get; set; }
        public EventBookingReportSort? Sort { get; set; }
        public FilteredField? FilteredField { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
    public enum EventBookingReportSort
    {
        Newest, Alphabetical, Attended, Absent
    }
}
