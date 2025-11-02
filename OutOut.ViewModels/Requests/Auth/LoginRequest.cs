using OutOut.ViewModels.Validators;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.Auth
{
    public class LoginRequest
    {
        [Required]
        [ValidEmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]
        [MaxLength(25)]
        public string Password { get; set; }

        public string FirebaseMessagingToken { get; set; }
    }
}
