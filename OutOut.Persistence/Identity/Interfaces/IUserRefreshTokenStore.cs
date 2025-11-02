using OutOut.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace OutOut.Persistence.Identity.Interfaces
{
    public interface IUserRefreshTokenStore<TUser> : IUserStore<TUser>, IDisposable where TUser : class
    {
        Task<List<IdentityRefreshToken>> GetRefreshTokensAsync(string userId, CancellationToken cancellationToken);
        Task RemoveRefreshTokenAsync(TUser user, string Token, CancellationToken cancellationToken);
        Task RemoveAllExpiredRefreshTokensAsync(TUser user, CancellationToken cancellationToken);
        Task AddRefreshTokenAsync(TUser user, string Token, DateTime ExpirationDate, string AccessTokenUniqeId, CancellationToken cancellationToken);
    }
}
