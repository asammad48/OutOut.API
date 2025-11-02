using OutOut.ViewModels.Validators;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.Cities
{
    public class CreateCityRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        [StringMaxLength(50)]
        public List<string> Areas { get; set; }
        
        [Required]
        [MongoId]
        public string CountryId { get; set; }
        
        [Required]
        public bool IsActive { get; set; }
    }
}
