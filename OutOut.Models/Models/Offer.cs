using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using OutOut.Constants.Enums;
using OutOut.Models.EntityInterfaces;

namespace OutOut.Models.Models
{
    public class Offer : INonSqlEntity
    {
        public Offer()
        {
            Id = ObjectId.GenerateNewId().ToString();
            AssignDate = DateTime.UtcNow;
        }
        public DateTime AssignDate { get; set; }
        public string Image { get; set; }
        public OfferType Type { get; set; }
        public bool IsActive { get; set; }
        [BsonDateTimeOptions(DateOnly = true)]
        public DateTime ExpiryDate { get; set; }
        public List<AvailableTime> ValidOn { get; set; }
        public OfferUsagePerYear MaxUsagePerYear { get; set; }
    }
}
