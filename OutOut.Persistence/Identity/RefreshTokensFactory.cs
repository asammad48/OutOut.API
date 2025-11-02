using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OutOut.Models;
using OutOut.Models.Identity;
using OutOut.Persistence.Identity.Interfaces;
using System.Security.Cryptography;

namespace OutOut.Persistence.Identity
{
    public class RefreshTokensFactory<TUser> where TUser : class
    {
        private readonly IUserRefreshTokenStore<TUser> _store;
        private readonly AppSettings _appSettings;
        public RefreshTokensFactory(IUserStore<TUser> store, IOptions<AppSettings> appSettings)
        {
            _store = store as IUserRefreshTokenStore<TUser>;
            _appSettings = appSettings.Value;
        }

        private string GenerateToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public async Task<IdentityRefreshToken> GenerateRefreshToken(TUser user, string AccessTokenUniqID)
        {
            CancellationToken cancelation = new CancellationToken();
            var tokenObj = new IdentityRefreshToken
            {
                RefreshToken = GenerateToken(),
                AccessTokenUniqeId = AccessTokenUniqID,
                ExpirationDate = DateTime.UtcNow.AddDays(_appSettings.JWTRefreshTokenDuration.Days)
                                                .AddHours(_appSettings.JWTRefreshTokenDuration.Hours)
                                                .AddMinutes(_appSettings.JWTRefreshTokenDuration.Minutes)
            };
            await _store.AddRefreshTokenAsync(user, tokenObj.RefreshToken, tokenObj.ExpirationDate, tokenObj.AccessTokenUniqeId, cancelation);
            return tokenObj;
        }

        public async Task RemoveAllExipredTokens(TUser user)
        {
            CancellationToken cancelation = new CancellationToken();
            await _store.RemoveAllExpiredRefreshTokensAsync(user, cancelation);
        }

        public async Task RevokeRefreshToken(TUser user, string token)
        {
            CancellationToken cancelation = new CancellationToken();
            await _store.RemoveRefreshTokenAsync(user, token, cancelation);
        }

    }
}
