using System;

namespace OutOut.ViewModels.Responses.Customers
{
    public class CustomerEventBookingResponse
    {
        public string EventId { get; set; }
        public string EventOccurrenceId { get; set; }
        public string EventName { get; set; }
        public string PackageName { get; set; }
        public string CityName { get; set; }
        public long RedeemedTicketsCount { get; set; }
        public DateTime EventOccurrenceStartDate { get; set; }
    }
}
