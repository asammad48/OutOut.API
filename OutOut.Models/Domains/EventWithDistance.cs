using MongoDB.Bson.Serialization.Attributes;
using OutOut.Models.EntityInterfaces;
using OutOut.Models.Models;

namespace OutOut.Models.Domains
{
    [BsonIgnoreExtraElements]
    public class EventWithDistance : INonSqlEntity
    {
        public string Name { get; set; }
        public string Logo { get; set; }
        public Location Location { get; set; }
        public bool IsFeatured { get; set; }
        public List<EventOccurrence> Occurrences { get; set; }
        public double Distance { get; set; }
    }
}
