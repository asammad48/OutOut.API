using OutOut.ViewModels.Responses.OfferTypes;
using OutOut.ViewModels.Responses.Venues;

namespace OutOut.ViewModels.Responses.Customers
{
    public class CustomerOfferResponse
    {
        public OfferTypeSummaryResponse Type { get; set; }
        public VenueMiniSummaryResponse Venue { get; set; }
        public long RedemptionsCount { get; set; }
    }
}
