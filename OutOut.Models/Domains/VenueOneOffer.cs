using MongoDB.Bson.Serialization.Attributes;
using OutOut.Constants.Enums;
using OutOut.Models.EntityInterfaces;
using OutOut.Models.Models;
using OutOut.Models.Models.Embedded;

namespace OutOut.Models.Domain
{
    [BsonIgnoreExtraElements]
    public class VenueOneOffer : INonSqlEntity
    {
        public string Name { get; set; }
        public string Logo { get; set; }
        public string DetailsLogo { get; set; }
        public string TableLogo { get; set; }
        public Location Location { get; set; }
        public List<AvailableTime> OpenTimes { get; set; }
        public List<Category> Categories { get; set; }
        public string OffersCode { get; set; }
        
        [BsonElement("Offers")] // Unwind maps using the same name in document which is "Offers"
        public Offer Offer { get; set; }
        public VenueSummary Venue { get; set; }
        public Availability Status { get; set; }
    }
}
