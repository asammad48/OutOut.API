using OutOut.Constants.Enums;
using OutOut.ViewModels.Responses.LoyaltyTypes;
using OutOut.ViewModels.Responses.Venues;
using System.Collections.Generic;

namespace OutOut.ViewModels.Responses.Loyalties
{
    public class AssignedLoyaltyResponse
    {
        public string Id { get; set; }
        public LoyaltyTypeSummaryResponse Type { get; set; }
        public LoyaltyStars Stars { get; set; }
        public bool IsActive { get; set; }
        public MaxUsage MaxUsage { get; set; }
        public List<AvailableTimeResponse> ValidOn { get; set; }
        public VenueSummaryResponse Venue { get; set; }
    }
}
