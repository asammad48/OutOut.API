using System;

namespace OutOut.Models.Identity
{
    public class IdentityRefreshToken
    {
        public string RefreshToken { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string AccessTokenUniqeId { get; set; }
    }
}
