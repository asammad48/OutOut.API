using OutOut.Models.EntityInterfaces;
using OutOut.Persistence.Data;
using MongoDB.Driver;

namespace OutOut.Persistence.Services.Basic
{
    public abstract class GenericSyncRepository<TEntity> where TEntity : INonSqlEntity
    {
        protected readonly ApplicationNonSqlDbContext _dbContext;
        protected IMongoCollection<TEntity> _collection
        {
            get { return _dbContext.GetCollection<TEntity>(); }
        }

        public GenericSyncRepository(ApplicationNonSqlDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
