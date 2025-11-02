using OutOut.ViewModels.Validators;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.Venues
{
    public class LocationRequest
    {
        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
        [Required]
        [MongoId]
        public string CityId { get; set; }
        [Required]
        public string Area { get; set; }
        [Required(ErrorMessage = "The Location field is required")]
        [MaxLength(100)]
        public string Description { get; set; }
    }
}
