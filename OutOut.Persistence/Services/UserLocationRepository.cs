using MongoDB.Driver;
using OutOut.Models.Identity;
using OutOut.Models.Models;
using OutOut.Persistence.Data;
using OutOut.Persistence.Interfaces;

namespace OutOut.Persistence.Services
{
    public class UserLocationRepository : IUserLocationRepository
    {
        private readonly ApplicationNonSqlDbContext _dbContext;
        private readonly IMongoCollection<ApplicationUser> _collection;
        public UserLocationRepository(ApplicationNonSqlDbContext dbContext)
        {
            _dbContext = dbContext;
            _collection = _dbContext.GetCollection<ApplicationUser>();
        }

        public Task<ApplicationUser> UpdateUserLocation(string userId, UserLocation userLocation)
        {
            var updateBuilder = Builders<ApplicationUser>.Update;
            var updateLocation = updateBuilder.Set(a => a.Location, userLocation);

            return _collection.FindOneAndUpdateAsync<ApplicationUser>(a => a.Id == userId, updateLocation, new FindOneAndUpdateOptions<ApplicationUser, ApplicationUser> { ReturnDocument = ReturnDocument.After });
        }

        public Task<ApplicationUser> GetUserLocation(string userId)
        {
            return _collection.Find(entity => entity.Id == userId).FirstOrDefaultAsync();
        }
    }
}
