using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.Users
{
    public class UserLocationRequest
    {
        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
        public string Description { get; set; }
    }
   
}
