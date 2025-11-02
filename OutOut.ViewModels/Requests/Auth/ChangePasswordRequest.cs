using OutOut.ViewModels.Validators;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.Auth
{
    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; }
        
        [ValidPassword]
        [Required]
        public string NewPassword { get; set; }
    }
}
