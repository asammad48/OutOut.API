using MongoDB.Bson.Serialization.Attributes;
using OutOut.Constants.Enums;
using OutOut.Models.EntityInterfaces;
using OutOut.Models.Models;
using OutOut.Models.Models.Embedded;

namespace OutOut.Models.Domains
{
    [BsonIgnoreExtraElements]
    public class SingleEventOccurrence : INonSqlEntity
    {
        public string CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string HeaderImage { get; set; }
        public string DetailsLogo { get; set; }
        public string TableLogo { get; set; }
        public Location Location { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public List<Category> Categories { get; set; }
        public string Code { get; set; }
        public string FacebookLink { get; set; }
        public string InstagramLink { get; set; }
        public string YoutubeLink { get; set; }
        public long TicketsNumber { get; set; }
        public bool IsFeatured { get; set; }
        [BsonElement("Occurrences")]
        public EventOccurrence Occurrence { get; set; }
        public double Distance { get; set; }
        public VenueSummary Venue { get; set; }
        public Availability Status { get; set; }
    }
}
