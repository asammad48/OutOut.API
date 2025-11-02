using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using OutOut.Constants.Enums;
using OutOut.Models.EntityInterfaces;
using OutOut.Models.Models.Embedded;

namespace OutOut.Models.Models
{
    public class Notification : INonSqlEntity
    {
        public Notification() { }
        public Notification(NotificationType notificationType, string userId, string title, string body, string image, NotificationAction action, VenueBookingSummary venueBooking = null, EventBookingSummary eventBooking = null,
            VenueSummary venue = null, EventSummary eventSummary = null, VenueOfferSummary offer = null, string affectedId = null, bool isRead = false)
        {
            Id = ObjectId.GenerateNewId().ToString();
            CreatedDate = DateTime.UtcNow;
            UserId = userId;
            NotificationType = notificationType;
            Title = title;
            Body = body;
            Image = image;
            Action = action;
            VenueBooking = venueBooking;
            EventBooking = eventBooking;
            Venue = venue;
            Event = eventSummary;
            Offer = offer;
            SentDate = null;
            ToBeSentDate = null;
            AffectedId = affectedId;
            IsRead = isRead;
            Log = new List<Log>();

        }


        public Notification(NotificationType notificationType, NotificationAction action, string userId, string title, string body, string image,
                     string affectedId = null, bool isRead = false)
        {
            Id = ObjectId.GenerateNewId().ToString();
            CreatedDate = DateTime.UtcNow;
            UserId = userId;
            NotificationType = notificationType;
            Title = title;
            Body = body;
            Image = image;
            Action = action;
            SentDate = null;
            ToBeSentDate = null;
            AffectedId = affectedId;
            IsRead = isRead;
            Log = new List<Log>();

        }

        public Notification(string userId, NotificationAction action, string affectedId, string body, bool isRead = false)
        {
            Id = ObjectId.GenerateNewId().ToString();
            CreatedDate = DateTime.UtcNow;
            UserId = userId;
            Body = body;
            Action = action;
            AffectedId = affectedId;
            NotificationType = NotificationType.Notification;
            SentDate = DateTime.UtcNow;
            NotificationStatus = NotificationStatus.Sent;
            ToBeSentDate = null;
            IsRead = isRead;
            Log = new List<Log>();
        }

        public DateTime CreatedDate { get; set; }
        public NotificationAction Action { get; set; }
        [BsonIgnoreIfNull]
        public string AffectedId { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Image { get; set; }
        public DateTime? SentDate { get; set; }
        [BsonIgnoreIfNull]
        public bool IsRead { get; set; }
        public NotificationStatus NotificationStatus { get; set; }
        public NotificationType NotificationType { get; set; }
        [BsonIgnoreIfNull]
        public DateTime? ToBeSentDate { get; set; }
        public List<Log> Log { get; set; }

        [BsonIgnoreIfNull]
        public VenueBookingSummary VenueBooking { get; set; }

        [BsonIgnoreIfNull]
        public EventBookingSummary EventBooking { get; set; }

        [BsonIgnoreIfNull]
        public VenueSummary Venue { get; set; }

        [BsonIgnoreIfNull]
        public EventSummary Event { get; set; }

        [BsonIgnoreIfNull]
        public VenueOfferSummary Offer { get; set; }
    }
    public class Log
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
