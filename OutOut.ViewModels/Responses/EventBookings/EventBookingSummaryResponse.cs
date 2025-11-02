using OutOut.Constants.Enums;
using OutOut.ViewModels.Responses.Events;
using OutOut.ViewModels.Responses.Users;
using OutOut.ViewModels.Responses.Venues;
using System;
using System.Collections.Generic;

namespace OutOut.ViewModels.Responses.EventBookings
{
    public class EventBookingSummaryResponse
    {
        public string Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public EventSummaryResponse Event { get; set; }
        public VenueMiniSummaryResponse Venue { get; set; }
        public int Quantity { get; set; }
        public double TotalAmount { get; set; }
        public string Currency { get; set; } = "AED";
        public PaymentStatus Status { get; set; }
        public List<TicketResponse> Tickets { get; set; }
        public List<ReminderType> Reminders { get; set; }
        public int OrderNumber { get; set; }
        public ApplicationUserSummaryResponse User { get; set; }
        public EventPackageSummaryResponse Package { get; set; }
    }
}
