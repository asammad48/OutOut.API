using OutOut.Constants.Enums;
using OutOut.Models.Models;
using OutOut.ViewModels.Responses.LoyaltyTypes;

namespace OutOut.ViewModels.Responses.Venues
{
    public class VenueLoyaltySummaryResponse
    {
        public string Id { get; set; }
        public LoyaltyTypeSummaryResponse Type { get; set; }
        public LoyaltyStars Stars { get; set; }
        public bool IsActive { get; set; }
        public List<AvailableTimeResponse> ValidOn { get; set; }
        public MaxUsage MaxUsage { get; set; }

        public List<Redemption> Redemptions { get; set; }
        public bool IsApplicable { get; set; }
        public bool CanGet { get; set; }
        public int StarsCount { get; set; }
    }
}
