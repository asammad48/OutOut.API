using MongoDB.Bson;
using OutOut.Models.EntityInterfaces;
using OutOut.Models.Models.Embedded;

namespace OutOut.Models.Models
{
    public class UserLoyalty : INonSqlEntity
    {
        public UserLoyalty()
        {
        }
        public UserLoyalty(string userId, Loyalty loyalty, string code, DateTime date)
        {
            Id = ObjectId.GenerateNewId().ToString();
            UserId = userId;
            Loyalty = loyalty;
            Redemptions = new List<Redemption>() { new Redemption(code, date) };
        }

        public DateTime LastModifiedDate { get; set; }
        public string UserId { get; set; }
        public Loyalty Loyalty { get; set; }
        public VenueLoyaltySummary Venue { get; set; }
        public List<Redemption> Redemptions { get; set; }
        public bool IsConsumed { get; set; }
        public bool CanGet { get; set; }
    }
    public class Redemption
    {
        public Redemption(string code, DateTime date)
        {
            Date = date;
            Code = code;
        }
        public DateTime Date { get; set; }
        public string Code { get; set; }
    }
}
