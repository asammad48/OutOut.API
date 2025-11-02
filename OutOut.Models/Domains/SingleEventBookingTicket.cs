using MongoDB.Bson.Serialization.Attributes;
using OutOut.Constants.Enums;
using OutOut.Models.EntityInterfaces;
using OutOut.Models.Models;
using OutOut.Models.Models.Embedded;

namespace OutOut.Models.Domains
{
    [BsonIgnoreExtraElements]
    public class SingleEventBookingTicket : INonSqlEntity
    {
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
        public bool IsTerminated { get; set; }
        [BsonElement("Tickets")]
        public Ticket Ticket { get; set; }
        public List<ReminderType> Reminders { get; set; }
    }
}
