using OutOut.Constants.Enums;
using OutOut.ViewModels.Responses.Venues;

namespace OutOut.ViewModels.Responses.EventBookings
{
    public class TicketDetails
    {
        public int OrderNumber { get; set; }
        public string EventName { get; set; }
        public DateTime ReservationDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string TicketOwnerUserName { get; set; }
        public string TicketOwnerPhoneNumber { get; set; }
        public string TicketOwnerEmail { get; set; }
        public LocationResponse BookingLocation { get; set; }
        public Gender Gender { get; set; }
        public Gender? TicketOwnerGender { get; set; }
        public DateTime? RedemptionDate { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public string Status { get; set; }
        public string RejectionReason { get; set; }
        public int RedeemedTicketsCount { get; set; }
        public int TotalTicketsCount { get; set; }
        public bool IsTicketShared { get; set; }
    }
}
