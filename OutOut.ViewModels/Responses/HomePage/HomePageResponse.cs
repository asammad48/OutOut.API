using OutOut.ViewModels.Responses.Events;
using OutOut.ViewModels.Responses.Offers;
using OutOut.ViewModels.Responses.Venues;
using System.Collections.Generic;

namespace OutOut.ViewModels.Responses.HomePage
{
    public class HomePageResponse
    {
        public List<VenueSummaryResponse> Venues { get; set; }
        public List<EventSummaryResponse> Events { get; set; }
        public List<OfferWithVenueResponse> Offers { get; set; }
    }
}
