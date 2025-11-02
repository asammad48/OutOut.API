using OutOut.ViewModels.Validators;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.LoyaltyTypes
{
    public class LoyaltyTypeRequest
    {
        [MongoId]
        [Required]
        public string Id { get; set; }

        [MaxLength(200)]
        [Required]
        public string Name { get; set; }
    }
}
