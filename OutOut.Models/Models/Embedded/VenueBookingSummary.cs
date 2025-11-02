using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using OutOut.Models.EntityInterfaces;

namespace OutOut.Models.Models.Embedded
{
    public class VenueBookingSummary : INonSqlEntity
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string VenueId { get; set; }
        public string VenueName { get; set; }
        public string VenueLogo { get; set; }
    }
}
