using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.Auth
{
    public class ForgetPasswordRequest
    {
        [Required(AllowEmptyStrings = false)]
        public string Email { get; set; }
    }
}
