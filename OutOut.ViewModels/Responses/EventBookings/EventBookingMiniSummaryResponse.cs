using OutOut.Constants.Enums;
using OutOut.ViewModels.Responses.Events;
using System.Collections.Generic;

namespace OutOut.ViewModels.Responses.EventBookings
{
    public class EventBookingMiniSummaryResponse
    {
        public string Id { get; set; }
        public int Quantity { get; set; }
        public double TotalAmount { get; set; }
        public string Currency { get; set; } = "AED";
        public PaymentStatus Status { get; set; }
        public List<TicketResponse> Tickets { get; set; }
        public List<ReminderType> Reminders { get; set; }
    }
}
