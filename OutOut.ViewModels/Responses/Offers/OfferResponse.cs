using OutOut.Constants.Enums;
using OutOut.ViewModels.Responses.OfferTypes;
using OutOut.ViewModels.Responses.Venues;
using System;
using System.Collections.Generic;

namespace OutOut.ViewModels.Responses.Offers
{
    public class OfferResponse
    {
        public string Id { get; set; }
        public string Image { get; set; }
        public OfferTypeSummaryResponse Type { get; set; }
        public bool IsActive { get; set; }
        public DateTime ExpiryDate { get; set; }
        public List<AvailableTimeResponse> ValidOn { get; set; }
        public OfferUsagePerYear MaxUsagePerYear { get; set; }
        public bool IsApplicable { get; set; }
        public DateTime NextAvailableDate { get; set; }
    }
}
