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
using OutOut.ViewModels.Requests.HomePage;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Requests.Offers;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Services
{
    public class OfferRepository : IOfferRepository
    {
        private readonly ApplicationNonSqlDbContext _dbContext;
        public OfferRepository(ApplicationNonSqlDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private IMongoCollection<Venue> _collection { get { return _dbContext.GetCollection<Venue>(); } }
        private IMongoCollection<UserOffer> _userOfferCollection { get { return _dbContext.GetCollection<UserOffer>(); } }

        private FilterDefinition<Venue> LocationFilter(UserLocation userLocation)
        {
            var point = GeoJson.Point(GeoJson.Geographic(userLocation.GeoPoint.Coordinates.Longitude, userLocation.GeoPoint.Coordinates.Latitude));
            return Builders<Venue>.Filter.Near(e => e.Location.GeoPoint, point);
        }

        private FilterDefinition<VenueOneOfferWithDistance> TypeFilter(string searchQuery)
        {
            var filter = Builders<VenueOneOfferWithDistance>.Filter.Empty;
            if (!string.IsNullOrEmpty(searchQuery))
                filter = Builders<VenueOneOfferWithDistance>.Filter.SearchContains(v => v.Offer.Type.Name, searchQuery) |
                         Builders<VenueOneOfferWithDistance>.Filter.SearchContains(v => v.Name, searchQuery);
            return filter;
        }

        private FilterDefinition<VenueOneOffer> TypeFilter(HomePageWebFilterationRequest filterRequest)
        {
            var filter = Builders<VenueOneOffer>.Filter.Empty;
            if (filterRequest != null && !string.IsNullOrEmpty(filterRequest.SearchQuery))
                filter = Builders<VenueOneOffer>.Filter.SearchContains(v => v.Offer.Type.Name, filterRequest.SearchQuery) |
                         Builders<VenueOneOffer>.Filter.SearchContains(v => v.Name, filterRequest.SearchQuery);
            return filter;
        }

        private FilterDefinition<VenueOneOfferWithDistance> AvailableVenueFilter() =>
            Builders<VenueOneOfferWithDistance>.Filter.Eq(a => a.Status, Availability.Active);

        private FilterDefinition<VenueOneOfferWithDistance> ActiveOfferFilter() =>
            Builders<VenueOneOfferWithDistance>.Filter.Eq(v => v.Offer.IsActive, true);

        private FilterDefinition<VenueOneOfferWithDistance> NonExpiredOfferFilter() =>
            Builders<VenueOneOfferWithDistance>.Filter.Gte(v => v.Offer.ExpiryDate, UAEDateTime.Now.Date);

        private FilterDefinition<VenueOneOfferWithDistance> VenueOnValidTimeFilter() =>
            Builders<VenueOneOfferWithDistance>.Filter.ElemMatch(v => v.OpenTimes, AvailableTimeFilters.IsCurrentlyAvailable());

        private FilterDefinition<VenueOneOfferWithDistance> OfferOnValidTimeFilter() =>
            Builders<VenueOneOfferWithDistance>.Filter.ElemMatch(o => o.Offer.ValidOn, AvailableTimeFilters.IsCurrentlyAvailable());
        private FilterDefinition<VenueOneOffer> VenueOneOfferOnValidTimeFilter() =>
            Builders<VenueOneOffer>.Filter.ElemMatch(o => o.Offer.ValidOn, AvailableTimeFilters.IsCurrentlyAvailable());

        private FilterDefinition<VenueOneOfferWithDistance> UserOffersRedeemsFilter(string userId) =>
            !Builders<VenueOneOfferWithDistance>.Filter.ElemMatch(u => u.UserOffers, Builders<UserOffer>.Filter.Eq(u => u.UserId, userId) & Builders<UserOffer>.Filter.Eq(u => u.Day, UAEDateTime.Now.Date)) |
            Builders<VenueOneOfferWithDistance>.Filter.Size(u => u.UserOffers, 0);


        /// <summary>
        /// get active offers and non expired for upcoming or avilable offers 
        /// </summary>
        /// <param name="paginationRequest"></param>
        /// <param name="userLocation"></param>
        /// <param name="filterRequest"></param>
        /// <param name="getUpcoming"></param>
        /// <returns></returns>
        public Task<Page<VenueOneOfferWithDistance>> GetActiveNonExpiredOffers(PaginationRequest paginationRequest, UserLocation userLocation, OfferFilterationRequest filterRequest, string userId, bool getUpcoming = false)
        {
            var searchFilter = TypeFilter(filterRequest?.SearchQuery);

            var geoNearOptions = new BsonDocument
                            {
                                {"spherical", true},
                                {"allowDiskUse",true},
                                {"near",new BsonArray(new double[] {
                                    userLocation.GeoPoint.Coordinates.Longitude,
                                    userLocation.GeoPoint.Coordinates.Latitude
                                })},
                                {"distanceField","Distance"},
                                {"distanceMultiplier", GenericConstants.EarthRadiusInKm} // Returns distance in kilometers by multiplying by radius of earth
                            };

            var lookUpFromCollection = _dbContext.GetCollection<UserOffer>();


            var offersFilter = getUpcoming ? !OfferOnValidTimeFilter() : OfferOnValidTimeFilter();

            var query = _collection.Aggregate()
                                   .AppendStage(new BsonDocumentPipelineStageDefinition<Venue, VenueAllOffersWithDistance>(new BsonDocument {
                                        { "$geoNear", geoNearOptions }
                                    }))
                                   .Unwind<VenueAllOffersWithDistance, VenueOneOfferWithDistance>(v => v.Offers)
                                   .Match(searchFilter)
                                   .Match(ActiveOfferFilter())
                                   .Match(NonExpiredOfferFilter())
                                   .Match(VenueOnValidTimeFilter())
                                   .Match(offersFilter)
                                   .Match(AvailableVenueFilter())
                                   .Lookup<VenueOneOfferWithDistance, UserOffer, VenueOneOfferWithDistance>(lookUpFromCollection,
                                                                                                            venue => venue.Offer.Id,
                                                                                                            userOffer => userOffer.Offer.Id,
                                                                                                            result => result.UserOffers)
                                   .Match(UserOffersRedeemsFilter(userId));

            return query.GetPaged(paginationRequest);
        }

        public Task<List<VenueOneOfferWithDistance>> HomeFilter(UserLocation userLocation, HomePageFilterationRequest filterRequest)
        {
            var searchFilter = TypeFilter(filterRequest?.SearchQuery);

            var typeFilter = Builders<VenueOneOfferWithDistance>.Filter.Empty;
            if (filterRequest != null && !string.IsNullOrEmpty(filterRequest.OfferTypeId))
                typeFilter = Builders<VenueOneOfferWithDistance>.Filter.Eq(a => a.Offer.Type.Id, filterRequest.OfferTypeId);

            var cityFilter = Builders<VenueOneOfferWithDistance>.Filter.Empty;
            if (filterRequest != null && !string.IsNullOrEmpty(filterRequest.CityId))
                cityFilter = Builders<VenueOneOfferWithDistance>.Filter.Eq(a => a.Location.City.Id, filterRequest.CityId);

            var areaFilter = Builders<VenueOneOfferWithDistance>.Filter.Empty;
            if (filterRequest != null && filterRequest.Areas != null && filterRequest.Areas.Any())
                areaFilter = Builders<VenueOneOfferWithDistance>.Filter.In(a => a.Location.Area, filterRequest.Areas);

            var venueCategoryFilter = Builders<VenueOneOfferWithDistance>.Filter.Empty;
            if (filterRequest != null && filterRequest.VenueCategories != null && filterRequest.VenueCategories.Any())
                venueCategoryFilter = Builders<VenueOneOfferWithDistance>.Filter.Where(a => a.Categories.Any(c => filterRequest.VenueCategories.Contains(c.Id)));

            var dateFilter = Builders<VenueOneOfferWithDistance>.Filter.Empty;
            if (filterRequest != null && filterRequest.From != null && filterRequest.To != null)
            {
                var from = filterRequest.From.Value;
                var to = filterRequest.To.Value;

                dateFilter = Builders<VenueOneOfferWithDistance>.Filter.IsAvailableInRange(v => v.Offer.ValidOn, from, to) &
                             Builders<VenueOneOfferWithDistance>.Filter.IsAvailableInRange(a => a.OpenTimes, from, to);
            }
            var activeVenue = AvailableVenueFilter();

            var offersFilters = Builders<VenueOneOfferWithDistance>.Filter.And(activeVenue, searchFilter, typeFilter, cityFilter, areaFilter, dateFilter, venueCategoryFilter);

            var geoNearOptions = new BsonDocument
                            {
                                {"spherical", true},
                                {"allowDiskUse",true},
                                {"near",new BsonArray(new double[] {
                                    userLocation.GeoPoint.Coordinates.Longitude,
                                    userLocation.GeoPoint.Coordinates.Latitude
                                })},
                                {"distanceField","Distance"},
                                {"distanceMultiplier", GenericConstants.EarthRadiusInKm}
                            };

            return _collection.Aggregate()
                                  .AppendStage(new BsonDocumentPipelineStageDefinition<Venue, VenueAllOffersWithDistance>(new BsonDocument {
                                        { "$geoNear", geoNearOptions }
                                   }))
                                  .Unwind<VenueAllOffersWithDistance, VenueOneOfferWithDistance>(v => v.Offers)
                                  .Match(offersFilters)
                                  .Match(ActiveOfferFilter())
                                  .Match(NonExpiredOfferFilter())
                                  .ToListAsync();
        }

        public async Task<Page<VenueOneOffer>> DashboardFilter(PaginationRequest paginationRequest, HomePageWebFilterationRequest filterRequest, List<string> accessibleVenues, bool isSuperAdmin)
        {
            var searchFilter = TypeFilter(filterRequest);

            var typeFilter = Builders<VenueOneOffer>.Filter.Empty;
            if (filterRequest != null && !string.IsNullOrEmpty(filterRequest.OfferTypeId))
                typeFilter = Builders<VenueOneOffer>.Filter.Eq(a => a.Offer.Type.Id, filterRequest.OfferTypeId);

            var cityFilter = Builders<VenueOneOffer>.Filter.Empty;
            if (filterRequest != null && !string.IsNullOrEmpty(filterRequest.CityId))
                cityFilter = Builders<VenueOneOffer>.Filter.Eq(a => a.Location.City.Id, filterRequest.CityId);

            var venueCategoryFilter = Builders<VenueOneOffer>.Filter.Empty;
            if (filterRequest != null && filterRequest.VenueCategories != null && filterRequest.VenueCategories.Any())
                venueCategoryFilter = Builders<VenueOneOffer>.Filter.Where(a => a.Categories.Any(c => filterRequest.VenueCategories.Contains(c.Id)));

            var dateFilter = Builders<VenueOneOffer>.Filter.Empty;
            var venueDateFilter = Builders<Venue>.Filter.Empty;
            if (filterRequest != null && filterRequest.From != null && filterRequest.To != null)
            {
                var from = filterRequest.From.Value;
                var to = filterRequest.To.Value;

                dateFilter = Builders<VenueOneOffer>.Filter.IsAvailableInRangeDateOnly(v => v.Offer.ValidOn, from, to) &
                             Builders<VenueOneOffer>.Filter.IsAvailableInRangeDateOnly(a => a.OpenTimes, from, to);
            }

            var accessibleVenuesFilter = Builders<Venue>.Filter.InOrParameterEmpty(a => a.Id, accessibleVenues, isSuperAdmin);

            var offersFilters = Builders<VenueOneOffer>.Filter.And(searchFilter, typeFilter, cityFilter, dateFilter, venueCategoryFilter);

            var records = await _collection.Aggregate(new AggregateOptions { Collation = new Collation(locale: "en", strength: CollationStrength.Secondary) })
                                            .Match(accessibleVenuesFilter)
                                            .Match(venueDateFilter)
                                            .Unwind<Venue, VenueOneOffer>(v => v.Offers)
                                            .Match(offersFilters)
                                            .SortBy(a => a.Name)
                                            .ToListAsync();
            return records.GetPaged(paginationRequest);
        }

        public async Task<List<VenueOneOffer>> GetNewestOffers(List<string> accessibleVenues, bool isSuperAdmin)
        {
            var filter = Builders<VenueOneOffer>.Filter.Eq(a => a.Offer.IsActive, true) &
                         Builders<VenueOneOffer>.Filter.Gte(a => a.Offer.ExpiryDate, UAEDateTime.Now.Date);

            var accessibleVenuesFilter = Builders<Venue>.Filter.InOrParameterEmpty(a => a.Id, accessibleVenues, isSuperAdmin);

            return await _collection.Aggregate(new AggregateOptions { Collation = new Collation(locale: "en", strength: CollationStrength.Secondary) })
                                  .Match(accessibleVenuesFilter)
                                  .Unwind<Venue, VenueOneOffer>(v => v.Offers)
                                  .Match(filter)
                                  .SortByDescending(a => a.Offer.AssignDate)
                                  .ThenBy(a => a.Offer.Type.Name)
                                  .ToListAsync();
        }

        public Task<VenueOneOffer> GetOfferById(string offerId)
        {
            var filter = Builders<VenueOneOffer>.Filter.Eq(a => a.Offer.Id, offerId);
            return _collection.Aggregate()
                               .Unwind<Venue, VenueOneOffer>(v => v.Offers)
                               .Match(filter)
                               .FirstOrDefaultAsync();
        }

        public Task<List<VenueOneOffer>> GetOfferByVenueId(string venueId)
        {
            var filter = Builders<VenueOneOffer>.Filter.Eq(a => a.Id, venueId);
            return _collection.Aggregate()
                               .Unwind<Venue, VenueOneOffer>(v => v.Offers)
                               .Match(filter)
                               .SortBy(a => a.Offer.ExpiryDate)
                               .ThenBy(a => a.Offer.Type.Name)
                               .ToListAsync();
        }

        public long GetAssignedOffersCount(string offerTypeId)
        {
            var filter = Builders<VenueOneOffer>.Filter.Eq(a => a.Offer.Type.Id, offerTypeId);
            var result = _collection.Aggregate()
                               .Unwind<Venue, VenueOneOffer>(v => v.Offers)
                               .Match(filter)
                               .Count()
                               .FirstOrDefault();
            return result == null ? 0 : result.Count;
        }

        public async Task<Page<VenueOneOffer>> GetAssignedOffersPage(PaginationRequest paginationRequest, FilterationRequest filterRequest, List<string> accessibleVenues, bool isSuperAdmin, bool getUpcoming = false)
        {
            var venueFilter = Builders<Venue>.Filter.InOrParameterEmpty(a => a.Id, accessibleVenues, isSuperAdmin) &
                              Builders<Venue>.Filter.SizeGte(a => a.Offers, 1);

            var upcomingFilter = Builders<VenueOneOffer>.Filter.Empty;
            var searchFilter = Builders<VenueOneOffer>.Filter.Empty;

            if (getUpcoming)
            {
                upcomingFilter = !VenueOneOfferOnValidTimeFilter();
                //upcomingFilter = Builders<VenueOneOffer>.Filter.Eq(c => c.Offer.IsActive, true)
                //& Builders<VenueOneOffer>.Filter.Gt(c => c.Offer.ExpiryDate, UAEDateTime.Now)
                //& (Builders<VenueOneOffer>.Filter.ElemMatch(c => c.Offer.ValidOn, !Builders<AvailableTime>.Filter.AnyEq(l => l.Days, UAEDateTime.Now.DayOfWeek))
                //| Builders<VenueOneOffer>.Filter.ElemMatch(c => c.Offer.ValidOn, Builders<AvailableTime>.Filter.Gt(l => l.From, UAEDateTime.Now.TimeOfDay)));
            }

            if (!string.IsNullOrEmpty(filterRequest?.SearchQuery))
                searchFilter = Builders<VenueOneOffer>.Filter.SearchContains(c => c.Name, filterRequest?.SearchQuery) |
                               Builders<VenueOneOffer>.Filter.SearchContains(c => c.Offer.Type.Name, filterRequest?.SearchQuery);

            var sort = filterRequest.SortBy switch
            {
                Sort.Newest => Builders<VenueOneOffer>.Sort.Descending(a => a.Offer.AssignDate).Ascending(a => a.Name),
                Sort.Alphabetical => Builders<VenueOneOffer>.Sort.Ascending(a => a.Name),
                (_) => Builders<VenueOneOffer>.Sort.Ascending(a => a.Name),
            };

            var records = await _collection.Aggregate(new AggregateOptions { Collation = new Collation(locale: "en", strength: CollationStrength.Secondary), AllowDiskUse = true })
                                  .Match(venueFilter)
                                  .Unwind<Venue, VenueOneOffer>(v => v.Offers)
                                  .Match(searchFilter)
                                  .Match(upcomingFilter)
                                  .Sort(sort)
                                  .Skip(paginationRequest.PageNumber * paginationRequest.PageSize)
                                  .Limit(paginationRequest.PageSize)
                                  .ToListAsync();

            var recordsCount = _collection.Aggregate().Match(venueFilter).Unwind<Venue, VenueOneOffer>(v => v.Offers).Match(searchFilter).Match(upcomingFilter).Count().FirstOrDefault()?.Count ?? 0;
            return new Page<VenueOneOffer>(records, paginationRequest.PageNumber, paginationRequest.PageSize, recordsCount);
        }
        public async Task<Page<VenueOneOffer>> GetAssignedUpcomingOffersPage(PaginationRequest paginationRequest, FilterationRequest filterRequest, List<string> accessibleVenues, bool isSuperAdmin)
        {
            var records = (await GetAssignedOffersPage(PaginationRequest.Max, filterRequest, accessibleVenues, isSuperAdmin)).Records;
            records = records.Where(l => l.Offer.IsActive).ToList();
            records = records.Where(l => l.Offer.ExpiryDate.AddDays(1).AddSeconds(-1) > UAEDateTime.Now).ToList();
            records = records.Where(l => !l.Offer.ValidOn.ToList().Any(l => l.Days.Contains(UAEDateTime.Now.DayOfWeek))
            || l.Offer.ValidOn.Any(l => l.From > UAEDateTime.Now.TimeOfDay)
            || l.Offer.ValidOn.Any(l => l.To < UAEDateTime.Now.TimeOfDay)).ToList();
            records = records.Skip(paginationRequest.PageNumber * paginationRequest.PageSize).Take(paginationRequest.PageSize).ToList();
            return new Page<VenueOneOffer>(records, paginationRequest.PageNumber, paginationRequest.PageSize, records.Count);

        }

        public async Task<VenueOneOffer> AssignOffer(string id, Offer offer)
        {
            var filter = Builders<Venue>.Filter.Eq(a => a.Id, id);
            var update = Builders<Venue>.Update.Push(a => a.Offers, offer);
            await _collection.FindOneAndUpdateAsync(filter, update);
            return await GetOfferById(offer.Id);
        }

        public async Task<bool> UnAssignOffer(string id, string offerId)
        {
            var filter = Builders<Venue>.Filter.Eq(a => a.Id, id);
            var offerFilter = Builders<Offer>.Filter.Eq(a => a.Id, offerId);
            var update = Builders<Venue>.Update.PullFilter(a => a.Offers, offerFilter);
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged;
        }

        public async Task<bool> UnAssignOffersFromVenue(string id)
        {
            var filter = Builders<Venue>.Filter.Eq(a => a.Id, id);
            var update = Builders<Venue>.Update.Set(a => a.Offers, new List<Offer>());
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged;
        }

        public async Task<bool> UpdateAssignedOffer(string id, Offer offer)
        {
            var oldOffer = _userOfferCollection.Find(a => a.Offer.Id == id)?.FirstOrDefault()?.Offer;

            var filter = Builders<Venue>.Filter.Eq(a => a.Id, id) &
                         Builders<Venue>.Filter.ElemMatch(a => a.Offers, a => a.Id == offer.Id);
            var update = Builders<Venue>.Update.Set(a => a.Offers[-1], offer);
            var updateResult = await _collection.UpdateOneAsync(filter, update);

            if (updateResult.IsAcknowledged)
                await SyncOffer(oldOffer, offer);

            return updateResult.IsAcknowledged;
        }

        public Task SyncOffer(Offer oldOffer, Offer updatedOffer)
        {
            if (oldOffer?.ExpiryDate != updatedOffer.ExpiryDate || oldOffer?.ValidOn != updatedOffer.ValidOn || oldOffer?.MaxUsagePerYear != updatedOffer.MaxUsagePerYear || oldOffer?.IsActive != updatedOffer.IsActive)
            {
                var filter = Builders<UserOffer>.Filter.Eq(a => a.Offer.Id, updatedOffer.Id);
                var update = Builders<UserOffer>.Update.Set(a => a.Offer.ExpiryDate, updatedOffer.ExpiryDate)
                                                       .Set(a => a.Offer.ValidOn, updatedOffer.ValidOn)
                                                       .Set(a => a.Offer.Type.Name, updatedOffer.Type.Name)
                                                       .Set(a => a.Offer.MaxUsagePerYear, updatedOffer.MaxUsagePerYear)
                                                       .Set(a => a.Offer.IsActive, updatedOffer.IsActive);
                return _userOfferCollection.UpdateManyAsync(filter, update);
            }

            return Task.CompletedTask;
        }
    }
}
