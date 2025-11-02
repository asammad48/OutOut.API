using MongoDB.Bson;
using OutOut.Models.EntityInterfaces;
using OutOut.Models.Models.Embedded;

namespace OutOut.Models.Models
{
    public class EventRequest : INonSqlEntity
    {
        public EventRequest(Event updatedEvent, Event oldEvent, LastModificationRequest lastModificationRequest)
        {
            Id = ObjectId.GenerateNewId().ToString();
            Event = updatedEvent;
            OldEvent = oldEvent;
            LastModificationRequest = lastModificationRequest;
        }

        public LastModificationRequest LastModificationRequest { get; set; }
        public Event Event { get; set; }
        public Event OldEvent { get; set; }
    }
}
