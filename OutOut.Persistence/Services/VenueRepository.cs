using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using OutOut.Constants;
using OutOut.Constants.Enums;
using OutOut.Models.Domain;
using OutOut.Models.Models;
using OutOut.Models.Utils;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Data;
using OutOut.Persistence.Extensions;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.Persistence.Services.Basic;
using OutOut.ViewModels.Enums;
using OutOut.ViewModels.Requests.Areas;
using OutOut.ViewModels.Requests.HomePage;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Requests.Venues;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Services
{
    public class VenueRepository : GenericNonSqlRepository<Venue>, IVenueRepository
    {
        protected IMongoCollection<UserLoyalty> _userLoyaltyCollection { get { return _dbContext.GetCollection<UserLoyalty>(); } }
        public VenueRepository(ApplicationNonSqlDbContext dbContext, IEnumerable<ISyncRepository<Venue>> syncRepositories) : base(dbContext, syncRepositories) { }
        public async Task<Venue> GetByEventId(string eventId)
        {
            var filter = Builders<Venue>.Filter.AnyEq(a => a.Events, eventId);
            return await FindFirst(filter);
        }
        private FilterDefinition<Venue> LocationFilter(UserLocation userLocation)
        {
            var point = GeoJson.Point(GeoJson.Geographic(userLocation.GeoPoint.Coordinates.Longitude, userLocation.GeoPoint.Coordinates.Latitude));
            return Builders<Venue>.Filter.Near(e => e.Location.GeoPoint, point);
        }

        private FilterDefinition<Venue> SearchFilter(string searchQuery)
        {
            var searchFilter = Builders<Venue>.Filter.Empty;
            if (!string.IsNullOrEmpty(searchQuery))
                searchFilter = Builders<Venue>.Filter.SearchContains(c => c.Name, searchQuery);
            return searchFilter;
        }

        private FilterDefinition<VenueWithDistance> CategoriesFilter(VenueFilterationRequest filterRequest)
        {
            var categoryFilter = Builders<VenueWithDistance>.Filter.Empty;
            if (filterRequest != null && filterRequest.CategoriesIds != null && filterRequest.CategoriesIds.Any())
                categoryFilter = Builders<VenueWithDistance>.Filter.Where(a => a.Categories.Any(a => filterRequest.CategoriesIds.Contains(a.Id)));
            return categoryFilter;
        }

        private FilterDefinition<VenueWithDistance> VenueOnValidTimeFilter()
        {
            return Builders<VenueWithDistance>.Filter.ElemMatch(v => v.OpenTimes, AvailableTimeFilters.IsCurrentlyAvailable());
        }

        private FilterDefinition<Venue> AvailableVenueFilter() =>
            Builders<Venue>.Filter.Eq(a => a.Status, Availability.Active);

        private FilterDefinition<VenueWithDistance> TimeFilter(VenueFilterationRequest filterRequest)
        {
            var timeFilter = Builders<VenueWithDistance>.Filter.Empty;
            if (filterRequest != null && filterRequest.TimeFilter != null && filterRequest.TimeFilter != VenueTimeFilter.All && filterRequest.TimeFilter != VenueTimeFilter.NearYou)
            {
                if (filterRequest.TimeFilter == VenueTimeFilter.OffersNow)
                {
                    var activeOfferFilter = Builders<Offer>.Filter.Eq(o => o.IsActive, true);
                    var nonExpiredOfferFilter = Builders<Offer>.Filter.Gte(o => o.ExpiryDate, UAEDateTime.Now.Date);
                    var offerFilter = Builders<VenueWithDistance>.Filter.ElemMatch(v => v.Offers, Builders<Offer>.Filter.ElemMatch(o => o.ValidOn, AvailableTimeFilters.IsCurrentlyAvailable())
                                      & activeOfferFilter
                                      & nonExpiredOfferFilter);
                    timeFilter = offerFilter & VenueOnValidTimeFilter();
                }
                else if (filterRequest.TimeFilter != VenueTimeFilter.All)
                    timeFilter = VenueOnValidTimeFilter();
            }
            return timeFilter;
        }

        public Task<List<Venue>> GetAllVenues(SearchFilterationRequest searchFilterationRequest, List<string> accessibleVenues, bool isSuperAdmin)
        {
            var searchFilter = SearchFilter(searchFilterationRequest?.SearchQuery) & Builders<Venue>.Filter.InOrParameterEmpty(a => a.Id, accessibleVenues, isSuperAdmin);
            return _collection.Find(searchFilter, new FindOptions { Collation = new Collation(locale: "en", strength: CollationStrength.Secondary) })
                              .SortBy(a => a.Name)
                              .ToListAsync();
        }

        public Task<List<Venue>> GetActiveVenues(SearchFilterationRequest searchFilterationRequest)
        {
            var searchFilter = AvailableVenueFilter() & SearchFilter(searchFilterationRequest?.SearchQuery); 
            return _collection.Find(searchFilter, new FindOptions { Collation = new Collation(locale: "en", strength: CollationStrength.Secondary) })
                              .SortBy(a => a.Name)
                              .ToListAsync();
        }

        public Task<List<Venue>> GetActiveAccessibleVenues(SearchFilterationRequest searchFilterationRequest, List<string> accessibleVenues, bool isSuperAdmin)
        {
            var searchFilter = AvailableVenueFilter() & SearchFilter(searchFilterationRequest?.SearchQuery) & Builders<Venue>.Filter.InOrParameterEmpty(a => a.Id, accessibleVenues, isSuperAdmin);
            return _collection.Find(searchFilter, new FindOptions { Collation = new Collation(locale: "en", strength: CollationStrength.Secondary) })
                              .SortBy(a => a.Name)
                              .ToListAsync();
        }

        public Page<VenueWithDistance> GetVenues(PaginationRequest paginationRequest, UserLocation userLocation, VenueFilterationRequest filterRequest)
        {
            var searchFilter = Builders<VenueWithDistance>.Filter.Empty;
            if (!string.IsNullOrEmpty(filterRequest?.SearchQuery))
                searchFilter = Builders<VenueWithDistance>.Filter.SearchContains(c => c.Name, filterRequest.SearchQuery);

            var categoriesFilter = CategoriesFilter(filterRequest);
            var timeFilter = TimeFilter(filterRequest);
            var activeVenues = Builders<VenueWithDistance>.Filter.Eq(a => a.Status, Availability.Active);

            var venueFilters = Builders<VenueWithDistance>.Filter.And(timeFilter & searchFilter & categoriesFilter & activeVenues);

            var geoNearOptions = new BsonDocument
                            {
                                {"spherical", true},
                                {"allowDiskUse",true},
                                {"near",new BsonArray(new double[] { userLocation.GeoPoint.Coordinates.Longitude, userLocation.GeoPoint.Coordinates.Latitude })},
                                {"distanceField", "Distance"},
                                {"distanceMultiplier", GenericConstants.EarthRadiusInKm}
                            };

            var results = _collection.Aggregate(new AggregateOptions { Collation = new Collation(locale: "en", strength: CollationStrength.Secondary), AllowDiskUse = true })
                                     .AppendStage(new BsonDocumentPipelineStageDefinition<Venue, VenueWithDistance>(new BsonDocument { { "$geoNear", geoNearOptions } }))
                                     .Match(venueFilters);

            if (filterRequest?.TimeFilter == VenueTimeFilter.All)
                return results.SortBy(a => a.Name).ToList().GetPaged(paginationRequest);

            else
                return results.SortBy(a => a.Distance).ThenBy(a => a.Name).ToList().GetPaged(paginationRequest);
        }

        public async Task<List<Venue>> GetUsersFavoriteVenues(List<string> venuesIds, SearchFilterationRequest filterRequest)
        {
            var filter = SearchFilter(filterRequest?.SearchQuery) &
                         Builders<Venue>.Filter.In(c => c.Id, venuesIds);
            var records = await _collection.Find(filter).ToListAsync();
            return records.OrderBy(v => venuesIds.IndexOf(v.Id)).ToList();
        }

        public async Task<Venue> GetByLoyaltyId(string loyaltyId)
        {
            var venueFilter = Builders<Venue>.Filter.Eq(c => c.Loyalty.Id, loyaltyId);
            return await FindFirst(venueFilter);
        }

        public async Task<Venue> GetByOfferId(string offerId)
        {
            var venueFilter = Builders<Venue>.Filter.ElemMatch(c => c.Offers, a => a.Id == offerId);
            return await FindFirst(venueFilter);
        }

        public Task<VenueOneOffer> GetVenueByOfferId(string offerId)
        {
            var filter = Builders<VenueOneOffer>.Filter.Eq(v => v.Offer.Id, offerId);

            return _collection.Aggregate()
                              .Unwind<Venue, VenueOneOffer>(v => v.Offers)
                              .Match(filter)
                              .FirstOrDefaultAsync();
        }

        public Task<List<VenueOneOffer>> GetOffersByVenueId(string id)
        {
            var filter = Builders<Venue>.Filter.Eq(v => v.Id, id);

            return _collection.Aggregate()
                              .Match(filter)
                              .Unwind<Venue, VenueOneOffer>(v => v.Offers)
                              .SortByDescending(a => a.Offer.AssignDate)
                              .ThenBy(a => a.Offer.Type.Name)
                              .ToListAsync();
        }

        public Task<List<Venue>> HomeFilter(UserLocation userLocation, HomePageFilterationRequest filterRequest)
        {
            var searchFilter = SearchFilter(filterRequest?.SearchQuery);

            var categoryFilter = Builders<Venue>.Filter.Empty;
            if (filterRequest != null && filterRequest.VenueCategories != null && filterRequest.VenueCategories.Any())
                categoryFilter = Builders<Venue>.Filter.Where(a => a.Categories.Any(a => filterRequest.VenueCategories.Contains(a.Id)));

            var offerTypeFilter = Builders<Venue>.Filter.Empty;
            if (filterRequest != null && !string.IsNullOrEmpty(filterRequest.OfferTypeId))
                offerTypeFilter = Builders<Venue>.Filter.ElemMatch(a => a.Offers, a => a.Type.Id == filterRequest.OfferTypeId)
                    & Builders<Venue>.Filter.ElemMatch(a => a.Offers, a => a.IsActive == true)
                    & Builders<Venue>.Filter.ElemMatch(a => a.Offers, a => a.ExpiryDate >= UAEDateTime.Now.Date);

            var cityFilter = Builders<Venue>.Filter.Empty;
            if (filterRequest != null && !string.IsNullOrEmpty(filterRequest.CityId))
                cityFilter = Builders<Venue>.Filter.Eq(a => a.Location.City.Id, filterRequest.CityId);

            var areaFilter = Builders<Venue>.Filter.Empty;
            if (filterRequest != null && filterRequest.Areas != null && filterRequest.Areas.Any())
                areaFilter = Builders<Venue>.Filter.In(a => a.Location.Area, filterRequest.Areas);

            var dateFilter = Builders<Venue>.Filter.Empty;
            if (filterRequest != null && filterRequest.From != null && filterRequest.To != null)
                dateFilter = VenueDateRangeFilter(filterRequest.From.Value, filterRequest.To.Value);

            var nearFilter = LocationFilter(userLocation);

            var activeVenues = AvailableVenueFilter();

            var venueFilters = Builders<Venue>.Filter.And(activeVenues, searchFilter, categoryFilter, cityFilter, areaFilter, dateFilter, offerTypeFilter, nearFilter);

            return _collection.Find(venueFilters).ToListAsync();
        }

        public async Task<Page<Venue>> DashboardFilter(PaginationRequest paginationRequest, HomePageWebFilterationRequest filterRequest, List<string> accessibleVenues, bool isSuperAdmin)
        {
            var searchFilter = SearchFilter(filterRequest?.SearchQuery);

            var categoryFilter = Builders<Venue>.Filter.Empty;
            if (filterRequest != null && filterRequest.VenueCategories != null && filterRequest.VenueCategories.Any())
                categoryFilter = Builders<Venue>.Filter.Where(a => a.Categories.Any(a => filterRequest.VenueCategories.Contains(a.Id)));

            var offerTypeFilter = Builders<Venue>.Filter.Empty;
            if (filterRequest != null && !string.IsNullOrEmpty(filterRequest.OfferTypeId))
                offerTypeFilter = Builders<Venue>.Filter.ElemMatch(a => a.Offers, a => a.Type.Id == filterRequest.OfferTypeId);

            var cityFilter = Builders<Venue>.Filter.Empty;
            if (filterRequest != null && !string.IsNullOrEmpty(filterRequest.CityId))
                cityFilter = Builders<Venue>.Filter.Eq(a => a.Location.City.Id, filterRequest.CityId);

            var dateFilter = Builders<Venue>.Filter.Empty;
            if (filterRequest != null && filterRequest.From != null && filterRequest.To != null)
                dateFilter = VenueDateRangeFilter(filterRequest.From.Value, filterRequest.To.Value);

            var accessibleVenuesFilter = Builders<Venue>.Filter.InOrParameterEmpty(a => a.Id, accessibleVenues, isSuperAdmin);

            var venueFilters = Builders<Venue>.Filter.And(accessibleVenuesFilter, searchFilter, categoryFilter, cityFilter, dateFilter, offerTypeFilter);
            
            var records = await _collection.FindAsync(venueFilters, new FindOptions<Venue, Venue> { Collation = new Collation(locale: "en", strength: CollationStrength.Secondary) });
            return records.ToList().OrderBy(a => a.Name).GetPaged(paginationRequest);
        }

        private FilterDefinition<Venue> VenueDateRangeFilter(DateTime from, DateTime to)
        {
            return Builders<Venue>.Filter.IsAvailableInRangeDateOnly(a => a.OpenTimes, from, to);
        }

        public async Task<Page<Venue>> GetVenuesByUserId(PaginationRequest paginationRequest, string userId)
        {
            var filter = Builders<Venue>.Filter.Eq(a => a.CreatedBy, userId);
            var records = await _collection.FindAsync(filter, new FindOptions<Venue, Venue> { Collation = new Collation(locale: "en", strength: CollationStrength.Secondary) });
            return records.ToList().OrderBy(a => a.Name).GetPaged(paginationRequest);
        }

        public async Task<Page<Venue>> GetVenuesUserAdminOn(PaginationRequest paginationRequest, List<string> venuesIds)
        {
            var filter = Builders<Venue>.Filter.In(a => a.Id, venuesIds);
            var records = await _collection.FindAsync(filter, new FindOptions<Venue, Venue> { Collation = new Collation(locale: "en", strength: CollationStrength.Secondary) });
            return records.ToList().OrderBy(a => a.Name).GetPaged(paginationRequest);
        }

        public async Task<List<Venue>> GetNewestVenues(List<string> accessibleVenues, bool isSuperAdmin)
        {
            var accessibleVenuesFilter = Builders<Venue>.Filter.InOrParameterEmpty(a => a.Id, accessibleVenues, isSuperAdmin);

            return await _collection.Find(accessibleVenuesFilter & AvailableVenueFilter())
                                    .SortByDescending(a => a.CreationDate)
                                    .ThenBy(a => a.Name)
                                    .Limit(10)
                                    .ToListAsync();
        }

        public async Task<Page<Venue>> GetVenuesPage(PaginationRequest paginationRequest, FilterationRequest filterRequest, List<string> accessibleVenues, bool isSuperAdmin)
        {
            var accessibleVenuesFilter = Builders<Venue>.Filter.InOrParameterEmpty(a => a.Id, accessibleVenues, isSuperAdmin);
            var searchFilter = Builders<Venue>.Filter.Empty;
            if (!string.IsNullOrEmpty(filterRequest?.SearchQuery))
                searchFilter = Builders<Venue>.Filter.SearchContains(c => c.Name, filterRequest?.SearchQuery) |
                               Builders<Venue>.Filter.SearchContains(c => c.Location.City.Name, filterRequest?.SearchQuery);

            var collation = new Collation(locale: "en", strength: CollationStrength.Secondary);

            var sort = filterRequest.SortBy switch
            {
                Sort.Newest => Builders<Venue>.Sort.Descending(a => a.CreationDate).Ascending(a => a.Name),
                Sort.Alphabetical => Builders<Venue>.Sort.Ascending(a => a.Name),
                (_) => Builders<Venue>.Sort.Ascending(a => a.Name),
            };

            var records = await _collection.Aggregate(new AggregateOptions { Collation = collation, AllowDiskUse = true })
                                           .Match(accessibleVenuesFilter & searchFilter)
                                           .Sort(sort)
                                           .Skip(paginationRequest.PageNumber * paginationRequest.PageSize)
                                           .Limit(paginationRequest.PageSize)
                                           .ToListAsync();
            var recordsCount = _collection.Aggregate().Match(accessibleVenuesFilter & searchFilter).Count().FirstOrDefault()?.Count ?? 0;
            return new Page<Venue>(records, paginationRequest.PageNumber, paginationRequest.PageSize, recordsCount);
        }

        public async Task<bool> DeleteLocationFromVenue(string cityId, string area = null)
        {
            var filter = Builders<Venue>.Filter.Eq(a => a.Location.City.Id, cityId);
            var areaFilter = area == null ? Builders<Venue>.Filter.Empty :
                                            Builders<Venue>.Filter.Eq(a => a.Location.Area, area);

            var update = Builders<Venue>.Update.Set(a => a.Location, null);
            var updateResult = await _collection.UpdateManyAsync(filter & areaFilter, update);

            return updateResult.IsAcknowledged;
        }

        public async Task<bool> UpdateVenuesArea(string cityId, UpdateAreaRequest request)
        {
            var filter = Builders<Venue>.Filter.Eq(a => a.Location.City.Id, cityId) &
                         Builders<Venue>.Filter.Eq(a => a.Location.Area, request.OldArea);
            var update = Builders<Venue>.Update.Set(a => a.Location.Area, request.NewArea);
            var updateResult = await _collection.UpdateManyAsync(filter, update);
            return updateResult.IsAcknowledged;
        }

        public Task<List<Venue>> GetVenuesByCityId(string cityId, string area = null)
        {
            var filter = Builders<Venue>.Filter.Eq(a => a.Location.City.Id, cityId);
            var areaFilter = area == null ? Builders<Venue>.Filter.Empty : Builders<Venue>.Filter.Eq(a => a.Location.Area, area);
            return _collection.Find(filter & areaFilter).ToListAsync();
        }

        public async Task DeleteCategory(string categoryId)
        {
            var filter = Builders<Venue>.Filter.ElemMatch(a => a.Categories, a => a.Id == categoryId);
            var categoryFilter = Builders<Category>.Filter.Eq(a => a.Id, categoryId);
            var update = Builders<Venue>.Update.PullFilter(a => a.Categories, categoryFilter);
            await _collection.UpdateManyAsync(filter, update);
        }

        public async Task DeleteLoyalty(string loyaltyTypeId)
        {
            var filter = Builders<Venue>.Filter.Eq(a => a.Loyalty.Type.Id, loyaltyTypeId);
            var update = Builders<Venue>.Update.Set(a => a.Loyalty, null);
            await _collection.UpdateManyAsync(filter, update);
        }

        public async Task DeleteOffer(string offerTypeId)
        {
            var filter = Builders<Venue>.Filter.ElemMatch(a => a.Offers, a => a.Type.Id == offerTypeId);
            var typeFilter = Builders<Offer>.Filter.Eq(a => a.Type.Id, offerTypeId);
            var update = Builders<Venue>.Update.PullFilter(a => a.Offers, typeFilter);
            await _collection.UpdateManyAsync(filter, update);
        }

        public async Task<bool> UpdateTermsAndConditions(string id, List<string> selectedTermsAndConditions)
        {
            var filter = Builders<Venue>.Filter.Eq(a => a.Id, id);
            var update = Builders<Venue>.Update.Set(a => a.SelectedTermsAndConditions, selectedTermsAndConditions);
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged;
        }

        public async Task DeleteTermsAndConditions(string termsAndConditionsId)
        {
            var filter = Builders<Venue>.Filter.Where(a => a.SelectedTermsAndConditions.Any(a => a == termsAndConditionsId));
            var update = Builders<Venue>.Update.Pull(a => a.SelectedTermsAndConditions, termsAndConditionsId);
            await _collection.UpdateManyAsync(filter, update);
        }

        public long GetAssignedLoyaltyCount(string loyaltyTypeId)
        {
            var filter = Builders<Venue>.Filter.Eq(a => a.Loyalty.Type.Id, loyaltyTypeId);
            return _collection.Find(filter).CountDocuments();
        }

        public async Task<bool> UpdateVenueCode(string id, string code)
        {
            var filter = Builders<Venue>.Filter.Eq(a => a.Id, id);
            var update = Builders<Venue>.Update.Set(a => a.LoyaltyCode, code).Set(a => a.OffersCode, code);
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged;
        }

        public async Task<bool> UpdateVenueStatus(string venueId, Availability status)
        {
            var filter = Builders<Venue>.Filter.Eq(a => a.Id, venueId);
            var update = Builders<Venue>.Update.Set(a => a.Status, status);
            var result = await _collection.UpdateManyAsync(filter, update);
            return result.IsAcknowledged;
        }

        public async Task<Page<Venue>> GetVenuesWithAssignedLoyalty(PaginationRequest paginationRequest, FilterationRequest filterRequest, List<string> accessibleVenues, bool isSuperAdmin)
        {
            var accessibleVenuesFilter = Builders<Venue>.Filter.InOrParameterEmpty(a => a.Id, accessibleVenues, isSuperAdmin);

            var searchFilter = Builders<Venue>.Filter.Empty;
            if (!string.IsNullOrEmpty(filterRequest?.SearchQuery))
                searchFilter = Builders<Venue>.Filter.SearchContains(c => c.Name, filterRequest?.SearchQuery) |
                               Builders<Venue>.Filter.SearchContains(c => c.Loyalty.Type.Name, filterRequest?.SearchQuery);

            var venueFilter = Builders<Venue>.Filter.Ne(a => a.Loyalty, null);

            var collation = new Collation(locale: "en", strength: CollationStrength.Secondary);

            var sort = filterRequest.SortBy switch
            {
                Sort.Newest => Builders<Venue>.Sort.Descending(a => a.Loyalty.AssignDate).Ascending(a => a.Name),
                Sort.Alphabetical => Builders<Venue>.Sort.Ascending(a => a.Name),
                (_) => Builders<Venue>.Sort.Ascending(a => a.Name),
            };

            var records = await _collection.Aggregate(new AggregateOptions { Collation = collation, AllowDiskUse = true })
                                           .Match(searchFilter & venueFilter & accessibleVenuesFilter)
                                           .Sort(sort)
                                           .Skip(paginationRequest.PageNumber * paginationRequest.PageSize)
                                           .Limit(paginationRequest.PageSize)
                                           .ToListAsync();
            var recordsCount = _collection.Aggregate().Match(searchFilter & venueFilter & accessibleVenuesFilter).Count().FirstOrDefault()?.Count ?? 0;
            return new Page<Venue>(records, paginationRequest.PageNumber, paginationRequest.PageSize, recordsCount);
        }

        public async Task<Venue> AssignLoyalty(string id, Loyalty loyalty)
        {
            var filter = Builders<Venue>.Filter.Eq(a => a.Id, id);
            var update = Builders<Venue>.Update.Set(a => a.Loyalty, loyalty);
            return await _collection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<Venue, Venue> { ReturnDocument = ReturnDocument.After });
        }

        public async Task<bool> UnAssignLoyalty(string id)
        {
            var filter = Builders<Venue>.Filter.Eq(a => a.Id, id);
            var update = Builders<Venue>.Update.Set(a => a.Loyalty, null);
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged;
        }

        public async Task<bool> UpdateAssignedLoyalty(string id, Loyalty loyalty)
        {
            var oldLoyalty = _collection.Find(a => a.Loyalty.Id == loyalty.Id).FirstOrDefault().Loyalty;

            var filter = Builders<Venue>.Filter.Eq(a => a.Id, id) &
                         Builders<Venue>.Filter.Eq(a => a.Loyalty.Id, loyalty.Id);

            var update = Builders<Venue>.Update.Set(a => a.Loyalty, loyalty);
            var result = await _collection.UpdateOneAsync(filter, update);

            if (result.IsAcknowledged)
                await SyncWithUserLoyalty(oldLoyalty, loyalty);

            return result.IsAcknowledged;
        }

        public Task SyncWithUserLoyalty(Loyalty oldLoyalty, Loyalty updatedLoyalty)
        {
            if (oldLoyalty?.MaxUsage != updatedLoyalty.MaxUsage || oldLoyalty?.ValidOn != updatedLoyalty.ValidOn || oldLoyalty?.IsActive != updatedLoyalty.IsActive)
            {
                var loyaltyIdFilter = Builders<UserLoyalty>.Filter.Eq(v => v.Loyalty.Id, updatedLoyalty.Id);
                var updateDefinition = Builders<UserLoyalty>.Update.Set(v => v.Loyalty.MaxUsage, updatedLoyalty.MaxUsage)
                                                                   .Set(v => v.Loyalty.ValidOn, updatedLoyalty.ValidOn)
                                                                   .Set(v => v.Loyalty.IsActive, updatedLoyalty.IsActive);
                return _userLoyaltyCollection.UpdateManyAsync(loyaltyIdFilter, updateDefinition);
            }
            return Task.CompletedTask;
        }

        public Task<List<Venue>> GetActiveVenuesWithNoLoyalty(List<string> accessibleVenues, bool isSuperAdmin)
        {
            var filter = Builders<Venue>.Filter.Eq(a => a.Status, Availability.Active) &
                         Builders<Venue>.Filter.Eq(a => a.Loyalty, null) &
                         Builders<Venue>.Filter.InOrParameterEmpty(a => a.Id, accessibleVenues, isSuperAdmin);
            return _collection.Find(filter).SortBy(a => a.Name).ToListAsync();
        }
        public Task<List<Venue>> GetActiveVenuesWithNoLoyaltyToAllAdmins()
        {
            var filter = Builders<Venue>.Filter.Eq(a => a.Status, Availability.Active) &
                         Builders<Venue>.Filter.Eq(a => a.Loyalty, null);
            return _collection.Find(filter).SortBy(a => a.Name).ToListAsync();
        }
        public async Task<bool> AddEventToVenue(string venueId, string eventId)
        {
            var filter = Builders<Venue>.Filter.Eq(a => a.Id, venueId);
            var update = Builders<Venue>.Update.Push(a => a.Events, eventId);
            var updateResult = await _collection.UpdateOneAsync(filter, update);
            return updateResult.IsAcknowledged;
        }

        public async Task<bool> RemoveEventFromVenue(string venueId, string eventId)
        {
            var filter = Builders<Venue>.Filter.Eq(a => a.Id, venueId);
            var update = Builders<Venue>.Update.Pull(a => a.Events, eventId);
            var updateResult = await _collection.UpdateOneAsync(filter, update);
            return updateResult.IsAcknowledged;
        }

        public async Task<bool> RemoveEventFromOldAssignedVenues(string newAssignedVenueId, string eventId)
        {
            var filter = Builders<Venue>.Filter.Ne(a => a.Id, newAssignedVenueId);
            var update = Builders<Venue>.Update.Pull(a => a.Events, eventId);
            var updateResult = await _collection.UpdateManyAsync(filter, update);
            return updateResult.IsAcknowledged;
        }

        public async Task<bool> UpdateAssignedLoyaltyStatus(string id, bool isActive)
        {
            var filter = Builders<Venue>.Filter.Eq(a => a.Id, id);
            var update = Builders<Venue>.Update.Set(a => a.Loyalty.IsActive, isActive);
            var updateResult = await _collection.UpdateOneAsync(filter, update);
            return updateResult.IsAcknowledged;
        }

        public async Task<bool> UpdateAssignedOffersStatus(string id, bool isActive)
        {
            var filter = Builders<Venue>.Filter.Eq(a => a.Id, id);
            var update = Builders<Venue>.Update.Set("Offers.$[i].IsActive", isActive);
            var arrayFilters = new List<ArrayFilterDefinition> { new BsonDocumentArrayFilterDefinition<Venue>(new BsonDocument("i.IsActive", new BsonDocument("$exists", "true"))) };
            var options = new UpdateOptions { ArrayFilters = arrayFilters };
            var updateResult = await _collection.UpdateOneAsync(filter, update, options);
            return updateResult.IsAcknowledged;
        }

        public async Task<bool> DeleteGalleryImages(string id, List<string> images)
        {
            var filter = Builders<Venue>.Filter.Eq(a => a.Id, id);
            var update = Builders<Venue>.Update.PullAll(a => a.Gallery, images);
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged;
        }

        public Venue GetVenueById(string id) => _collection.Find(a => a.Id == id).FirstOrDefault();
        public List<Venue> GetVenuesByIds(List<string> ids) => _collection.Find(a => ids.Contains(a.Id)).ToList();

        public async Task<bool> UnAssignAllLoyalty()
        {
            var venueFilter = Builders<Venue>.Filter.Exists(a => a.Id);
            var update = Builders<Venue>.Update.Set(a => a.Loyalty, null);
            var result = await _collection.UpdateManyAsync(venueFilter, update);
            return result.IsAcknowledged;
        }

        public async Task<bool> UnAssignAllOffers()
        {
            var venueFilter = Builders<Venue>.Filter.Exists(a => a.Id);
            var update = Builders<Venue>.Update.Set(a => a.Offers, new List<Offer>());
            var result = await _collection.UpdateManyAsync(venueFilter, update);
            return result.IsAcknowledged;
        }
    }
}
