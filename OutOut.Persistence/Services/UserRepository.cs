using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using OutOut.Models;
using OutOut.Models.Identity;
using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Data;
using OutOut.Persistence.Extensions;
using OutOut.Persistence.Interfaces;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Services
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationNonSqlDbContext _dbContext;
        private readonly IMongoCollection<ApplicationUser> _collection;
        private readonly AppSettings _appSettings;
        public UserRepository(ApplicationNonSqlDbContext dbContext, IOptions<AppSettings> appSettings)
        {
            _dbContext = dbContext;
            _collection = _dbContext.GetCollection<ApplicationUser>();
            _appSettings = appSettings.Value;
        }

        public FilterDefinition<ApplicationUser> GetDistance(Location location)
        {
            var point = GeoJson.Point(GeoJson.Geographic(location.GeoPoint.Coordinates.Longitude, location.GeoPoint.Coordinates.Latitude));
            return Builders<ApplicationUser>.Filter.Ne(user => user.Location, null) &
                      Builders<ApplicationUser>.Filter.Size(user => user.Roles, 0) &
                      Builders<ApplicationUser>.Filter.Near(user => user.Location.GeoPoint, point, maxDistance: _appSettings.UserRadius);
        }

        public async Task AssignFirebaseMessagingTokenToUser(string userId, string fcmToken)
        {
            //remove fcm token from users if found
            var fcmTokenFilter = Builders<ApplicationUser>.Filter.AnyEq(a => a.FirebaseMessagingTokens, fcmToken);
            var removeFcmTokenUpdate = Builders<ApplicationUser>.Update.Pull(a => a.FirebaseMessagingTokens, fcmToken);
            await _collection.UpdateManyAsync(fcmTokenFilter, removeFcmTokenUpdate);

            //add the new fcm token to the user
            var userIdFilter = Builders<ApplicationUser>.Filter.Eq(a => a.Id, userId);
            var addFcmTokenUpdate = Builders<ApplicationUser>.Update.AddToSet(a => a.FirebaseMessagingTokens, fcmToken);
            await _collection.UpdateManyAsync(userIdFilter, addFcmTokenUpdate);
        }

        public Task UnassignFirebaseMessagingTokenFromUser(string userId, string fcmToken)
        {
            //remove fcm token from the user
            var userIdFilter = Builders<ApplicationUser>.Filter.Eq(a => a.Id, userId);
            var removeFcmTokenUpdate = Builders<ApplicationUser>.Update.Pull(a => a.FirebaseMessagingTokens, fcmToken);
            return _collection.UpdateOneAsync(userIdFilter, removeFcmTokenUpdate);
        }

        public async Task<List<string>> GetAllFirebaseMessagingTokens()
        {
            var result = await _collection
                .Aggregate()
                .Project(a => new { a.FirebaseMessagingTokens })
                .ToListAsync();
            var fcmTokens = result.SelectMany(x => x.FirebaseMessagingTokens).ToList();
            return fcmTokens;
        }

        public async Task<List<string>> GetFirebaseMessagingTokens(string userId)
        {
            var userFilter = Builders<ApplicationUser>.Filter.Eq(a => a.Id, userId);
            var result = await _collection
                .Aggregate()
                .Match(userFilter)
                .Project(a => new { a.FirebaseMessagingTokens })
                .ToListAsync();
            var fcmTokens = result.SelectMany(x => x.FirebaseMessagingTokens).ToList();
            return fcmTokens;
        }

        public Task<List<ApplicationUser>> GetInactiveUsers()
        {
            var filter = Builders<ApplicationUser>.Filter.Lte(a => a.LastUsage.LastUsageDate, DateTime.UtcNow.AddDays(-30).Date) &
                         Builders<ApplicationUser>.Filter.Lte(a => a.LastUsage.LastNotificationSentDate, DateTime.UtcNow.AddDays(-30).Date);
            return _collection.Find(filter).ToListAsync();
        }

        public async Task<bool> AddToSharedTickets(string id, SharedTicket sharedTicket)
        {
            var userFilter = Builders<ApplicationUser>.Filter.Eq(a => a.Id, id);
            var updateFilter = Builders<ApplicationUser>.Update.AddToSet(a => a.SharedTickets, sharedTicket);
            var result = await _collection.UpdateOneAsync(userFilter, updateFilter);
            return result.IsAcknowledged;
        }

        public async Task<bool> UpdateLastUsageDate(string id)
        {
            var userFilter = Builders<ApplicationUser>.Filter.Eq(a => a.Id, id);
            var updateFilter = Builders<ApplicationUser>.Update.Set(a => a.LastUsage.LastUsageDate, DateTime.UtcNow.Date);
            var result = await _collection.UpdateOneAsync(userFilter, updateFilter);
            return result.IsAcknowledged;
        }

        public async Task<Page<ApplicationUser>> GetUsers(PaginationRequest paginationRequest, FilterationRequest filterRequest, string userId)
        {
            var searchFilter = Builders<ApplicationUser>.Filter.Empty;
            if (filterRequest != null && !string.IsNullOrEmpty(filterRequest.SearchQuery))
                searchFilter = Builders<ApplicationUser>.Filter.SearchContains(a => a.FullName, filterRequest.SearchQuery);

            var roleFilter = Builders<ApplicationUser>.Filter.SizeGt(a => a.Roles, 0);

            var userFilter = Builders<ApplicationUser>.Filter.Ne(a => a.Id, userId);

            var collation = new Collation(locale: "en", strength: CollationStrength.Secondary);

            var sort = filterRequest.SortBy switch
            {
                Sort.Newest => Builders<ApplicationUser>.Sort.Descending(a => a.CreationDate).Ascending(a => a.FullName),
                Sort.Alphabetical => Builders<ApplicationUser>.Sort.Ascending(a => a.FullName),
                (_) => Builders<ApplicationUser>.Sort.Ascending(a => a.FullName),
            };

            var records = await _collection.Find(searchFilter & roleFilter & userFilter, new FindOptions { Collation = collation })
                                           .Sort(sort)
                                            .Skip(paginationRequest.PageNumber * paginationRequest.PageSize)
                                           .Limit(paginationRequest.PageSize)
                                           .ToListAsync();
            var recordsCount = _collection.Aggregate().Match(searchFilter).Match(roleFilter).Match(userFilter).Count().FirstOrDefault()?.Count ?? 0;
            return new Page<ApplicationUser>(records, paginationRequest.PageNumber, paginationRequest.PageSize, recordsCount);
        }

        public async Task<Page<ApplicationUser>> GetCustomersPage(PaginationRequest paginationRequest, FilterationRequest filterRequest)
        {
            var searchFilter = Builders<ApplicationUser>.Filter.Empty;
            if (filterRequest != null && !string.IsNullOrEmpty(filterRequest.SearchQuery))
                searchFilter = Builders<ApplicationUser>.Filter.SearchContains(a => a.FullName, filterRequest.SearchQuery);

            var roleFilter = Builders<ApplicationUser>.Filter.Size(a => a.Roles, 0);

            var collation = new Collation(locale: "en", strength: CollationStrength.Secondary);

            var sort = filterRequest.SortBy switch
            {
                Sort.Newest => Builders<ApplicationUser>.Sort.Descending(a => a.CreationDate).Ascending(a => a.FullName),
                Sort.Alphabetical => Builders<ApplicationUser>.Sort.Ascending(a => a.FullName),
                (_) => Builders<ApplicationUser>.Sort.Ascending(a => a.FullName),
            };

            var records = await _collection.Find(searchFilter & roleFilter, new FindOptions { Collation = collation })
                                            .Sort(sort)
                                            .Skip(paginationRequest.PageNumber * paginationRequest.PageSize)
                                            .Limit(paginationRequest.PageSize)
                                            .ToListAsync();
            var recordsCount = _collection.Aggregate().Match(searchFilter & roleFilter).Count().FirstOrDefault()?.Count ?? 0;
            return new Page<ApplicationUser>(records, paginationRequest.PageNumber, paginationRequest.PageSize, recordsCount);
        }

        public async Task<bool> AddVenueIdToAccessibleVenues(string userId, string venueId)
        {
            var filter = Builders<ApplicationUser>.Filter.Eq(a => a.Id, userId) &
                         Builders<ApplicationUser>.Filter.SizeGt(a => a.Roles, 0);
            var update = Builders<ApplicationUser>.Update.AddToSet(a => a.AccessibleVenues, venueId);
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged;
        }

        public async Task<bool> AddEventIdToAccessibleEvents(string userId, string eventId)
        {
            var filter = Builders<ApplicationUser>.Filter.Eq(a => a.Id, userId) &
                         Builders<ApplicationUser>.Filter.SizeGt(a => a.Roles, 0);
            var update = Builders<ApplicationUser>.Update.AddToSet(a => a.AccessibleEvents, eventId);
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged;
        }

        public async Task<bool> DeleteEventIdFromAccessibleEvents(string eventId)
        {
            var filter = Builders<ApplicationUser>.Filter.AnyEq(a => a.AccessibleEvents, eventId) &
                         Builders<ApplicationUser>.Filter.SizeGt(a => a.Roles, 0);
            var update = Builders<ApplicationUser>.Update.Pull(a => a.AccessibleEvents, eventId);
            var result = await _collection.UpdateManyAsync(filter, update);
            return result.IsAcknowledged;
        }

        public async Task<bool> DeleteVenueIdFromAccessibleVenues(string venueId)
        {
            var filter = Builders<ApplicationUser>.Filter.AnyEq(a => a.AccessibleVenues, venueId) &
                         Builders<ApplicationUser>.Filter.SizeGt(a => a.Roles, 0);
            var update = Builders<ApplicationUser>.Update.Pull(a => a.AccessibleVenues, venueId);
            var result = await _collection.UpdateManyAsync(filter, update);
            return result.IsAcknowledged;
        }

        public async Task<ApplicationUser> GetUserById(string id)
        {
            var filter = Builders<ApplicationUser>.Filter.Eq(c => c.Id, id);
            var query = _collection.Find(filter).Limit(1);
            return await query.FirstOrDefaultAsync();
        }
        public async Task<Page<ApplicationUser>> GetCustomersByIds(PaginationRequest paginationRequest, FilterationRequest sortRequest, List<string> ids)
        {
            var searchFilter = Builders<ApplicationUser>.Filter.Empty;

            var userFilter = Builders<ApplicationUser>.Filter.In(a => a.Id, ids);

            var collation = new Collation(locale: "en", strength: CollationStrength.Secondary);

            var sort = sortRequest.SortBy switch
            {
                Sort.Newest => Builders<ApplicationUser>.Sort.Descending(a => a.CreationDate).Ascending(a => a.FullName),
                Sort.Alphabetical => Builders<ApplicationUser>.Sort.Ascending(a => a.FullName),
                (_) => Builders<ApplicationUser>.Sort.Ascending(a => a.FullName),
            };

            var records = await _collection.Find(searchFilter & userFilter, new FindOptions { Collation = collation })
                                           .Sort(sort)
                                            .Skip(paginationRequest.PageNumber * paginationRequest.PageSize)
                                           .Limit(paginationRequest.PageSize)
                                           .ToListAsync();
            var recordsCount = _collection.Aggregate().Match(searchFilter).Match(userFilter).Count().FirstOrDefault()?.Count ?? 0;
            return new Page<ApplicationUser>(records, paginationRequest.PageNumber, paginationRequest.PageSize, recordsCount);
        }
        public bool EmailExistsForOtherUsers(string userId, string email)
        {
            var filter = Builders<ApplicationUser>.Filter.Ne(c => c.Id, userId) & Builders<ApplicationUser>.Filter.Eq(c => c.Email, email);
            var query = _collection.Find(filter).Limit(1);
            return query.FirstOrDefault() != null;
        }
    }
}
