using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.Auth
{
    public class LogoutRequest
    {
        [Required]
        public string FirebaseMessagingToken { get; set; }
    }
}
