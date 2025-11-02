using OutOut.Constants.Enums;

namespace OutOut.ViewModels.Requests.Loyalties
{
    public class LoyaltyFilterationRequest
    {
        public string SearchQuery { get; set; }
        public LoyaltyTimeFilter TimeFilter { get; set; }
    }
    public enum LoyaltyTimeFilter
    {
        Recent, History
    }
}
