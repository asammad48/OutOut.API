using MongoDB.Bson;
using OutOut.Constants.Enums;
using OutOut.Models.EntityInterfaces;
using OutOut.Models.Models.Embedded;

namespace OutOut.Models.Models
{
    public class VenueBooking : INonSqlEntity
    {
        public VenueBooking()
        {
            Id = ObjectId.GenerateNewId().ToString();
            Status = VenueBookingStatus.Pending;
            Reminders = new List<ReminderType>();
        }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string ModifiedBy { get; set; }

        public int BookingNumber { get; set; }
        public int PeopleNumber { get; set; }      
        public DateTime Date { get; set; }
        public VenueBookingStatus Status { get; set; }
        public List<ReminderType> Reminders { get; set; }

        public VenueSummary Venue { get; set; }
        public ApplicationUserSummary User { get; set; }
    }
}
