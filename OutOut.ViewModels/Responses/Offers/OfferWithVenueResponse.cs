using OutOut.ViewModels.Responses.OfferTypes;
using OutOut.ViewModels.Responses.Venues;
using System;
using System.Collections.Generic;

namespace OutOut.ViewModels.Responses.Offers
{
    public class OfferWithVenueResponse
    {
        public string Id { get; set; }
        public string Image { get; set; }
        public OfferTypeSummaryResponse Type { get; set; }
        public bool IsActive { get; set; }
        public VenueSummaryResponse Venue { get; set; }
        public DateTime NextAvailableDate { get; set; }
    }
}
