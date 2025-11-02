using OutOut.Models.Models;
using OutOut.Persistence.Data;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.Persistence.Services.Basic;
using OutOut.ViewModels.Wrappers;
using OutOut.ViewModels.Requests.Events;
using MongoDB.Bson;
using OutOut.Constants;
using MongoDB.Driver;
using OutOut.Models.Domains;
using OutOut.Persistence.Extensions;
using OutOut.ViewModels.Enums;
using OutOut.ViewModels.Requests.Venues;
using OutOut.ViewModels.Requests.HomePage;
using OutOut.ViewModels.Requests.Areas;
using OutOut.Constants.Enums;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.Models.Models.Embedded;
using OutOut.Models.Wrappers;
using OutOut.Models.Utils;

namespace OutOut.Persistence.Services
{
    public class EventRepository : GenericNonSqlRepository<Event>, IEventRepository
    {
        public EventRepository(ApplicationNonSqlDbContext dbContext, IEnumerable<ISyncRepository<Event>> syncRepositories) : base(dbContext, syncRepositories) { }

        public Event GetEventById(string id) => _collection.Find(a => a.Id == id).FirstOrDefault();

        public async Task<Event> CreateEventWithOccurrences(Event newEvent)
        {
            var occurrence = newEvent.Occurrences.FirstOrDefault();

            var daysCount = (occurrence.StartDate.AddDays(7) - occurrence.StartDate).Days;
            for (int i = 0; i < daysCount - 1; i++)
            {
                newEvent.Occurrences.Add(new EventOccurrence
                {
                    StartDate = newEvent.Occurrences.LastOrDefault().StartDate.AddDays(1),
                    EndDate = newEvent.Occurrences.LastOrDefault().EndDate.AddDays(1),
                    StartTime = new TimeSpan(16, 0, 0),
                    EndTime = new TimeSpan(20, 0, 0),
                    Packages = newEvent.Occurrences.LastOrDefault().Packages
                });
            }
            var weeksCount = (occurrence.StartDate.AddMonths(1) - occurrence.StartDate).Days / 7;
            for (int i = 0; i < weeksCount - 1; i++)
            {
                newEvent.Occurrences.Add(new EventOccurrence
                {
                    StartDate = newEvent.Occurrences.LastOrDefault().StartDate.AddDays(7),
                    EndDate = newEvent.Occurrences.LastOrDefault().EndDate.AddDays(7),
                    StartTime = new TimeSpan(16, 0, 0),
                    EndTime = new TimeSpan(20, 0, 0),
                    Packages = newEvent.Occurrences.LastOrDefault().Packages
                });
            }
            await _dbContext.GetCollection<Event>().InsertOneAsync(newEvent);
            return newEvent;
        }

        public async Task<Event> UpdatePackageRemainingTickets(string eventOccurrenceId, string packageId, int ticketsQuantity)
        {
            var filter = Builders<Event>.Filter.PackageFilter(a => a.Occurrences, eventOccurrenceId, packageId);
            var update = Builders<Event>.Update.Inc("Occurrences.$[i].Packages.$[j].RemainingTickets", ticketsQuantity);
            var arrayFilters = new List<ArrayFilterDefinition>
                {
                    new BsonDocumentArrayFilterDefinition<Event>(new BsonDocument("i.Id", new BsonDocument("$eq", new ObjectId(eventOccurrenceId)))),
                    new BsonDocumentArrayFilterDefinition<Event>(new BsonDocument("j._id", new BsonDocument("$eq", new ObjectId(packageId)))),
                };
            var options = new FindOneAndUpdateOptions<Event>() { ReturnDocument = ReturnDocument.After, ArrayFilters = arrayFilters };
            return await _collection.FindOneAndUpdateAsync(filter, update, options);
        }

        public async Task<Event> GetEventByOccurrenceId(string eventOccurrenceId)
        {
            var filter = Builders<Event>.Filter.ElemMatch(a => a.Occurrences, Builders<EventOccurrence>.Filter.ObjectIdEq("Id", eventOccurrenceId));
            return await FindFirst(filter);
        }

        public SingleEventOccurrence GetSingleEventOccurrenceById(string eventOccurrenceId)
        {
            var filter = Builders<Event>.Filter.ElemMatch(a => a.Occurrences, Builders<EventOccurrence>.Filter.ObjectIdEq("Id", eventOccurrenceId));
            return _collection.Aggregate()
                              .Match(filter)
                              .Unwind<Event, SingleEventOccurrence>(v => v.Occurrences)
                              .ToList()
                              .Where(a => a.Occurrence.Id == eventOccurrenceId)
                              .FirstOrDefault();
        }

        private FilterDefinition<SingleEventOccurrence> TodayEventsFilter(EventFilter? filterRequest)
        {
            var filter = Builders<SingleEventOccurrence>.Filter.Empty;
            if (filterRequest != null && filterRequest == EventFilter.Today)
                filter = Builders<SingleEventOccurrence>.Filter.IsAvailableToday(a => a.Occurrence);
            return filter;
        }

        private FilterDefinition<SingleEventOccurrence> FeaturedEventsFilter(EventFilter? filterRequest)
        {
            var filter = Builders<SingleEventOccurrence>.Filter.Empty;
            if (filterRequest != null && filterRequest == EventFilter.FeaturedEvents)
                filter = Builders<SingleEventOccurrence>.Filter.Eq(a => a.IsFeatured, true);
            return filter;
        }

        private FilterDefinition<SingleEventOccurrence> AvailableEventOccurrencesFilter() =>
            Builders<SingleEventOccurrence>.Filter.Eq(a => a.Status, Availability.Active);

        private FilterDefinition<SingleEventOccurrence> SearchOccurrenceFilter(string searchQuery)
        {
            var filter = Builders<SingleEventOccurrence>.Filter.Empty;
            if (!string.IsNullOrEmpty(searchQuery))
                filter = Builders<SingleEventOccurrence>.Filter.SearchContains(a => a.Name, searchQuery);
            return filter;
        }

        private FilterDefinition<Event> SearchFilter(string searchQuery)
        {
            var filter = Builders<Event>.Filter.Empty;
            if (!string.IsNullOrEmpty(searchQuery))
                filter = Builders<Event>.Filter.SearchContains(a => a.Name, searchQuery);
            return filter;
        }

        private FilterDefinition<SingleEventOccurrence> CategoriesFilter(EventFilterationRequest filterRequest)
        {
            var categoryFilter = Builders<SingleEventOccurrence>.Filter.Empty;
            if (filterRequest != null && filterRequest.CategoriesIds != null && filterRequest.CategoriesIds.Any())
                categoryFilter = Builders<SingleEventOccurrence>.Filter.Where(a => a.Categories.Any(a => filterRequest.CategoriesIds.Contains(a.Id)));
            return categoryFilter;
        }

        private FilterDefinition<SingleEventOccurrence> UpcomingDateFilter(EventFilter? request = null)
        {
            var filter = Builders<SingleEventOccurrence>.Filter.Empty;
            if (request == null || request != EventFilter.Today)
                filter = Builders<SingleEventOccurrence>.Filter.UpcomingDate(a => a.Occurrence);
            return filter;
        }

        public Task<List<SingleEventOccurrence>> GetEvents(EventFilterationRequest filterRequest, UserLocation userLocation)
        {
            var searchFilter = SearchOccurrenceFilter(filterRequest?.SearchQuery);
            var featuredEvents = FeaturedEventsFilter(filterRequest?.EventFilter);
            var todayEventsFilter = TodayEventsFilter(filterRequest?.EventFilter);
            var dateFilter = UpcomingDateFilter(filterRequest?.EventFilter);
            var categoriesFilter = CategoriesFilter(filterRequest);
            var activeFilter = AvailableEventOccurrencesFilter();

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

            var aggregateOption = new AggregateOptions { AllowDiskUse = true };
            var result = _collection.Aggregate(aggregateOption)
                                    .Unwind<Event, SingleEventOccurrence>(v => v.Occurrences)
                                    .Match(dateFilter & todayEventsFilter & featuredEvents & searchFilter & categoriesFilter & activeFilter);

            if (filterRequest != null && filterRequest.EventFilter == EventFilter.NearYou)
                return _collection.Aggregate(aggregateOption)
                                  .AppendStage(new BsonDocumentPipelineStageDefinition<Event, EventWithDistance>(new BsonDocument { { "$geoNear", geoNearOptions } }))
                                  .Unwind<EventWithDistance, SingleEventOccurrence>(v => v.Occurrences)
                                  .Match(dateFilter & searchFilter & categoriesFilter & activeFilter)
                                  .SortBy(a => a.Distance)
                                  .ThenBy(EventOccurrenceTimeFilters.GetAscendingDateTimeSort<SingleEventOccurrence>())
                                  .ToListAsync();

            else if (filterRequest != null && filterRequest.EventFilter == EventFilter.AllEvents)
                return _collection.Aggregate(new AggregateOptions { Collation = new Collation(locale: "en", strength: CollationStrength.Secondary), AllowDiskUse = true })
                                  .Unwind<Event, SingleEventOccurrence>(v => v.Occurrences)
                                  .Match(activeFilter & searchFilter & categoriesFilter & dateFilter & featuredEvents)
                                  .Sort(EventOccurrenceTimeFilters.GetAscendingDateTimeSort<SingleEventOccurrence>())
                                  .SortBy(a => a.Name)
                                  .ToListAsync();
            else
                return result.Sort(EventOccurrenceTimeFilters.GetAscendingDateTimeSort<SingleEventOccurrence>()).ToListAsync();
        }

        public Task<List<SingleEventOccurrence>> GetUpcomingEvents(List<string> eventIds)
        {
            var eventsFilter = Builders<Event>.Filter.In(a => a.Id, eventIds);
            var dateFilter = UpcomingDateFilter();
            var activeEvent = AvailableEventOccurrencesFilter();

            return _collection.Aggregate()
                .Match(eventsFilter)
                .Unwind<Event, SingleEventOccurrence>(v => v.Occurrences)
                .Match(dateFilter)
                .Match(activeEvent)
                .Sort(EventOccurrenceTimeFilters.GetAscendingDateTimeSort<SingleEventOccurrence>())
                .ToListAsync();
        }

        public Task<List<SingleEventOccurrence>> GetUpcomingEvents(List<string> accessibleEvents, bool isSuperAdmin)
        {
            var accessibleEventsFilter = Builders<Event>.Filter.InOrParameterEmpty(a => a.Id, accessibleEvents, isSuperAdmin);
            var dateFilter = UpcomingDateFilter();

            return _collection.Aggregate(new AggregateOptions { AllowDiskUse = true })
               .Match(accessibleEventsFilter)
               .Unwind<Event, SingleEventOccurrence>(v => v.Occurrences)
               .Match(dateFilter)
               .Match(AvailableEventOccurrencesFilter())
               .Sort(EventOccurrenceTimeFilters.GetAscendingDateTimeSort<SingleEventOccurrence>())
               .ToListAsync();
        }

        public Task<List<SingleEventOccurrence>> GetUpcomingEvents(List<string> eventIds, List<string> accessibleEvents, bool isSuperAdmin)
        {
            var accessibleEventsFilter = Builders<Event>.Filter.InOrParameterEmpty(a => a.Id, accessibleEvents, isSuperAdmin);
            var eventsFilter = Builders<Event>.Filter.In(a => a.Id, eventIds);
            var dateFilter = UpcomingDateFilter();

            return _collection.Aggregate(new AggregateOptions { AllowDiskUse = true })
               .Match(accessibleEventsFilter)
               .Match(eventsFilter)
               .Unwind<Event, SingleEventOccurrence>(a => a.Occurrences)
               .Match(dateFilter)
               .Sort(EventOccurrenceTimeFilters.GetAscendingDateTimeSort<SingleEventOccurrence>())
               .ToListAsync();
        }

        public async Task<List<Event>> GetAllEvents(SearchFilterationRequest searchFilterationRequest, List<string> accessibleEvents, bool isSuperAdmin)
        {
            var searchFilter = SearchFilter(searchFilterationRequest?.SearchQuery) & Builders<Event>.Filter.InOrParameterEmpty(a => a.Id, accessibleEvents, isSuperAdmin);
            var records = await _collection.FindAsync(searchFilter, new FindOptions<Event, Event> { Collation = new Collation(locale: "en", strength: CollationStrength.Secondary) });
            return records.ToList().OrderBy(a => a.Name).ToList();
        }
        public async Task<List<Event>> GetAllEventsNotAssociatedToVenue(string venueId, SearchFilterationRequest searchFilterationRequest, List<string> accessibleEvents, bool isSuperAdmin)
        {
            var searchFilter = SearchFilter(searchFilterationRequest?.SearchQuery) &
                Builders<Event>.Filter.InOrParameterEmpty(a => a.Id, accessibleEvents, isSuperAdmin) &
                (Builders<Event>.Filter.Eq(a => a.Venue, null) | (Builders<Event>.Filter.Eq(a => a.Venue.Id, venueId)));
            var records = await _collection.FindAsync(searchFilter, new FindOptions<Event, Event> { Collation = new Collation(locale: "en", strength: CollationStrength.Secondary) });
            return records.ToList().OrderBy(a => a.Name).ToList();
        }
        public async Task<List<SingleEventOccurrence>> GetUsersFavoriteEvents(List<string> eventOccurrenceIds, SearchFilterationRequest filterRequest)
        {
            var searchFilter = SearchFilter(filterRequest?.SearchQuery);
            var eventsFilter = Builders<SingleEventOccurrence>.Filter.ObjectIdIn("Occurrence.Id", eventOccurrenceIds);

            var records = await _collection.Aggregate(new AggregateOptions { AllowDiskUse = true })
                .Match(searchFilter)
                .Unwind<Event, SingleEventOccurrence>(a => a.Occurrences)
                .Match(eventsFilter)
                .ToListAsync();

            return records.OrderBy(v => eventOccurrenceIds.IndexOf(v.Occurrence.Id)).ToList();
        }

        public Task<List<SingleEventOccurrence>> HomeFilter(HomePageFilterationRequest filterRequest)
        {
            var searchFilter = Builders<SingleEventOccurrence>.Filter.Empty;
            if (!string.IsNullOrEmpty(filterRequest.SearchQuery))
                searchFilter = Builders<SingleEventOccurrence>.Filter.SearchContains(a => a.Name, filterRequest.SearchQuery);

            var categoryFilter = Builders<SingleEventOccurrence>.Filter.Empty;
            if (filterRequest != null && filterRequest.EventCategories != null && filterRequest.EventCategories.Any())
                categoryFilter = Builders<SingleEventOccurrence>.Filter.ElemMatch(a => a.Categories, c => filterRequest.EventCategories.Contains(c.Id));

            var cityFilter = Builders<SingleEventOccurrence>.Filter.Empty;
            if (filterRequest != null && !string.IsNullOrEmpty(filterRequest.CityId))
                cityFilter = Builders<SingleEventOccurrence>.Filter.Eq(a => a.Location.City.Id, filterRequest.CityId);

            var areaFilter = Builders<SingleEventOccurrence>.Filter.Empty;
            if (filterRequest != null && filterRequest.Areas != null && filterRequest.Areas.Any())
                areaFilter = Builders<SingleEventOccurrence>.Filter.In(a => a.Location.Area, filterRequest.Areas);

            var dateFilter = Builders<SingleEventOccurrence>.Filter.Empty;
            if (filterRequest != null && filterRequest.From != null && filterRequest.To != null)
                dateFilter = Builders<SingleEventOccurrence>.Filter.IsEventAvailableInRange(a => a.Occurrence, filterRequest.From.Value, filterRequest.To.Value);

            else if (filterRequest != null && filterRequest.From != null && filterRequest.To == null)
                dateFilter = Builders<SingleEventOccurrence>.Filter.AllUpcomingEvents(a => a.Occurrence, filterRequest.From.Value);

            var upcomingDateFilter = UpcomingDateFilter();

            var activeEvent = AvailableEventOccurrencesFilter();

            var eventFilters = filterRequest.From == null && filterRequest.To == null ?
                                    Builders<SingleEventOccurrence>.Filter.And(activeEvent, searchFilter, categoryFilter, cityFilter, areaFilter, upcomingDateFilter) :
                                    Builders<SingleEventOccurrence>.Filter.And(activeEvent, searchFilter, categoryFilter, cityFilter, areaFilter, dateFilter);

            return _collection.Aggregate(new AggregateOptions { AllowDiskUse = true })
                                 .Unwind<Event, SingleEventOccurrence>(v => v.Occurrences)
                                 .Match(eventFilters)
                                 .Sort(EventOccurrenceTimeFilters.GetAscendingDateTimeSort<SingleEventOccurrence>())
                                 .ToListAsync();
        }
        public async Task<List<SingleEventOccurrence>> DashboardFilter(HomePageWebFilterationRequest filterRequest, List<string> accessibleEvents, bool isSuperAdmin)
        {
            var searchFilter = Builders<SingleEventOccurrence>.Filter.Empty;
            if (!string.IsNullOrEmpty(filterRequest.SearchQuery))
                searchFilter = Builders<SingleEventOccurrence>.Filter.SearchContains(a => a.Name, filterRequest.SearchQuery);

            var categoryFilter = Builders<SingleEventOccurrence>.Filter.Empty;
            if (filterRequest != null && filterRequest.EventCategories != null && filterRequest.EventCategories.Any())
                categoryFilter = Builders<SingleEventOccurrence>.Filter.ElemMatch(a => a.Categories, c => filterRequest.EventCategories.Contains(c.Id));

            var featuredFilter = Builders<SingleEventOccurrence>.Filter.Empty;
            if (filterRequest != null && filterRequest.FeaturedEvents)
                featuredFilter = Builders<SingleEventOccurrence>.Filter.Eq(a => a.IsFeatured, true);

            var cityFilter = Builders<SingleEventOccurrence>.Filter.Empty;
            if (filterRequest != null && !string.IsNullOrEmpty(filterRequest.CityId))
                cityFilter = Builders<SingleEventOccurrence>.Filter.Eq(a => a.Location.City.Id, filterRequest.CityId);

            var dateFilter = Builders<SingleEventOccurrence>.Filter.Empty;
            if (filterRequest != null && filterRequest.From != null && filterRequest.To != null)
                dateFilter = Builders<SingleEventOccurrence>.Filter.IsEventAvailableInRange(a => a.Occurrence, filterRequest.From.Value, filterRequest.To.Value);

            else if (filterRequest != null && filterRequest.From != null && filterRequest.To == null)
                dateFilter = Builders<SingleEventOccurrence>.Filter.AllUpcomingEvents(a => a.Occurrence, filterRequest.From.Value);

            var eventFilters = Builders<SingleEventOccurrence>.Filter.And(searchFilter, categoryFilter, cityFilter, featuredFilter, dateFilter);

            var accessibleEventsFilter = Builders<Event>.Filter.InOrParameterEmpty(a => a.Id, accessibleEvents, isSuperAdmin);

            var records = await _collection.Aggregate(new AggregateOptions { Collation = new Collation(locale: "en", strength: CollationStrength.Secondary), AllowDiskUse = true })
                              .Match(accessibleEventsFilter)
                              .Unwind<Event, SingleEventOccurrence>(v => v.Occurrences)
                              .Match(eventFilters)
                              .Sort(EventOccurrenceTimeFilters.GetAscendingDateTimeSort<SingleEventOccurrence>())
                              .ToListAsync();
            return GetNearestOccurrencesInDate(records);
        }

        public async Task<Page<SingleEventOccurrence>> GetEventsByUserId(PaginationRequest paginationRequest, string userId)
        {
            var filter = Builders<SingleEventOccurrence>.Filter.Eq(a => a.CreatedBy, userId);
            var records = await _collection.Aggregate(new AggregateOptions { AllowDiskUse = true })
                                    .Unwind<Event, SingleEventOccurrence>(v => v.Occurrences)
                                    .Match(filter)
                                    .Sort(EventOccurrenceTimeFilters.GetAscendingDateTimeSort<SingleEventOccurrence>())
                                    .ToListAsync();
            var result = GetNearestOccurrencesInDate(records);
            return result.GetPaged(paginationRequest);
        }

        public async Task<List<SingleEventOccurrence>> GetOngoingEvents(List<string> accessibleEvents, bool isSuperAdmin)
        {
            var filter = Builders<SingleEventOccurrence>.Filter.OngoingEvent(a => a.Occurrence) & AvailableEventOccurrencesFilter();
            var accessibleEventsFilter = Builders<Event>.Filter.InOrParameterEmpty(a => a.Id, accessibleEvents, isSuperAdmin);

            return await _collection.Aggregate()
                                .Match(accessibleEventsFilter)
                                .Unwind<Event, SingleEventOccurrence>(v => v.Occurrences)
                                .Match(filter)
                                .Sort(EventOccurrenceTimeFilters.GetDescendingDateTimeSort<SingleEventOccurrence>())
                                .ToListAsync();
        }

        public async Task<bool> UpdateEventsArea(string cityId, UpdateAreaRequest request)
        {
            var filter = Builders<Event>.Filter.Eq(a => a.Location.City.Id, cityId) &
                         Builders<Event>.Filter.Eq(a => a.Location.Area, request.OldArea);
            var update = Builders<Event>.Update.Set(a => a.Location.Area, request.NewArea);
            var updateResult = await _collection.UpdateManyAsync(filter, update);
            return updateResult.IsAcknowledged;
        }

        public async Task DeleteCategory(string categoryId)
        {
            var filter = Builders<Event>.Filter.ElemMatch(a => a.Categories, a => a.Id == categoryId);
            var categoryFilter = Builders<Category>.Filter.Eq(a => a.Id, categoryId);
            var update = Builders<Event>.Update.PullFilter(a => a.Categories, categoryFilter);
            await _collection.UpdateManyAsync(filter, update);
        }

        public async Task<bool> DeleteLocationFromEvent(string cityId, string area = null)
        {
            var filter = Builders<Event>.Filter.Eq(a => a.Location.City.Id, cityId);
            var areaFilter = area == null ? Builders<Event>.Filter.Empty : Builders<Event>.Filter.Eq(a => a.Location.Area, area);
            var update = Builders<Event>.Update.Set(a => a.Location, null);
            var updateResult = await _collection.UpdateManyAsync(filter & areaFilter, update);
            return updateResult.IsAcknowledged;
        }

        private FilterDefinition<SingleEventOccurrence> FeaturedEventFilter(bool isFeatured)
        {
            var featuredEventFilter = Builders<SingleEventOccurrence>.Filter.Empty;
            if (isFeatured)
                featuredEventFilter = Builders<SingleEventOccurrence>.Filter.Eq(a => a.IsFeatured, true);
            return featuredEventFilter;
        }

        public async Task<Page<SingleEventOccurrence>> GetAllEventsOccurrences(PaginationRequest paginationRequest, FilterationRequest filterRequest, List<string> accessibleEvents, bool isSuperAdmin)
        {
            var accessibleEventsFilter = Builders<SingleEventOccurrence>.Filter.InOrParameterEmpty(a => a.Id, accessibleEvents, isSuperAdmin);

            var searchFilter = Builders<SingleEventOccurrence>.Filter.Empty;
            if (!string.IsNullOrEmpty(filterRequest?.SearchQuery))
                searchFilter = Builders<SingleEventOccurrence>.Filter.SearchContains(a => a.Name, filterRequest?.SearchQuery) |
                               Builders<SingleEventOccurrence>.Filter.SearchContains(a => a.Location.City.Name, filterRequest?.SearchQuery);

            var records = await _collection.Aggregate(new AggregateOptions { Collation = new Collation(locale: "en", strength: CollationStrength.Secondary), AllowDiskUse = true })
                           .Unwind<Event, SingleEventOccurrence>(v => v.Occurrences)
                           .Match(accessibleEventsFilter)
                           .Match(searchFilter)
                           .ToListAsync();

            var groupedOccurrences = GetNearestOccurrencesInDate(records);
            return filterRequest.SortBy switch
            {
                Sort.Newest => groupedOccurrences.OrderByDescending(a => a.CreationDate).ThenBy(a => a.Name).GetPaged(paginationRequest),
                Sort.Alphabetical => groupedOccurrences.OrderBy(a => a.Name).GetPaged(paginationRequest),
                (_) => groupedOccurrences.OrderBy(a => a.Name).GetPaged(paginationRequest),
            };
        }

        public async Task<Page<SingleEventOccurrence>> GetFeaturedEventsPage(PaginationRequest paginationRequest, FilterationRequest filterRequest)
        {
            var searchFilter = SearchOccurrenceFilter(filterRequest?.SearchQuery);
            var featuredFilter = FeaturedEventFilter(true);

            var records = await _collection.Aggregate(new AggregateOptions { Collation = new Collation(locale: "en", strength: CollationStrength.Secondary), AllowDiskUse = true })
                           .Unwind<Event, SingleEventOccurrence>(v => v.Occurrences)
                           .Match(searchFilter)
                           .Match(featuredFilter)
                           .ToListAsync();

            var groupedOccurrences = GetNearestOccurrencesInDate(records);
            return filterRequest.SortBy switch
            {
                Sort.Newest => groupedOccurrences.OrderByDescending(a => a.CreationDate).ThenBy(a => a.Name).GetPaged(paginationRequest),
                Sort.Date => groupedOccurrences.OrderByDescending(a => a.Occurrence.StartDate).ThenByDescending(a => a.Occurrence.StartTime).ThenBy(a => a.Name).GetPaged(paginationRequest),
                Sort.Alphabetical => groupedOccurrences.OrderBy(a => a.Name).GetPaged(paginationRequest),
                (_) => groupedOccurrences.OrderBy(a => a.Name).GetPaged(paginationRequest),
            };
        }

        private List<SingleEventOccurrence> GetNearestOccurrencesInDate(List<SingleEventOccurrence> occurrences)
        {
            var orderedOccurrences = occurrences.OrderBy(a => a.Occurrence.StartDate).ThenBy(a => a.Occurrence.StartTime);
            var newEvents = orderedOccurrences.Where(a => a.Occurrence.GetStartDateTime() >= UAEDateTime.Now).GroupBy(a => a.Id).Select(a => a.FirstOrDefault());
            var oldEvents = orderedOccurrences.Where(a => !newEvents.Any(e => e.Id == a.Id)).GroupBy(a => a.Id).Select(a => a.LastOrDefault());
            return newEvents.Concat(oldEvents.OrderByDescending(a => a.Occurrence.GetStartDateTime())).ToList();
        }

        public async Task<bool> UpdateIsFeatured(string id, bool isFeatured)
        {
            var filter = Builders<Event>.Filter.Eq(a => a.Id, id);
            var update = Builders<Event>.Update.Set(a => a.IsFeatured, isFeatured);
            var updateResult = await _collection.UpdateOneAsync(filter, update);
            return updateResult.IsAcknowledged;
        }

        public async Task<bool> UnsetAllFeaturedEvents()
        {
            var filter = Builders<Event>.Filter.Exists(a => a.Id);
            var update = Builders<Event>.Update.Set(a => a.IsFeatured, false);
            var updateResult = await _collection.UpdateManyAsync(filter, update);
            return updateResult.IsAcknowledged;
        }

        public async Task<bool> UpdateEventCode(string id, string code)
        {
            var filter = Builders<Event>.Filter.Eq(a => a.Id, id);
            var update = Builders<Event>.Update.Set(a => a.Code, code);
            var updateResult = await _collection.UpdateOneAsync(filter, update);
            return updateResult.IsAcknowledged;
        }

        public async Task<bool> UpdateEventsStatus(List<string> ids, Availability status)
        {
            var filter = Builders<Event>.Filter.In(a => a.Id, ids);
            var update = Builders<Event>.Update.Set(a => a.Status, status);
            var updateResult = await _collection.UpdateManyAsync(filter, update);
            return updateResult.IsAcknowledged;
        }

        public Task<List<Event>> GetEventsByCityId(string cityId, string area = null)
        {
            var filter = Builders<Event>.Filter.Eq(a => a.Location.City.Id, cityId);
            var areaFilter = area == null ? Builders<Event>.Filter.Empty : Builders<Event>.Filter.Eq(a => a.Location.Area, area);
            return _collection.Find(filter & areaFilter).ToListAsync();
        }

        public async Task<bool> UpdateAssignedVenue(string id, VenueSummary venue = null)
        {
            var filter = Builders<Event>.Filter.Eq(a => a.Id, id);
            var update = Builders<Event>.Update.Set(a => a.Venue, venue);
            var updateResult = await _collection.UpdateOneAsync(filter, update);
            return updateResult.IsAcknowledged;
        }
    }
}
