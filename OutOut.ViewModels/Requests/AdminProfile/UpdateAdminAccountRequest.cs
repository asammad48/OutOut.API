using Microsoft.AspNetCore.Http;
using OutOut.ViewModels.Validators;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.AdminProfile
{
    public class UpdateAdminAccountRequest
    {
        [MinLength(2)]
        [MaxLength(100)]
        public string FullName { get; set; }

        [ImageFile]
        public IFormFile ProfileImage { get; set; }

        [ValidEmailAddress]
        [Required]
        public string Email { get; set; }

        [ValidPhoneNumber(AllowTollFree: true)]
        public string PhoneNumber { get; set; }

        [MaxLength(100)]
        public string CompanyName { get; set; }

    }
}
