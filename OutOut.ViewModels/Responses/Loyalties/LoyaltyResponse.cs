using OutOut.Constants.Enums;
using OutOut.Models.Models;
using OutOut.ViewModels.Responses.LoyaltyTypes;
using OutOut.ViewModels.Responses.Venues;

namespace OutOut.ViewModels.Responses.Loyalties
{
    public class LoyaltyResponse
    {
        public string Id { get; set; }
        public LoyaltyTypeSummaryResponse Type { get; set; }
        public LoyaltyStars Stars { get; set; }
        public bool IsActive { get; set; }
        public List<AvailableTimeResponse> ValidOn { get; set; }
        public MaxUsage MaxUsage { get; set; }

        public List<Redemption> Redemptions { get; set; }
        public bool IsApplicable { get; set; } //loyalty will be dimmed if it's false
        public bool CanGet { get; set; } //it will be true if user completed stars, and Get button will be shown instead of Redeem button
        public int StarsCount { get; set; }

        public VenueMiniSummaryResponse Venue { get; set; }
    }
}
