using MongoDB.Bson.Serialization.Attributes;
using OutOut.Constants.Enums;
using OutOut.Models.EntityInterfaces;
using OutOut.Models.Models;

namespace OutOut.Models.Domain
{
    [BsonIgnoreExtraElements]
    public class VenueWithDistance : INonSqlEntity
    {
        public string Name { get; set; }
        public string Logo { get; set; }
        public string DetailsLogo { get; set; }
        public string TableLogo { get; set; }
        public List<AvailableTime> OpenTimes { get; set; }
        public Location Location { get; set; }
        public double Distance { get; set; }
        public List<Category> Categories { get; set; }
        public List<Offer> Offers { get; set; }
        public Availability Status { get; set; }
        public bool IsFavorite { get; set; }
        public string PhoneNumber { get; set; }
    }
}
