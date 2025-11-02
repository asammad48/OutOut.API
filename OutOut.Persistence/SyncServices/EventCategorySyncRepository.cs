using OutOut.Persistence.Services.Basic;
using MongoDB.Driver;
using OutOut.Models.Models;
using OutOut.Persistence.Data;
using OutOut.Persistence.Interfaces.Basic;
using MongoDB.Bson;

namespace OutOut.Persistence.SyncServices
{
    public class EventCategorySyncRepository : GenericSyncRepository<Event>, ISyncRepository<Category>
    {
        public EventCategorySyncRepository(ApplicationNonSqlDbContext dbContext) : base(dbContext) { }

        public Task Sync(Category oldOtherEntity, Category otherEntity)
        {
            if (oldOtherEntity?.Name != otherEntity.Name || oldOtherEntity?.Icon != otherEntity.Icon || oldOtherEntity?.IsActive != otherEntity.IsActive)
            {
                var filter = Builders<Event>.Filter.ElemMatch(v => v.Categories, c => c.Id == otherEntity.Id);
                var update = Builders<Event>.Update.Set("Categories.$[i].Name", otherEntity.Name)
                                                   .Set("Categories.$[i].IsActive", otherEntity.IsActive)
                                                   .Set("Categories.$[i].Icon", otherEntity.Icon);
                var arrayFilters = new List<ArrayFilterDefinition> { new BsonDocumentArrayFilterDefinition<Event>(new BsonDocument("i._id", new BsonDocument("$eq", new BsonObjectId(new ObjectId(otherEntity.Id))))) };
                var options = new UpdateOptions { ArrayFilters = arrayFilters };
                return _collection.UpdateManyAsync(filter, update, options);
            }

            return Task.CompletedTask;
        }
    }
}
