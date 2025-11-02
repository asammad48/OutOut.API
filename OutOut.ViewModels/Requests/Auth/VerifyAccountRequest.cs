namespace OutOut.ViewModels.Requests.Auth
{
    public class VerifyAccountRequest
    {
        public string Email { get; set; }
        public string OTP { get; set; }
        public string FirebaseMessagingToken { get; set; }
    }
}
