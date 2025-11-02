using MongoDB.Driver;
using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Data;
using OutOut.Persistence.Extensions;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.Persistence.Services.Basic;
using OutOut.ViewModels.Requests.Loyalties;
using OutOut.ViewModels.Requests.Venues;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Services
{
    public class UserLoyaltyRepository : GenericNonSqlRepository<UserLoyalty>, IUserLoyaltyRepository
    {
        public UserLoyaltyRepository(ApplicationNonSqlDbContext dbContext, IEnumerable<ISyncRepository<UserLoyalty>> syncRepositories) : base(dbContext, syncRepositories)
        {
        }

        public async Task<UserLoyalty> GetLatestUserLoyalty(string userId, string loyaltyId)
        {
            var result = await GetAllLoyaltyRedeems(userId, loyaltyId, isConsumed: false);
            return result?.FirstOrDefault();
        }

        public async Task<List<UserLoyalty>> GetUserLoyalty(string userId)
        {
            var userFilter = Builders<UserLoyalty>.Filter.Eq(c => c.UserId, userId);
            return await Find(userFilter);
        }
        public async Task<List<UserLoyalty>> GetUserLoyaltiesByLoyaltyId(string loyaltyId)
        {
            var userFilter = Builders<UserLoyalty>.Filter.Eq(c => c.Loyalty.Id, loyaltyId) &
                             Builders<UserLoyalty>.Filter.Eq(c => c.IsConsumed, false) &
                             Builders<UserLoyalty>.Filter.Eq(c => c.Loyalty.IsActive, true);
            return await Find(userFilter);
        }

        public Task<List<UserLoyalty>> GetUserLoyalty(LoyaltyFilterationRequest filterRequest, string userId)
        {
            var userFilter = Builders<UserLoyalty>.Filter.Empty;
            var isConsumedFilter = Builders<UserLoyalty>.Filter.Empty;
            var searchQuery = Builders<UserLoyalty>.Filter.Empty;

            if (filterRequest != null)
            {
                userFilter = Builders<UserLoyalty>.Filter.Eq(a => a.UserId, userId);

                if (filterRequest.TimeFilter == LoyaltyTimeFilter.History)
                    isConsumedFilter = Builders<UserLoyalty>.Filter.Eq(a => a.IsConsumed, true);

                else
                    isConsumedFilter = Builders<UserLoyalty>.Filter.Eq(a => a.IsConsumed, false);

                if (filterRequest.SearchQuery != null)
                    searchQuery = Builders<UserLoyalty>.Filter.SearchContains(a => a.Venue.Name, filterRequest.SearchQuery);
            }

            var loyaltyFilter = Builders<UserLoyalty>.Filter.And(userFilter & isConsumedFilter & searchQuery) & Builders<UserLoyalty>.Filter.SizeGte(a => a.Redemptions, 1);
            var sortDefinition = Builders<UserLoyalty>.Sort.Descending(c => c.LastModifiedDate);

            return _collection.Find(loyaltyFilter).Sort(sortDefinition).ToListAsync();
        }

        public Task<List<UserLoyalty>> GetAllLoyaltyRedeems(string userId, string loyaltyId, bool? isConsumed = null)
        {
            var userLoyaltyFilter = Builders<UserLoyalty>.Filter.Eq(c => c.UserId, userId) &
                                    Builders<UserLoyalty>.Filter.Eq(c => c.Loyalty.Id, loyaltyId);
            var isConsumedFilter = Builders<UserLoyalty>.Filter.Empty;
            if (isConsumed != null)
                isConsumedFilter = Builders<UserLoyalty>.Filter.Eq(c => c.IsConsumed, isConsumed);

            var filter = Builders<UserLoyalty>.Filter.And(userLoyaltyFilter & isConsumedFilter);
            return Find(filter);
        }

        public async Task DeleteUserLoyaltyByType(string loyaltyTypeId)
        {
            var filter = Builders<UserLoyalty>.Filter.Eq(a => a.Loyalty.Type.Id, loyaltyTypeId);
            await _collection.DeleteManyAsync(filter);
        }

        public async Task DeleteUserLoyaltyByVenue(string venueId)
        {
            var filter = Builders<UserLoyalty>.Filter.Eq(a => a.Venue.Id, venueId);
            await _collection.DeleteManyAsync(filter);
        }

        public async Task<List<UserLoyalty>> GetUserLoyaltyByAssignedLoyalty(string assignedLoyaltyId, string venueId)
        {
            var filter = Builders<UserLoyalty>.Filter.Eq(a => a.Loyalty.Id, assignedLoyaltyId) &
                         Builders<UserLoyalty>.Filter.Eq(a => a.Venue.Id, venueId);
            var userLoyalty = await _collection.FindAsync(filter);
            return userLoyalty.ToList();
        }

        public async Task DeleteUserLoyaltyByAssignedLoyalty(string assignedLoyaltyId, string venueId)
        {
            var filter = Builders<UserLoyalty>.Filter.Eq(a => a.Loyalty.Id, assignedLoyaltyId) &
                         Builders<UserLoyalty>.Filter.Eq(a => a.Venue.Id, venueId);
            await _collection.DeleteManyAsync(filter);
        }

        public async Task<bool> DeleteAllUserLoyalty()
        {
            var filter = Builders<UserLoyalty>.Filter.Eq(a => a.IsConsumed, true);
            var result = await _collection.DeleteManyAsync(filter);
            return result.IsAcknowledged;
        }

        public long GetConsumedUserLoyaltyCount(string loyaltyTypeId)
        {
            var filter = Builders<UserLoyalty>.Filter.Eq(a => a.Loyalty.Type.Id, loyaltyTypeId) &
                        Builders<UserLoyalty>.Filter.Eq(a => a.IsConsumed, true);
            return _collection.Find(filter).CountDocuments();
        }

        public Page<UserLoyalty> GetCustomersAvailedLoyalty(string userId, PaginationRequest paginationRequest, SearchFilterationRequest searchFilterationRequest, List<string> accessibleVenues, bool isSuperAdmin)
        {
            var searchFilter = Builders<UserLoyalty>.Filter.Empty;
            if (!string.IsNullOrEmpty(searchFilterationRequest?.SearchQuery))
                searchFilter = Builders<UserLoyalty>.Filter.SearchContains(a => a.Venue.Name, searchFilterationRequest?.SearchQuery);

            var filter = Builders<UserLoyalty>.Filter.Eq(a => a.UserId, userId) &
                         Builders<UserLoyalty>.Filter.Eq(a => a.IsConsumed, true) &
                         Builders<UserLoyalty>.Filter.InOrParameterEmpty(a => a.Venue.Id, accessibleVenues, isSuperAdmin);

            var records = _collection.Find(searchFilter & filter, new FindOptions { Collation = new Collation(locale: "en", strength: CollationStrength.Secondary) }).SortByDescending(a => a.LastModifiedDate).ThenBy(a => a.Venue.Name);
            var groupedResultByVenue = records.ToList().GroupBy(a => a.Venue.Id ).Select(a => a.FirstOrDefault()).ToList();
            return groupedResultByVenue.GetPaged(paginationRequest);
        }

        public long GetRedemptionsCount(string userId, string venueId)
        {
            var filter = Builders<UserLoyalty>.Filter.Eq(a => a.Venue.Id, venueId) &
                         Builders<UserLoyalty>.Filter.Eq(a => a.UserId, userId) &
                         Builders<UserLoyalty>.Filter.Eq(a => a.IsConsumed, true);
            return _collection.Find(filter).CountDocuments();
        }

        public long GetLoyaltyUsageCount(string venueId)
        {
            var filter = Builders<UserLoyalty>.Filter.Eq(a => a.Venue.Id, venueId) &
                         Builders<UserLoyalty>.Filter.Eq(a => a.IsConsumed, true);
            return _collection.Find(filter).CountDocuments();
        }

        public Task SyncVenueData(Venue oldVenue, Venue updatedVenue)
        {
            if (oldVenue?.Logo != updatedVenue.Logo)
            {
                var venueIdFilter = Builders<UserLoyalty>.Filter.Eq(v => v.Venue.Id, oldVenue.Id);
                var updateLogoDef = Builders<UserLoyalty>.Update.Set(v => v.Venue.Logo, updatedVenue.Logo)
                                                                .Set(v => v.Venue.Name, updatedVenue.Name);

                return _collection.UpdateManyAsync(venueIdFilter, updateLogoDef);
            }
            return Task.CompletedTask;
        }
    }
}