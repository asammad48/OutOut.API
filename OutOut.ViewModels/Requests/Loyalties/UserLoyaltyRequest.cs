using OutOut.ViewModels.Validators;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.Loyalties
{
    public class UserLoyaltyRequest
    {
        [Required]
        [MongoId]
        public string LoyaltyId { get; set; }
        [Required]
        public string LoyaltyCode { get; set; }
    }
}
