using Microsoft.AspNetCore.Http;
using OutOut.ViewModels.Validators;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.AdminProfile
{
    public class UpdateAdminProfileRequest
    {
        [MinLength(2)]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        public string Role { get; set; }

        [ImageFile]
        public IFormFile ProfileImage { get; set; }

        [ValidEmailAddress]
        [Required]
        public string Email { get; set; }

        [ValidPhoneNumber(AllowTollFree: true)]
        public string PhoneNumber { get; set; }

        [MaxLength(100)]
        public string CompanyName { get; set; }

        [ValidPassword]
        public string Password { get; set; }

        public List<string> AccessibleVenues { get; set; }

        public List<string> AccessibleEvents { get; set; }

    }
}
