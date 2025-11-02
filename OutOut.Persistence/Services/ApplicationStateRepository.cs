using MongoDB.Driver;
using OutOut.Models.Models;
using OutOut.Persistence.Data;
using OutOut.Persistence.Interfaces;

namespace OutOut.Persistence.Services
{
    public class ApplicationStateRepository : IApplicationStateRepository
    {
        protected readonly ApplicationNonSqlDbContext _dbContext;
        protected IMongoCollection<ApplicationState> _collection
        {
            get { return _dbContext.GetCollection<ApplicationState>(); }
        }
        public ApplicationStateRepository(ApplicationNonSqlDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<string> GetByKey(string key)
        {
            var result = await _collection.Find(entity => entity.Key == key).FirstOrDefaultAsync();
            return result?.Value ?? null;
        }
        public async Task SetByKey(string key, string value)
        {
            var state = new ApplicationState { Key = key, Value = value };
            var existingValue = await GetByKey(key);
            if (existingValue != null)
            {
                await _collection.DeleteOneAsync(entity => entity.Key == key);
            }
            await _collection.InsertOneAsync(state);
        }
    }
}
