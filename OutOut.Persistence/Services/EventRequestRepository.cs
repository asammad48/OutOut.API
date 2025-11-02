using MongoDB.Bson;
using MongoDB.Driver;
using OutOut.Constants.Enums;
using OutOut.Models.Models;
using OutOut.Models.Models.Embedded;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Data;
using OutOut.Persistence.Extensions;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.Persistence.Providers;
using OutOut.Persistence.Services.Basic;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Services
{
    public class EventRequestRepository : GenericNonSqlRepository<EventRequest>, IEventRequestRepository
    {
        private readonly IUserDetailsProvider _userDetailsProvider;
        protected IMongoCollection<Event> _eventCollection
        {
            get { return _dbContext.GetCollection<Event>(); }
        }

        public EventRequestRepository(ApplicationNonSqlDbContext dbContext, IEnumerable<ISyncRepository<EventRequest>> syncRepositories, IUserDetailsProvider userDetailsProvider) : base(dbContext, syncRepositories)
        {
            _userDetailsProvider = userDetailsProvider;
        }

        public async Task<bool> UpsertEventRequest(Event eventObject, Event oldEvent, RequestType type, string modifiedId)
        {
            var filter = Builders<EventRequest>.Filter.Eq(a => a.Event.Id, eventObject.Id) &
                         Builders<EventRequest>.Filter.Eq(a => a.LastModificationRequest.Type, type);
            var update = Builders<EventRequest>.Update.Set(a => a.Event, eventObject)
                                                      .Set(a => a.OldEvent, oldEvent)
                                                      .Set(a => a.LastModificationRequest, new LastModificationRequest { Type = type, ModifiedFieldId = modifiedId, CreatedBy = _userDetailsProvider.UserId })
                                                      .SetOnInsert(a => a.Id, ObjectId.GenerateNewId().ToString());
            var result = await _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
            return result.IsAcknowledged;
        }

        public async Task<Page<EventRequest>> GetAllEventsRequests(PaginationRequest paginationRequest, FilterationRequest filterRequest, string createdBy = null)
        {
            var searchFilter = Builders<EventRequest>.Filter.Empty;
            if (filterRequest != null && !string.IsNullOrEmpty(filterRequest?.SearchQuery))
                searchFilter = Builders<EventRequest>.Filter.SearchContains(a => a.Event.Name, filterRequest?.SearchQuery) |
                               Builders<EventRequest>.Filter.SearchContains(c => c.Event.Location.City.Name, filterRequest.SearchQuery);

            var userFilter = Builders<EventRequest>.Filter.Empty;
            if (!string.IsNullOrEmpty(createdBy))
                userFilter = Builders<EventRequest>.Filter.Eq(a => a.LastModificationRequest.CreatedBy, createdBy);

            var records = await _collection.Aggregate(new AggregateOptions { Collation = new Collation(locale: "en", strength: CollationStrength.Secondary), AllowDiskUse = true })
                                  .Match(searchFilter & userFilter)
                                  .ToListAsync();

            return filterRequest.SortBy switch
            {
                Sort.Newest => records.OrderByDescending(a => a.LastModificationRequest.Date).ThenBy(a => a.Event.Name).GetPaged(paginationRequest),
                Sort.Alphabetical => records.OrderBy(a => a.Event.Name).GetPaged(paginationRequest),
                (_) => records.OrderBy(a => a.Event.Name).GetPaged(paginationRequest),
            };
        }

        public async Task<EventRequest> GetEventRequestById(string id)
        {
            var filter = Builders<EventRequest>.Filter.Eq(c => c.Id, id);
            var result = _collection.Find(filter).Limit(1);
            return await result.FirstOrDefaultAsync();
        }

        public async Task<EventRequest> GetEventRequestByEventId(string eventId, RequestType type)
        {
            var filter = Builders<EventRequest>.Filter.Eq(c => c.Event.Id, eventId) &
                         Builders<EventRequest>.Filter.Eq(a => a.LastModificationRequest.Type, type);
            var result = _collection.Find(filter).Limit(1);
            return await result.FirstOrDefaultAsync();
        }

        public async Task<bool> ApproveEvent(string requestId, Event eventRequest)
        {
            var oldEvent = _eventCollection.Find(v => v.Id == eventRequest.Id).FirstOrDefault();

            var result = await _eventCollection.FindOneAndReplaceAsync<Event, Event>(a => a.Id.Equals(eventRequest.Id), eventRequest, new FindOneAndReplaceOptions<Event, Event> { IsUpsert = true, ReturnDocument = ReturnDocument.After });

            if (result != null)
            {
                var requestFilter = Builders<EventRequest>.Filter.Eq(a => a.Id, requestId);
                await _collection.DeleteOneAsync(requestFilter);
            }

            await SyncOldEvent(oldEvent, eventRequest);

            return result != null;
        }

        private Task SyncOldEvent(Event oldOtherEntity, Event otherEntity)
        {
            if (oldOtherEntity?.Name != otherEntity.Name || oldOtherEntity?.Image != otherEntity.Image || oldOtherEntity?.Location != otherEntity.Location ||
                oldOtherEntity?.Status != otherEntity.Status || oldOtherEntity?.PhoneNumber != otherEntity.PhoneNumber)
            {
                var eventFilter = Builders<EventRequest>.Filter.Eq(v => v.OldEvent.Id, otherEntity.Id);
                var updateTypeDef = Builders<EventRequest>.Update.Set(v => v.OldEvent.Name, otherEntity.Name)
                                                                 .Set(v => v.OldEvent.Image, otherEntity.Image)
                                                                 .Set(v => v.OldEvent.Location, otherEntity.Location)
                                                                 .Set(v => v.OldEvent.Status, otherEntity.Status)
                                                                 .Set(v => v.OldEvent.PhoneNumber, otherEntity.PhoneNumber);
                return _collection.UpdateManyAsync(eventFilter, updateTypeDef);
            }
            return Task.CompletedTask;
        }

        public async Task<bool> DeleteEventRequest(string id)
        {
            var filter = Builders<EventRequest>.Filter.Eq(a => a.Id, id);
            var result = await _collection.DeleteOneAsync(filter);
            return result.IsAcknowledged;
        }

        public async Task<bool> AddOccurrenceToEvent(string id, EventOccurrence eventOccurrence)
        {
            var filter = Builders<EventRequest>.Filter.Eq(a => a.Event.Id, id) &
                         Builders<EventRequest>.Filter.Eq(a => a.LastModificationRequest.Type, RequestType.UpdateEvent);
            var update = Builders<EventRequest>.Update.Push(a => a.Event.Occurrences, eventOccurrence);
            var updateResult = await _collection.UpdateOneAsync(filter, update);
            return updateResult.IsAcknowledged;
        }

        public async Task<bool> UpdateOccurrenceDateTime(string eventOccurrenceId, EventOccurrence occurrence)
        {
            var filter = Builders<EventRequest>.Filter.ElemMatch(a => a.Event.Occurrences, Builders<EventOccurrence>.Filter.ObjectIdEq("Id", eventOccurrenceId)) &
                         Builders<EventRequest>.Filter.Eq(a => a.LastModificationRequest.Type, RequestType.UpdateEvent);
            var update = Builders<EventRequest>.Update.Set("Event.Occurrences.$.StartDate", occurrence.StartDate.Date)
                                                      .Set("Event.Occurrences.$.EndDate", occurrence.EndDate.Date)
                                                      .Set("Event.Occurrences.$.StartTime", occurrence.StartTime)
                                                      .Set("Event.Occurrences.$.EndTime", occurrence.EndTime);
            var updateResult = await _collection.UpdateOneAsync(filter, update);
            return updateResult.IsAcknowledged;
        }

        public async Task<bool> DeleteOccurrenceFromEvent(string id, string occurrenceId)
        {
            var filter = Builders<EventRequest>.Filter.Eq(a => a.Event.Id, id) &
                         Builders<EventRequest>.Filter.Eq(a => a.LastModificationRequest.Type, RequestType.UpdateEvent);
            var update = Builders<EventRequest>.Update.PullFilter(a => a.Event.Occurrences, Builders<EventOccurrence>.Filter.Eq("Id", new BsonObjectId(new ObjectId(occurrenceId))));
            var updateResult = await _collection.UpdateOneAsync(filter, update);
            return updateResult.IsAcknowledged;
        }
        public async Task<bool> DeleteOccurrenceFromEvent(string id, List<string> occurrenceIds)
        {
            var filter = Builders<EventRequest>.Filter.Eq(a => a.Event.Id, id) &
                         Builders<EventRequest>.Filter.Eq(a => a.LastModificationRequest.Type, RequestType.UpdateEvent);

            var update = Builders<EventRequest>.Update.PullFilter(a => a.Event.Occurrences,
                Builders<EventOccurrence>.Filter.ObjectIdIn("Id", occurrenceIds));

            var updateResult = await _collection.UpdateOneAsync(filter, update);
            return updateResult.IsAcknowledged;
        }
    }
}
