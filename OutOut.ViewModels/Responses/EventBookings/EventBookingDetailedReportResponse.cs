using OutOut.Constants.Enums;
using OutOut.ViewModels.Responses.Events;
using OutOut.ViewModels.Responses.Users;
using System;

namespace OutOut.ViewModels.Responses.EventBookings
{
    public class EventBookingDetailedReportResponse
    {
        public string Id { get; set; }
        public int OrderNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public double TotalAmount { get; set; }
        public string Currency { get; set; } = "AED";
        public PaymentStatus Status { get; set; }
        public ApplicationUserSummaryResponse User { get; set; }
        public long Attendees { get; set; }
        public long Absentees { get; set; }
        public string EventId { get; set; }
        public string EventName { get; set; }
        public EventOccurrenceResponse Occurrence { get; set; }
    }
}
