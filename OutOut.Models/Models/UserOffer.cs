using OutOut.Models.EntityInterfaces;
using OutOut.Models.Models.Embedded;
using System;
using System.Collections.Generic;

namespace OutOut.Models.Models
{
    public class UserOffer : INonSqlEntity
    {
        public string UserId { get; set; }
        public Offer Offer { get; set; }
        public VenueSummary Venue { get; set; }
        public DateTime Day { get; set; }
        public bool HasReachedLimit { get; set; }
        public List<UserOfferRedeemLog> Log { get; set; }
    }

    public class UserOfferRedeemLog
    {
        public UserOfferRedeemLog(string offerCode)
        {
            Time = DateTime.UtcNow.TimeOfDay;
            OfferCode = offerCode;
        }
        public TimeSpan Time { get; set; }
        public string OfferCode { get; set; }
    }
}
