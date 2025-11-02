using OutOut.ViewModels.Responses.LoyaltyTypes;
using OutOut.ViewModels.Responses.Venues;

namespace OutOut.ViewModels.Responses.Customers
{
    public class CustomerLoyaltyResponse
    {
        public LoyaltyTypeSummaryResponse Type { get; set; }
        public VenueMiniSummaryResponse Venue { get; set; }
        public long RedemptionsCount { get; set; }
    }
}
