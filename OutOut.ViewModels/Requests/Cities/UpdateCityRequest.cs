using OutOut.ViewModels.Validators;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.Cities
{
    public class UpdateCityRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        [Required]
        [MongoId]
        public string CountryId { get; set; }
        
        [Required]
        public bool IsActive { get; set; }
    }
}
