using MongoDB.Bson;
using OutOut.Constants.Enums;
using OutOut.Models.Domains;
using OutOut.Models.EntityInterfaces;
using OutOut.Models.Models.Embedded;

namespace OutOut.Models.Models
{
    public class EventBooking : INonSqlEntity
    {
        public EventBooking()
        {
            Id = ObjectId.GenerateNewId().ToString();
            Reminders = new List<ReminderType>();
            Tickets = new List<Ticket>();
        }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string ModifiedBy { get; set; }

        public ApplicationUserSummary User { get; set; }
        public SingleEventOccurrence Event { get; set; }
        public VenueSummary Venue { get; set; }

        public PaymentGateway PaymentGateway { get; set; }
        public int OrderNumber { get; set; }
        public int Quantity { get; set; }
        public EventPackageSummary Package { get; set; }
        public double TotalAmount { get; set; }
        public string Currency { get; set; } = "AED";
        public string Description { get; set; }
        public string OrderReference { get; set; }
        public PaymentStatus Status { get; set; }
        public List<Ticket> Tickets { get; set; }
        public List<ReminderType> Reminders { get; set; }
    }

    public class Ticket : INonSqlEntity
    {
        public Ticket()
        {
            Id = ObjectId.GenerateNewId().ToString();
            RedemptionDate = null;
        }

        public EventPackageSummary Package { get; set; }
        public string Secret { get; set; }
        public DateTime? RedemptionDate { get; set; }
        public string RedeemedBy { get; set; }
        public string RejectedBy { get; set; }
        public string QrRedeemedBy { get; set; }
        public TicketStatus Status { get; set; }
        public string RejectionReason { get; set; }
        public string TicketHolder { get; set; }
    }
}
