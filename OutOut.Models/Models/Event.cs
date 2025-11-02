using MongoDB.Bson.Serialization.Attributes;
using OutOut.Constants.Enums;
using OutOut.Models.EntityInterfaces;
using OutOut.Models.Models.Embedded;

namespace OutOut.Models.Models
{
    [BsonIgnoreExtraElements]
    public class Event : INonSqlEntity
    {
        public Event()
        {
            Categories = new List<Category>();
            Occurrences = new List<EventOccurrence>();
            CreationDate = DateTime.UtcNow;
        }
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
        public string WebpageLink { get; set; }
        public bool IsFeatured { get; set; }
        public List<EventOccurrence> Occurrences { get; set; }
        public VenueSummary Venue { get; set; }
        public Availability Status { get; set; }
        public long TicketsNumber { get; set; }
    }
}
