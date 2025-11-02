using MongoDB.Bson.Serialization.Attributes;
using OutOut.Constants.Enums;
using OutOut.Models.EntityInterfaces;
using OutOut.Models.Models;
using OutOut.Models.Models.Embedded;

namespace OutOut.Models.Domain
{
    [BsonIgnoreExtraElements]
    public class VenueOneOfferWithDistance : INonSqlEntity
    {
        public string Name { get; set; }
        public string Logo { get; set; }
        public Location Location { get; set; }
        public List<AvailableTime> OpenTimes { get; set; }
        public List<Category> Categories { get; set; }

        [BsonElement("Offers")] // Unwind maps using the same name in document which is "Offers"
        public Offer Offer { get; set; }
        public VenueSummary Venue { get; set; }
        public double Distance { get; set; }
        public List<UserOffer> UserOffers { get; set; }
        public Availability Status { get; set; }
    }
}
