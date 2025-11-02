using OutOut.Constants.Enums;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.Auth
{
    public class ExternalAuthenticationRequest
    {
        [Required]
        public ExternalProvider ExternalLoginProvider { get; set; }

        [Required]
        public string AccessToken { get; set; }

        public string FirebaseMessagingToken { get; set; }

        public string FullName { get; set; }

    }
}
