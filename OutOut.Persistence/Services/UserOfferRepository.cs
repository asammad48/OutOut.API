using MongoDB.Driver;
using OutOut.Models.Models;
using OutOut.Models.Utils;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Data;
using OutOut.Persistence.Extensions;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.Persistence.Services.Basic;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Requests.Venues;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Services
{
    public class UserOfferRepository : GenericNonSqlRepository<UserOffer>, IUserOfferRepository
    {
        public UserOfferRepository(ApplicationNonSqlDbContext dbContext, IEnumerable<ISyncRepository<UserOffer>> syncRepositories) : base(dbContext, syncRepositories)
        {
        }

        public Task<UserOffer> GetUserRedeems(string userId, string offerId, DateTime inDate)
        {
            var filters = Builders<UserOffer>.Filter.Eq(u => u.UserId, userId) &
                          Builders<UserOffer>.Filter.Eq(u => u.Offer.Id, offerId) &
                          Builders<UserOffer>.Filter.Eq(u => u.Day, inDate.Date.Date);
            return _collection.Find(filters).FirstOrDefaultAsync();
        }

        public Task<List<UserOffer>> GetUserRedeemsThisYear(string userId, string offerId)
        {
            var filters = Builders<UserOffer>.Filter.Eq(u => u.UserId, userId) &
                          Builders<UserOffer>.Filter.Eq(u => u.Offer.Id, offerId) &
                          (Builders<UserOffer>.Filter.Gte(u => u.Day, new DateTime(UAEDateTime.Now.Year, 1, 1)) &
                           Builders<UserOffer>.Filter.Lte(u => u.Day, new DateTime(UAEDateTime.Now.Year + 1, 1, 1)));
            return Find(filters);
        }

        public long GetUserOffersCountByUserIdAndOfferId(string offerId, string userId)
        {
            var offerIdfilter = Builders<UserOffer>.Filter.Eq(u => u.Offer.Id, offerId);
            var userIdfilter = Builders<UserOffer>.Filter.Eq(u => u.UserId, userId);
            var record = _collection.Find(offerIdfilter & userIdfilter).FirstOrDefault();
            return record?.Log?.Count ?? 0;
        }

        public long GetUserOffersCountById(string id)
        {
            var offerIdfilter = Builders<UserOffer>.Filter.Eq(u => u.Offer.Id, id);
            return _collection.Find(offerIdfilter).CountDocuments();
        }

        public long GetUserOffersCount(string offerTypeId)
        {
            var offerIdfilter = Builders<UserOffer>.Filter.Eq(u => u.Offer.Type.Id, offerTypeId);
            return _collection.Find(offerIdfilter).CountDocuments();
        }

        public async Task DeleteUserOfferByType(string offerTypeId)
        {
            var filter = Builders<UserOffer>.Filter.Eq(a => a.Offer.Type.Id, offerTypeId);
            await _collection.DeleteManyAsync(filter);
        }

        public async Task DeleteUserOffersByAssignedOffer(string assignedOfferId)
        {
            var filter = Builders<UserOffer>.Filter.Eq(a => a.Offer.Id, assignedOfferId);
            await _collection.DeleteManyAsync(filter);
        }
        public async Task<List<UserOffer>> GetUserOffersByAssignedOffer(string assignedOfferId)
        {
            var filter = Builders<UserOffer>.Filter.Eq(a => a.Offer.Id, assignedOfferId);
            var userOffers = await _collection.FindAsync(filter);
            return userOffers.ToList();
        }
        public async Task<List<UserOffer>> GetUserOffersByVenueId(string venueId)
        {
            var filter = Builders<UserOffer>.Filter.Eq(a => a.Venue.Id, venueId);
            var userOffers = await _collection.FindAsync(filter);
            return userOffers.ToList();
        }
        public async Task<bool> DeleteAllUserOffers()
        {
            var filter = Builders<UserOffer>.Filter.Exists(a => a.Offer.Id);
            var result = await _collection.DeleteManyAsync(filter);
            return result.IsAcknowledged;
        }

        public async Task<Page<UserOffer>> GetCustomersAvailedOffers(string userId, PaginationRequest paginationRequest, SearchFilterationRequest searchFilterationRequest, List<string> accessibleVenues, bool isSuperAdmin)
        {
            var searchFilter = Builders<UserOffer>.Filter.Empty;
            if (!string.IsNullOrEmpty(searchFilterationRequest?.SearchQuery))
                searchFilter = Builders<UserOffer>.Filter.SearchContains(a => a.Venue.Name, searchFilterationRequest?.SearchQuery);

            var userFilter = Builders<UserOffer>.Filter.Eq(a => a.UserId, userId);

            var accessibleVenuesFilter = Builders<UserOffer>.Filter.InOrParameterEmpty(a => a.Venue.Id, accessibleVenues, isSuperAdmin);

            var collation = new Collation(locale: "en", strength: CollationStrength.Secondary);

            var records = await _collection.FindAsync(searchFilter & accessibleVenuesFilter & userFilter, new FindOptions<UserOffer, UserOffer> { Collation = collation });
            var groupedResultByVenue = records.ToList().OrderByDescending(a => a.Day.Add(a.Log[0].Time)).ThenBy(a => a.Venue.Name).GroupBy(a => new { VenueId = a.Venue.Id, Offer = a.Offer.Type.Name }).Select(a => a.FirstOrDefault()).ToList();

            return groupedResultByVenue.GetPaged(paginationRequest);
        }

        public long GetRedemptionsCount(string offerTypeId, string userId, string venueId)
        {
            var filter = Builders<UserOffer>.Filter.Eq(a => a.Offer.Type.Id, offerTypeId) &
                         Builders<UserOffer>.Filter.Eq(a => a.Venue.Id, venueId) &
                         Builders<UserOffer>.Filter.Eq(a => a.UserId, userId);
            return _collection.Find(filter).CountDocuments();
        }

        public long GetOfferUsageCountByVenueId(string venueId, string typeId)
        {
            var filter = Builders<UserOffer>.Filter.Eq(a => a.Venue.Id, venueId) & Builders<UserOffer>.Filter.Eq(a => a.Offer.Type.Id, typeId);
            return _collection.Find(filter).CountDocuments();
        }

        public Task SyncVenueData(Venue oldVenue, Venue updatedVenue)
        {
            if (oldVenue?.Logo != updatedVenue.Logo || oldVenue?.Name != updatedVenue.Name)
            {
                var venueIdFilter = Builders<UserOffer>.Filter.Eq(v => v.Venue.Id, oldVenue.Id);
                var updateLogoDef = Builders<UserOffer>.Update.Set(v => v.Venue.Logo, updatedVenue.Logo)
                                                              .Set(v => v.Venue.Name, updatedVenue.Name);

                return _collection.UpdateManyAsync(venueIdFilter, updateLogoDef);
            }
            return Task.CompletedTask;
        }

        public Task<Page<UserOffer>> GetUserOffersByUserId(string userId, PaginationRequest paginationRequest, FilterationRequest filterationRequest)
        {
            var userFilter = Builders<UserOffer>.Filter.Eq(o => o.UserId, userId);
            var searchFilter = Builders<UserOffer>.Filter.Empty;
            if (!string.IsNullOrEmpty(filterationRequest?.SearchQuery))
                searchFilter = Builders<UserOffer>.Filter.SearchContains(a => a.Venue.Name, filterationRequest?.SearchQuery) |
                    Builders<UserOffer>.Filter.SearchContains(a => a.Offer.Type.Name, filterationRequest?.SearchQuery);

            var sortDef = filterationRequest.SortBy switch
            {
                Sort.Newest => Builders<UserOffer>.Sort.Descending(a => a.Offer.AssignDate).Ascending(a => a.Offer.Type.Name),
                Sort.Alphabetical => Builders<UserOffer>.Sort.Ascending(a => a.Offer.Type.Name),
                (_) => Builders<UserOffer>.Sort.Ascending(a => a.Offer.Type.Name),
            };

            return GetPageWhere(paginationRequest, userFilter & searchFilter, sortDef);
        }
    }
}
