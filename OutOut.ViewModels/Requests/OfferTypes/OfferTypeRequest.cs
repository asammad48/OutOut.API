using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.OfferTypes
{
    public class OfferTypeRequest
    {
        [MaxLength(200)]
        [Required]
        public string Name { get; set; }
    }
}
