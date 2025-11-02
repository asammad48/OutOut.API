using MongoDB.Bson.Serialization.Attributes;
using OutOut.Models.EntityInterfaces;
using OutOut.Models.Models;
using OutOut.Models.Models.Embedded;

namespace OutOut.Models.Domains
{
    [BsonIgnoreExtraElements]
    public class UnwindVenueRequestOffer : INonSqlEntity
    {
        public LastModificationRequest LastModificationRequest { get; set; }
        public SingleVenueOffer Venue { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class SingleVenueOffer : INonSqlEntity
    {
        public string Name { get; set; }
        public string Logo { get; set; }
        public string DetailsLogo { get; set; }
        public string TableLogo { get; set; }
        public List<AvailableTime> OpenTimes { get; set; }
        public Location Location { get; set; }
        public List<Category> Categories { get; set; }
        public string PhoneNumber { get; set; }

        [BsonElement("Offers")]
        public Offer Offer { get; set; }
    }
}
