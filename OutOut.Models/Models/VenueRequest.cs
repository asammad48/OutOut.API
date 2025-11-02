using MongoDB.Bson;
using OutOut.Models.EntityInterfaces;
using OutOut.Models.Models.Embedded;

namespace OutOut.Models.Models
{
    public class VenueRequest : INonSqlEntity
    {
        public VenueRequest(Venue updatedVenue, Venue oldVenue, LastModificationRequest lastModificationRequest)
        {
            Id = ObjectId.GenerateNewId().ToString();
            Venue = updatedVenue;
            OldVenue = oldVenue;
            LastModificationRequest = lastModificationRequest;
        }
        public LastModificationRequest LastModificationRequest { get; set; }
        public Venue Venue { get; set; }
        public Venue OldVenue { get; set; }
    }
}
