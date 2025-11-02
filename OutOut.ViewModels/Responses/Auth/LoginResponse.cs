using OutOut.ViewModels.Responses.Users;
using System;
using System.Collections.Generic;

namespace OutOut.ViewModels.Responses.Auth
{
    public class LoginResponse
    {
        public ApplicationUserResponse User { get; set; }
        public List<string> UserRoles { get; set; }
        public bool IsVerifiedEmail { get; set; }

        public string Token { get; set; }

        public DateTime Expiration { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
    }
}
