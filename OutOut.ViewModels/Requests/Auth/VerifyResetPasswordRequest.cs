namespace OutOut.ViewModels.Requests.Auth
{
    public class VerifyResetPasswordRequest
    {
        public string Email { get; set; }
        public string OTP { get; set; }
    }
}
