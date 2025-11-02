using OutOut.Persistence.Services.Basic;
using MongoDB.Driver;
using OutOut.Models.Models;
using OutOut.Persistence.Data;
using OutOut.Persistence.Interfaces.Basic;

namespace OutOut.Persistence.SyncServices
{
    public class CityVenueSyncRepository : GenericSyncRepository<Venue>, ISyncRepository<City>
    {
        public CityVenueSyncRepository(ApplicationNonSqlDbContext dbContext) : base(dbContext) { }

        public Task Sync(City oldOtherEntity, City otherEntity)
        {
            if (oldOtherEntity?.Name != otherEntity.Name || oldOtherEntity?.IsActive != otherEntity.IsActive)
            {
                var venueCityIdFilter = Builders<Venue>.Filter.Eq(v => v.Location.City.Id, otherEntity.Id);
                var updateNameDef = Builders<Venue>.Update.Set(v => v.Location.City.Name, otherEntity.Name);
                var updateStatusDef = Builders<Venue>.Update.Set(v => v.Location.City.IsActive, otherEntity.IsActive);

                var updates = new List<UpdateDefinition<Venue>> { updateNameDef, updateStatusDef };
                var updatesBuilder = Builders<Venue>.Update.Combine(updates);

                return _collection.UpdateManyAsync(venueCityIdFilter, updatesBuilder);
            }

            return Task.CompletedTask;
        }
    }
}
