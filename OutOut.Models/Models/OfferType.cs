using OutOut.Models.EntityInterfaces;
using System;

namespace OutOut.Models.Models
{
    public class OfferType : INonSqlEntity
    {
        public OfferType()
        {
            CreationDate = DateTime.UtcNow;
        }
        public DateTime CreationDate { get; set; }
        public string Name { get; set; }
    }
}
