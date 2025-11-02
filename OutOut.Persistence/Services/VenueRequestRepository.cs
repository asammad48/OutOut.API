using MongoDB.Bson;
using MongoDB.Driver;
using OutOut.Constants.Enums;
using OutOut.Models.Domains;
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
    public class VenueRequestRepository : GenericNonSqlRepository<VenueRequest>, IVenueRequestRepository
    {
        private readonly IUserDetailsProvider _userDetailsProvider;
        protected IMongoCollection<Venue> _venueCollection
        {
            get { return _dbContext.GetCollection<Venue>(); }
        }
        public VenueRequestRepository(ApplicationNonSqlDbContext dbContext, IEnumerable<ISyncRepository<VenueRequest>> syncRepositories, IUserDetailsProvider userDetailsProvider) : base(dbContext, syncRepositories)
        {
            _userDetailsProvider = userDetailsProvider;
        }

        public async Task<bool> UpsertVenueRequest(Venue updatedVenue, Venue oldVenue, RequestType type, string modifiedId)
        {
            var filter = Builders<VenueRequest>.Filter.Eq(a => a.Venue.Id, updatedVenue.Id) &
                         Builders<VenueRequest>.Filter.Eq(a => a.LastModificationRequest.Type, type);
            var update = Builders<VenueRequest>.Update.Set(a => a.Venue, updatedVenue)
                                                      .Set(a => a.OldVenue, oldVenue)
                                                      .Set(a => a.LastModificationRequest, new LastModificationRequest { Type = type, ModifiedFieldId = modifiedId, CreatedBy = _userDetailsProvider.UserId })
                                                      .SetOnInsert(a => a.Id, ObjectId.GenerateNewId().ToString());
            var result = await _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
            return result.IsAcknowledged;
        }

        public async Task<Page<VenueRequest>> GetVenueRequests(PaginationRequest paginationRequest, FilterationRequest filterRequest, string createdBy = null)
        {
            var searchFilter = Builders<VenueRequest>.Filter.Empty;
            if (filterRequest != null && !string.IsNullOrEmpty(filterRequest?.SearchQuery))
                searchFilter = Builders<VenueRequest>.Filter.SearchContains(c => c.Venue.Name, filterRequest.SearchQuery) |
                               Builders<VenueRequest>.Filter.SearchContains(c => c.Venue.Location.City.Name, filterRequest.SearchQuery);
            var userFilter = Builders<VenueRequest>.Filter.Empty;
            if (!string.IsNullOrEmpty(createdBy))
                userFilter = Builders<VenueRequest>.Filter.Eq(a => a.LastModificationRequest.CreatedBy, createdBy);

            var sort = filterRequest.SortBy switch
            {
                Sort.Newest => Builders<VenueRequest>.Sort.Descending(a => a.LastModificationRequest.Date).Ascending(a => a.Venue.Name),
                Sort.Alphabetical => Builders<VenueRequest>.Sort.Ascending(a => a.Venue.Name),
                (_) => Builders<VenueRequest>.Sort.Ascending(a => a.Venue.Name),
            };

            var records = await _collection.FindAsync(searchFilter & userFilter, new FindOptions<VenueRequest, VenueRequest> { Sort = sort, Collation = new Collation(locale: "en", strength: CollationStrength.Secondary) });
            return records.ToList().GetPaged(paginationRequest);
        }

        public async Task<VenueRequest> GetVenueRequestById(string id)
        {
            var filter = Builders<VenueRequest>.Filter.Eq(c => c.Id, id);
            var result = _collection.Find(filter).Limit(1);
            return await result.FirstOrDefaultAsync();
        }

        public async Task<VenueRequest> GetVenueRequestByVenueId(string venueId, RequestType type, string modifiedFieldId = null)
        {
            var filter = Builders<VenueRequest>.Filter.Eq(c => c.Venue.Id, venueId) &
                         Builders<VenueRequest>.Filter.Eq(a => a.LastModificationRequest.Type, type);

            var modifiedFieldFilter = Builders<VenueRequest>.Filter.Empty;
            if (modifiedFieldId != null)
                modifiedFieldFilter = Builders<VenueRequest>.Filter.Eq(a => a.LastModificationRequest.ModifiedFieldId, modifiedFieldId);

            var result = _collection.Find(filter).Limit(1);
            return await result.FirstOrDefaultAsync();
        }

        public async Task<VenueRequest> GetVenueRequest(string venueId, RequestType type, string requestCreator)
        {
            var filter = Builders<VenueRequest>.Filter.Eq(c => c.Venue.Id, venueId) &
                         Builders<VenueRequest>.Filter.Eq(a => a.LastModificationRequest.Type, type) &
                         Builders<VenueRequest>.Filter.Eq(a => a.LastModificationRequest.CreatedBy, requestCreator);
            var result = _collection.Find(filter).Limit(1);
            return await result.FirstOrDefaultAsync();
        }

        public Task<List<UnwindVenueRequestOffer>> GetOffersByRequestId(string requetsId)
        {
            var filter = Builders<VenueRequest>.Filter.Eq(v => v.Id, requetsId);

            return _collection.Aggregate(new AggregateOptions { Collation = new Collation(locale: "en", strength: CollationStrength.Secondary), AllowDiskUse = true })
                              .Match(filter)
                              .Unwind<VenueRequest, UnwindVenueRequestOffer>(v => v.Venue.Offers)
                              .SortBy(a => a.Venue.Offer.ExpiryDate)
                              .ThenBy(a => a.Venue.Offer.Type.Name)
                              .ToListAsync();
        }

        public async Task<bool> ApproveVenue(string requestId, Venue venue)
        {
            var oldVenue = _venueCollection.Find(v => v.Id == venue.Id).FirstOrDefault();

            var result = await _venueCollection.FindOneAndReplaceAsync<Venue, Venue>(a => a.Id.Equals(venue.Id), venue, new FindOneAndReplaceOptions<Venue, Venue> { IsUpsert = true, ReturnDocument = ReturnDocument.After });

            if (result != null)
            {
                var requestFilter = Builders<VenueRequest>.Filter.Eq(a => a.Id, requestId);
                await _collection.DeleteOneAsync(requestFilter);
            }

            await SyncOldVenue(oldVenue, venue);

            return result != null;
        }

        private Task SyncOldVenue(Venue oldOtherEntity, Venue otherEntity)
        {
            if (oldOtherEntity?.Name != otherEntity.Name || oldOtherEntity?.Logo != otherEntity.Logo || oldOtherEntity?.Location != otherEntity.Location ||
                oldOtherEntity?.Status != otherEntity.Status || oldOtherEntity?.PhoneNumber != otherEntity.PhoneNumber)
            {
                var venueFilter = Builders<VenueRequest>.Filter.Eq(v => v.OldVenue.Id, otherEntity.Id);
                var updateTypeDef = Builders<VenueRequest>.Update.Set(v => v.OldVenue.Name, otherEntity.Name)
                                                                 .Set(v => v.OldVenue.Logo, otherEntity.Logo)
                                                                 .Set(v => v.OldVenue.Location, otherEntity.Location)
                                                                 .Set(v => v.OldVenue.Status, otherEntity.Status)
                                                                 .Set(v => v.OldVenue.PhoneNumber, otherEntity.PhoneNumber);
                return _collection.UpdateManyAsync(venueFilter, updateTypeDef);
            }
            return Task.CompletedTask;
        }

        public async Task<bool> DeleteVenueRequest(string venueId, RequestType type, string modifiedFieldId)
        {
            var request = await GetVenueRequestByVenueId(venueId, type, modifiedFieldId);
            var filter = Builders<VenueRequest>.Filter.Eq(a => a.Id, request?.Id);
            var result = await _collection.DeleteOneAsync(filter);
            return result.IsAcknowledged;
        }

        public async Task<bool> DeleteVenueRequest(string venueId, RequestType type)
        {
            var request = await GetVenueRequestByVenueId(venueId, type);
            var filter = Builders<VenueRequest>.Filter.Eq(a => a.Id, request?.Id);
            var result = await _collection.DeleteOneAsync(filter);
            return result.IsAcknowledged;
        }

        public async Task<bool> DeleteVenueRequest(string id)
        {
            var filter = Builders<VenueRequest>.Filter.Eq(a => a.Id, id);
            var result = await _collection.DeleteOneAsync(filter);
            return result.IsAcknowledged;
        }

        public async Task<bool> DeleteGalleryImages(string requestId, List<string> images)
        {
            var filter = Builders<VenueRequest>.Filter.Eq(a => a.Id, requestId);
            var update = Builders<VenueRequest>.Update.PullAll(a => a.Venue.Gallery, images);
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged;
        }

        public async Task<bool> DeleteVenueRequestsByType(RequestType type)
        {
            var filter = Builders<VenueRequest>.Filter.Eq(a => a.LastModificationRequest.Type, type);
            var result = await _collection.DeleteManyAsync(filter);
            return result.IsAcknowledged;
        }

        public async Task<bool> RequestUpdateAssignedOffer(string id, Offer offer)
        {
            var filter = Builders<VenueRequest>.Filter.Eq(a => a.Venue.Id, id) &
                         Builders<VenueRequest>.Filter.ElemMatch(a => a.Venue.Offers, a => a.Id == offer.Id);
            var update = Builders<VenueRequest>.Update.Set(a => a.Venue.Offers[-1], offer);
            var updateResult = await _collection.UpdateOneAsync(filter, update);
            return updateResult.IsAcknowledged;
        }
    }
}
