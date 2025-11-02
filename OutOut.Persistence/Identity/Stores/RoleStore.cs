using OutOut.Persistence.Data;
using OutOut.Models.Identity;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using OutOut.Persistence.Extensions;

namespace OutOut.Persistence.Identity.Stores
{
    public class RoleStore<TRole> : IQueryableRoleStore<TRole> where TRole : ApplicationRole
    {
        private readonly ApplicationNonSqlDbContext _nonSqlContext;

        public RoleStore(ApplicationNonSqlDbContext nonSqlContext)
        {
            _nonSqlContext = nonSqlContext;
        }

        IQueryable<TRole> IQueryableRoleStore<TRole>.Roles
        {
            get
            {
                var query = _nonSqlContext.GetCollection<TRole>().AsQueryable();
                return query;
            }
        }

        public async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken)
        {
            var collection = _nonSqlContext.GetCollection<TRole>();
            var found = await collection.FirstOrDefaultAsync(x => x.NormalizedName == role.NormalizedName);
            if (found == null) await collection.InsertOneAsync(role, new InsertOneOptions(), cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken)
        {
            await _nonSqlContext.GetCollection<TRole>().ReplaceOneAsync(x => x.Id == role.Id, role, cancellationToken: cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken)
        {
            await _nonSqlContext.GetCollection<TRole>().DeleteOneAsync(x => x.Id == role.Id, cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
        {
            return await Task.FromResult(role.Id);
        }

        public async Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            return (await _nonSqlContext.GetCollection<TRole>().FirstOrDefaultAsync(x => x.Id == role.Id))?.Name ?? role.Name;
        }

        public async Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            await _nonSqlContext.GetCollection<TRole>().UpdateOneAsync(x => x.Id == role.Id, Builders<TRole>.Update.Set(x => x.Name, roleName), cancellationToken: cancellationToken);
        }

        public async Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            return await Task.FromResult(role.NormalizedName);
        }

        public async Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken)
        {
            role.NormalizedName = normalizedName;
            await _nonSqlContext.GetCollection<TRole>().UpdateOneAsync(x => x.Id == role.Id, Builders<TRole>.Update.Set(x => x.NormalizedName, normalizedName), cancellationToken: cancellationToken);
        }

        public Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return _nonSqlContext.GetCollection<TRole>().FirstOrDefaultAsync(x => x.Id == roleId);
        }

        public Task<TRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return _nonSqlContext.GetCollection<TRole>().FirstOrDefaultAsync(x => x.NormalizedName == normalizedRoleName);
        }

        void IDisposable.Dispose()
        {
        }
    }
}
