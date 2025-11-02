using MongoDB.Bson.Serialization.Attributes;
using OutOut.Models.EntityInterfaces;
using OutOut.Models.Models;
using OutOut.Models.Models.Embedded;

namespace OutOut.Models.Domain
{
    [BsonIgnoreExtraElements]
    public class VenueAllOffersWithDistance : INonSqlEntity
    {
        public string Name { get; set; }
        public string Logo { get; set; }
        public Location Location { get; set; }
        public List<AvailableTime> OpenTimes { get; set; }
        public List<Offer> Offers { get; set; }
        public VenueSummary Venue { get; set; }
        public List<Category> Categories { get; set; }
        public double Distance { get; set; }
    }
}
