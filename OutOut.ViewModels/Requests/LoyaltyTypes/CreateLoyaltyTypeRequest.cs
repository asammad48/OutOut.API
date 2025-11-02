using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.LoyaltyTypes
{
    public class CreateLoyaltyTypeRequest
    {
        [MaxLength(200)]
        [Required]
        public string Name { get; set; }
    }
}
