using OutOut.Constants.Enums;
using OutOut.ViewModels.Responses.Events;
using OutOut.ViewModels.Responses.Venues;
using System.Collections.Generic;

namespace OutOut.ViewModels.Responses.EventBookings
{
    public class SingleEventBookingTicketSummaryResponse
    {
        public string Id { get; set; }
        public EventSummaryResponse Event { get; set; }
        public VenueMiniSummaryResponse Venue { get; set; }
        public TicketResponse Ticket { get; set; }
        public PaymentStatus Status { get; set; }
        public List<ReminderType> Reminders { get; set; }
    }
}
