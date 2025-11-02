using OutOut.Persistence.Services.Basic;
using MongoDB.Driver;
using OutOut.Models.Models;
using OutOut.Persistence.Data;
using OutOut.Persistence.Interfaces.Basic;
using MongoDB.Bson;

namespace OutOut.Persistence.SyncServices
{
    public class VenueOfferTypeSyncRepository : GenericSyncRepository<Venue>, ISyncRepository<OfferType>
    {
        public VenueOfferTypeSyncRepository(ApplicationNonSqlDbContext dbContext) : base(dbContext) { }

        public Task Sync(OfferType oldOtherEntity, OfferType otherEntity)
        {
            if (oldOtherEntity?.Name != otherEntity.Name)
            {
                var filter = Builders<Venue>.Filter.ElemMatch(v => v.Offers, a => a.Type.Id == otherEntity.Id);
                var update = Builders<Venue>.Update.Set("Offers.$[i].Type.Name", otherEntity.Name);
                var arrayFilters = new List<ArrayFilterDefinition> { new BsonDocumentArrayFilterDefinition<Venue>(new BsonDocument("i.Type._id", new BsonDocument("$eq", new BsonObjectId(new ObjectId(otherEntity.Id))))) };
                var options = new UpdateOptions { ArrayFilters = arrayFilters };
                return _collection.UpdateManyAsync(filter, update, options);
            }

            return Task.CompletedTask;
        }
    }
}
