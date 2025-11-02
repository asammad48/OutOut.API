using OutOut.Persistence.Services.Basic;
using MongoDB.Driver;
using OutOut.Models.Models;
using OutOut.Persistence.Data;
using OutOut.Persistence.Interfaces.Basic;

namespace OutOut.Persistence.SyncServices
{
    public class VenueLoyaltyTypeSyncRepository : GenericSyncRepository<Venue>, ISyncRepository<LoyaltyType>
    {
        public VenueLoyaltyTypeSyncRepository(ApplicationNonSqlDbContext dbContext) : base(dbContext) { }

        public Task Sync(LoyaltyType oldOtherEntity, LoyaltyType otherEntity)
        {
            if (oldOtherEntity?.Name != otherEntity.Name)
            {
                var venueLoyaltyTypeIdFilter = Builders<Venue>.Filter.Eq(v => v.Loyalty.Type.Id, otherEntity.Id);
                var updateTypeDef = Builders<Venue>.Update.Set(v => v.Loyalty.Type.Name, otherEntity.Name);

                return _collection.UpdateManyAsync(venueLoyaltyTypeIdFilter, updateTypeDef);
            }
            
            return Task.CompletedTask;
        }
    }
}
