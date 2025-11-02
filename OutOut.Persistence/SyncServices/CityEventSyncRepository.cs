using OutOut.Persistence.Services.Basic;
using MongoDB.Driver;
using OutOut.Models.Models;
using OutOut.Persistence.Data;
using OutOut.Persistence.Interfaces.Basic;

namespace OutOut.Persistence.SyncServices
{
    public class CityEventSyncRepository : GenericSyncRepository<Event>, ISyncRepository<City>
    {
        public CityEventSyncRepository(ApplicationNonSqlDbContext dbContext) : base(dbContext) { }

        public Task Sync(City oldOtherEntity, City otherEntity)
        {
            if (oldOtherEntity?.Name != otherEntity.Name || oldOtherEntity?.IsActive != otherEntity.IsActive)
            {
                var eventCityIdFilter = Builders<Event>.Filter.Eq(v => v.Location.City.Id, otherEntity.Id);
                var updateNameDef = Builders<Event>.Update.Set(v => v.Location.City.Name, otherEntity.Name);
                var updateStatusDef = Builders<Event>.Update.Set(v => v.Location.City.IsActive, otherEntity.IsActive);

                var updates = new List<UpdateDefinition<Event>> { updateNameDef, updateStatusDef };
                var updatesBuilder = Builders<Event>.Update.Combine(updates);

                return _collection.UpdateManyAsync(eventCityIdFilter, updatesBuilder);
            }

            return Task.CompletedTask;
        }
    }
}
