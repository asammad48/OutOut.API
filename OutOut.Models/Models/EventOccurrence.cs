using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using OutOut.Models.EntityInterfaces;

namespace OutOut.Models.Models
{
    public class EventOccurrence : INonSqlEntity
    {
        public EventOccurrence()
        {
            Id = ObjectId.GenerateNewId().ToString();
            Packages = new List<EventPackage>();
        }

        [BsonDateTimeOptions(DateOnly = true)]
        public DateTime StartDate { get; set; }
        [BsonDateTimeOptions(DateOnly = true)]
        public DateTime EndDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public List<EventPackage> Packages { get; set; }

        public DateTime GetStartDateTime() => StartDate.Add(StartTime);
    }
}
