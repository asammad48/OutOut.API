using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OutOut.Models;
using OutOut.Models.Domains;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OutOut.Infrastructure.Services
{
    public class AppleAuthenticator
    {
        private readonly AppSettings _appSettings;
        public AppleAuthenticator(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public async Task<ExternalUserInfo> GetAccessTokenInfo(string accessToken)
        {
            // Get the public keys 
            var client = new HttpClient();
            string keysJson = await client.GetStringAsync("https://appleid.apple.com/auth/keys");

            // Parse the keys
            JsonWebKeySet keySet = JsonWebKeySet.Create(keysJson);

            // Setup the validation parameters
            var parameters = new TokenValidationParameters()
            {
                ValidAudience = _appSettings.AppSecrets.AppleClientId,
                ValidIssuer = "https://appleid.apple.com",
                IssuerSigningKeys = keySet.Keys,
                ValidateLifetime = true,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var result = tokenHandler.ValidateToken(accessToken, parameters, out var _);

            return new ExternalUserInfo { Email = result.Claims.FirstOrDefault(a => a.Type == ClaimTypes.Email)?.Value };
        }
    }
}
