using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.Auth
{
    public class RefreshTokenRequest
    {
        [Required]
        public string AccessToken { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}
