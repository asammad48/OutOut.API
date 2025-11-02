using OutOut.Models.EntityInterfaces;
using System;

namespace OutOut.Models.Models
{
    public class LoyaltyType : INonSqlEntity
    {
        public LoyaltyType()
        {
            CreationDate = DateTime.UtcNow;
        }
        public DateTime CreationDate { get; set; }
        public string Name { get; set; }
    }
}
