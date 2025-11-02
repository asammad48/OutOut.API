using OutOut.ViewModels.Validators;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.Auth
{
    public class CustomerRegistrationRequest
    {
        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string FullName { get; set; }

        [ValidEmailAddress]
        [Required]
        public string Email { get; set; }

        [ValidPassword]
        [Required]
        public string Password { get; set; }

        [ValidPhoneNumber]
        public string PhoneNumber { get; set; }
    }
}
