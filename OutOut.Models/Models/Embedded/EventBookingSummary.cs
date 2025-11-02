using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using OutOut.Models.EntityInterfaces;

namespace OutOut.Models.Models.Embedded
{
    public class EventBookingSummary : INonSqlEntity
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string EventId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string EventOccurrenceId { get; set; }
        public string EventName { get; set; }
        public string EventImage { get; set; }
    }
}
