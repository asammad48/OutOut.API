using AutoMapper;
using MongoDB.Bson.Serialization.Attributes;
using OutOut.Constants.Enums;
using OutOut.Models.EntityInterfaces;

namespace OutOut.Models.Models
{
    [BsonIgnoreExtraElements]
    public class Venue : INonSqlEntity
    {
        public Venue()
        {
            Categories = new List<Category>();
            Gallery = new List<string>();
            SelectedTermsAndConditions = new List<string>();
            Offers = new List<Offer>();
            Events = new List<string>();
            CreationDate = DateTime.UtcNow;
        }
        public string CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Logo { get; set; }
        public string DetailsLogo { get; set; }
        public string TableLogo { get; set; }
        public string Background { get; set; }
        public Location Location { get; set; }
        [IgnoreMap]
        public List<Category> Categories { get; set; }
        public string LoyaltyCode { get; set; }
        public Loyalty Loyalty { get; set; }
        public string OffersCode { get; set; }
        public List<Offer> Offers { get; set; }
        public List<AvailableTime> OpenTimes { get; set; }
        public string PhoneNumber { get; set; }
        public string Menu { get; set; }
        public List<string> SelectedTermsAndConditions { get; set; }
        public List<string> Gallery { get; set; }
        public string FacebookLink { get; set; }
        public string InstagramLink { get; set; }
        public string YoutubeLink { get; set; }
        public string WebpageLink { get; set; }
        public List<string> Events { get; set; }
        public Availability Status { get; set; }
    }
}
