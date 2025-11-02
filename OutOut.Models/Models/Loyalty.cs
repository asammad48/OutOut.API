using MongoDB.Bson;
using OutOut.Constants.Enums;
using OutOut.Models.EntityInterfaces;

namespace OutOut.Models.Models
{
    public class Loyalty : INonSqlEntity
    {
        public Loyalty()
        {
            Id = ObjectId.GenerateNewId().ToString();
            AssignDate = DateTime.UtcNow;
        }
        public DateTime AssignDate { get; set; }
        public LoyaltyType Type { get; set; }
        public LoyaltyStars Stars { get; set; }
        public bool IsActive { get; set; }
        public List<AvailableTime> ValidOn { get; set; }
        public MaxUsage MaxUsage { get; set; }
    }
}
