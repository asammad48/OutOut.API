using OutOut.ViewModels.Validators;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.Auth
{
    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public string HashedOTP { get; set; }

        [ValidPassword]
        [Required]
        public string NewPassword { get; set; }
    }
}
