using MongoDB.Driver;
using OutOut.Constants.Enums;
using OutOut.Models.Identity;
using OutOut.Models.Models;
using OutOut.Models.Models.Embedded;
using OutOut.Models.Utils;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Data;
using OutOut.Persistence.Extensions;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.Persistence.Providers;
using OutOut.Persistence.Services.Basic;
using OutOut.ViewModels.Requests.Bookings;
using OutOut.ViewModels.Requests.Customers;
using OutOut.ViewModels.Requests.Fitlers;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Requests.VenueBooking;
using OutOut.ViewModels.Responses.Bookings;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Services
{
    public class VenueBookingRepository : GenericNonSqlRepository<VenueBooking>, IVenueBookingRepository
    {
        protected readonly IUserDetailsProvider _userDetailsProvider;
        protected IMongoCollection<Venue> _venueCollection
        {
            get { return _dbContext.GetCollection<Venue>(); }
        }
        public VenueBookingRepository(ApplicationNonSqlDbContext dbContext, IEnumerable<ISyncRepository<VenueBooking>> syncRepositories, IUserDetailsProvider userDetailsProvider) : base(dbContext, syncRepositories)
        {
            _userDetailsProvider = userDetailsProvider;
        }

        public async Task<List<ApplicationUserSummary>> GetUsersByVenueIds(List<string> venueIds, FilterationRequest filterRequest = null)
        {
            var venuesFilter = Builders<VenueBooking>.Filter.In(a => a.Venue.Id, venueIds);

            var searchFilter = Builders<VenueBooking>.Filter.Empty;
            if (filterRequest != null && !string.IsNullOrEmpty(filterRequest.SearchQuery))
                searchFilter = Builders<VenueBooking>.Filter.SearchContains(a => a.User.FullName, filterRequest.SearchQuery);

            //var roleFilter = Builders<ApplicationUser>.Filter.Size(a => a.Roles, 0);

            return await _collection.Find(venuesFilter & searchFilter).Project(x => x.User).ToListAsync();
        }

        public async Task<VenueBooking> UpdateVenueBooking(string id, VenueBooking venueBooking)
        {
            venueBooking.LastModifiedDate = DateTime.UtcNow;
            venueBooking.ModifiedBy = _userDetailsProvider.UserId;

            VenueBooking oldEntity = null;
            if (_syncRepositories.Any())
            {
                oldEntity = await GetById(id);
            }

            var venueBookingFilter = Builders<VenueBooking>.Filter.Eq(a => a.Id, id);
            await _collection.ReplaceOneAsync(venueBookingFilter, venueBooking);

            if (_syncRepositories.Any())
            {
                await Sync(oldEntity, venueBooking);
            }
            return venueBooking;
        }

        private FilterDefinition<VenueBooking> SearchFilter(string searchQuery)
        {
            var searchFilter = Builders<VenueBooking>.Filter.Empty;
            if (!string.IsNullOrEmpty(searchQuery))
                searchFilter = Builders<VenueBooking>.Filter.SearchContains(c => c.Venue.Name, searchQuery);
            return searchFilter;
        }

        private FilterDefinition<VenueBooking> TimeFilter(MyBookingFilterationRequest filterRequest)
        {
            var timeFilter = Builders<VenueBooking>.Filter.Empty;

            if (filterRequest != null && filterRequest.MyBooking == MyBookingFilteration.History)
                timeFilter = Builders<VenueBooking>.Filter.Lte(a => a.Date, UAEDateTime.Now.AddHours(-24));

            else if (filterRequest != null && filterRequest.MyBooking == MyBookingFilteration.Recent)
                timeFilter = Builders<VenueBooking>.Filter.Gt(a => a.Date, UAEDateTime.Now.AddHours(-24));

            return timeFilter;
        }

        private FilterDefinition<VenueBooking> UserFilter(MyBookingFilterationRequest filterRequest, string userId)
        {
            var userFilter = Builders<VenueBooking>.Filter.Empty;
            if (filterRequest != null)
                userFilter = Builders<VenueBooking>.Filter.Eq(a => a.User.Id, userId);
            return userFilter;
        }

        public Task<Page<VenueBooking>> GetMyBooking(PaginationRequest paginationRequest, MyBookingFilterationRequest filterRequest, string userId)
        {
            var userFilter = UserFilter(filterRequest, userId);
            var searchFilter = SearchFilter(filterRequest?.SearchQuery);
            var timeFilter = TimeFilter(filterRequest);

            var venueBookingFilters = Builders<VenueBooking>.Filter.And(searchFilter & userFilter & timeFilter);

            var records = filterRequest.MyBooking == MyBookingFilteration.Recent ?
                _collection.Find(venueBookingFilters).SortBy(a => a.Date).ThenBy(a => a.Venue.Name) :
                _collection.Find(venueBookingFilters).SortByDescending(a => a.Date).ThenBy(a => a.Venue.Name);

            return records.GetPaged(paginationRequest);
        }

        public long GetApprovedBookingsCount(string venueId)
        {
            var filter = Builders<VenueBooking>.Filter.Eq(a => a.Venue.Id, venueId) &
                         Builders<VenueBooking>.Filter.Eq(a => a.Status, VenueBookingStatus.Approved);
            return _collection.Find(filter).CountDocuments();
        }

        public long GetAllBookingsCount(string venueId)
        {
            var filter = Builders<VenueBooking>.Filter.Eq(a => a.Venue.Id, venueId);
            return _collection.Find(filter).CountDocuments();
        }

        public long GetCancelledBookingsCount(string venueId)
        {
            var filter = Builders<VenueBooking>.Filter.Eq(a => a.Venue.Id, venueId) &
                         Builders<VenueBooking>.Filter.Eq(a => a.Status, VenueBookingStatus.Cancelled);
            return _collection.Find(filter).CountDocuments();
        }

        public long GetBookingsCountPerVenueByUserId(string userId, string venueId)
        {
            var filter = Builders<VenueBooking>.Filter.Eq(a => a.Venue.Id, venueId) &
                         Builders<VenueBooking>.Filter.Eq(a => a.User.Id, userId);
            return _collection.Find(filter).CountDocuments();
        }

        public async Task<List<VenueBooking>> RejectBookingsForDeactivatedVenues(List<string> venueIds)
        {
            var findFilter = Builders<VenueBooking>.Filter.In(a => a.Venue.Id, venueIds) &
                             Builders<VenueBooking>.Filter.Gt(a => a.Date, UAEDateTime.Now) &
                             Builders<VenueBooking>.Filter.Ne(a => a.Status, VenueBookingStatus.Rejected) &
                             Builders<VenueBooking>.Filter.Ne(a => a.Status, VenueBookingStatus.Cancelled);

            var bookings = _collection.Find(findFilter).ToList();

            var update = Builders<VenueBooking>.Update.Set(a => a.Status, VenueBookingStatus.Rejected)
                                                      .Set(a => a.Reminders, new List<ReminderType>());
            var updateResult = await _collection.UpdateManyAsync(findFilter, update);
            return bookings;
        }

        public async Task<bool> DeleteBookingsForDeletedVenue(string venueId)
        {
            var filter = Builders<VenueBooking>.Filter.Eq(a => a.Venue.Id, venueId);
            var deleteResult = await _collection.DeleteManyAsync(filter);
            return deleteResult.IsAcknowledged;
        }

        public async Task<bool> ApproveBooking(string bookingId)
        {
            var filter = Builders<VenueBooking>.Filter.Eq(a => a.Id, bookingId);
            var update = Builders<VenueBooking>.Update.Set(a => a.Status, VenueBookingStatus.Approved)
                                                      .Set(a => a.ModifiedBy, _userDetailsProvider.UserId)
                                                      .Set(a => a.LastModifiedDate, DateTime.UtcNow);
            var updateResult = await _collection.UpdateOneAsync(filter, update);
            return updateResult.IsAcknowledged;
        }

        public async Task<bool> RejectBooking(string bookingId)
        {
            var filter = Builders<VenueBooking>.Filter.Eq(a => a.Id, bookingId);
            var update = Builders<VenueBooking>.Update.Set(a => a.Status, VenueBookingStatus.Rejected)
                                                      .Set(a => a.ModifiedBy, _userDetailsProvider.UserId)
                                                      .Set(a => a.LastModifiedDate, DateTime.UtcNow)
                                                      .Set(a => a.Reminders, new List<ReminderType>());
            var updateResult = await _collection.UpdateOneAsync(filter, update);
            return updateResult.IsAcknowledged;
        }

        public async Task<List<BookingResponse>> GetAllBookings(BookingFilterationRequest filterRequest, List<string> accessibleVenues, bool isSuperAdmin)
        {
            var searchFilter = SearchFilter(filterRequest?.SearchQuery) & Builders<VenueBooking>.Filter.InOrParameterEmpty(a => a.Venue.Id, accessibleVenues, isSuperAdmin);
            var records = await _collection.Aggregate(new AggregateOptions { AllowDiskUse = true, Collation = new Collation(locale: "en", strength: CollationStrength.Secondary) })
                                           .Match(searchFilter)
                                           .SortByDescending(a => a.CreatedDate)
                                           .Group(a => new { a.Venue.Id }, v => new BookingResponse
                                           {
                                               Id = v.First().Venue.Id,
                                               Name = v.First().Venue.Name,
                                               City = v.First().Venue.Location.City.Name,
                                               LastBookingDate = v.First().CreatedDate,
                                               BookingsCount = v.Count()
                                           })
                                           .SortBy(a => a.Name).ToListAsync();
            return records;
        }

        public async Task<List<VenueBooking>> GetBookingsByVenueId(string venueId, FilterationRequest filterRequest = null)
        {
            var venueFilter = Builders<VenueBooking>.Filter.Eq(a => a.Venue.Id, venueId);

            var searchFilter = Builders<VenueBooking>.Filter.Empty;
            if (filterRequest != null && !string.IsNullOrEmpty(filterRequest.SearchQuery))
                searchFilter = Builders<VenueBooking>.Filter.SearchContains(a => a.User.FullName, filterRequest.SearchQuery);

            var collation = new Collation(locale: "en", strength: CollationStrength.Secondary);

            var sort = filterRequest?.SortBy switch
            {
                Sort.Alphabetical => Builders<VenueBooking>.Sort.Ascending(a => a.User.FullName),
                Sort.Newest => Builders<VenueBooking>.Sort.Descending(a => a.CreatedDate).Ascending(a => a.User.FullName),
                Sort.Date => Builders<VenueBooking>.Sort.Descending(a => a.Date).Ascending(a => a.User.FullName),
                (_) => Builders<VenueBooking>.Sort.Ascending(a => a.User.FullName),
            };

            var records = new List<VenueBooking>();
            if (int.TryParse(filterRequest?.SearchQuery, out int value))
                records = await _collection.Aggregate(new AggregateOptions { Collation = collation })
                                  .Match(venueFilter)
                                  .Sort(sort)
                                  .AppendStage<VenueBooking>(@"{ $addFields: { bookingNumberString : { $toString : '$BookingNumber'} } }")
                                  .AppendStage<VenueBooking>(@$"{{ $match: {{bookingNumberString : /{Convert.ToInt32(filterRequest.SearchQuery)}/ }} }}")
                                  .Project<VenueBooking>(Builders<VenueBooking>.Projection.Exclude("bookingNumberString"))
                                  .ToListAsync();

            else
                records = await _collection.Aggregate(new AggregateOptions { Collation = collation })
                                  .Match(venueFilter & searchFilter)
                                  .Sort(sort)
                                  .ToListAsync();

            return filterRequest?.SortBy == Sort.Date ? GetNearestBookingsInDate(records) : records;
        }

        private List<VenueBooking> GetNearestBookingsInDate(List<VenueBooking> bookings)
        {
            var orderedBookings = bookings.OrderBy(a => a.Date);
            var newBookings = orderedBookings.Where(a => a.Date >= UAEDateTime.Now).GroupBy(a => a.Id).Select(a => a.FirstOrDefault());
            var oldBookings = orderedBookings.Where(a => !newBookings.Any(e => e.Id == a.Id)).GroupBy(a => a.Id).Select(a => a.LastOrDefault());
            return newBookings.Concat(oldBookings.OrderByDescending(a => a.Date)).ToList();
        }

        public async Task<List<VenueBooking>> GetVenueBookingDetailedReport(string venueId, VenueBookingReportFilterRequest filterRequest, List<string> bookingsIds = null)
        {
            var venueIdFilter = Builders<VenueBooking>.Filter.Eq(a => a.Venue.Id, venueId);

            var searchFilter = Builders<VenueBooking>.Filter.Empty;
            if (filterRequest != null && !string.IsNullOrEmpty(filterRequest?.SearchQuery))
                searchFilter = Builders<VenueBooking>.Filter.SearchContains(a => a.User.FullName, filterRequest?.SearchQuery);

            var bookingsFilter = Builders<VenueBooking>.Filter.Empty;
            if (bookingsIds != null && bookingsIds.Any())
                bookingsFilter = Builders<VenueBooking>.Filter.In(a => a.Id, bookingsIds);

            var dateFilter = Builders<VenueBooking>.Filter.Empty;

            if (filterRequest != null && filterRequest.From != null && filterRequest.To != null)
            {
                var fromDate = filterRequest.From.Value.Date;
                var toDate = filterRequest.To.Value.Date.AddDays(1);
                switch (filterRequest.FilteredField)
                {
                    case FilteredField.CreationDate:
                        dateFilter = Builders<VenueBooking>.Filter.Gte(a => a.CreatedDate, fromDate) &
                                     Builders<VenueBooking>.Filter.Lt(a => a.CreatedDate, toDate);
                        break;

                    case FilteredField.ReservationDate:
                        dateFilter = Builders<VenueBooking>.Filter.Gte(a => a.Date, fromDate) &
                                     Builders<VenueBooking>.Filter.Lt(a => a.Date, toDate);
                        break;

                    default:
                        dateFilter = Builders<VenueBooking>.Filter.Gte(a => a.Date, fromDate) &
                                     Builders<VenueBooking>.Filter.Lt(a => a.Date, toDate);
                        break;
                }
            }

            var records = await _collection.Find(venueIdFilter & dateFilter & bookingsFilter & searchFilter).ToListAsync();

            return filterRequest?.Sort switch
            {
                VenueBookingReportSort.Newest => records.OrderByDescending(a => a.CreatedDate).ThenBy(a => a.User.FullName).ToList(),
                VenueBookingReportSort.Alphabetical => records.OrderBy(a => a.User.FullName).ThenByDescending(a => a.CreatedDate).ToList(),
                VenueBookingReportSort.Approved => records.OrderBy(a => a.Status == VenueBookingStatus.Approved ? 0 : 1).ThenBy(a => a.Status == VenueBookingStatus.Pending ? 0 : 1).ThenBy(a => a.User.FullName).ToList(),
                VenueBookingReportSort.Cancelled => records.OrderBy(a => a.Status == VenueBookingStatus.Cancelled ? 0 : 1).ThenBy(a => a.Status == VenueBookingStatus.Rejected ? 0 : 1).ThenBy(a => a.User.FullName).ToList(),
                (_) => records.OrderBy(a => a.Status == VenueBookingStatus.Approved ? 0 : 1).ThenBy(a => a.User.FullName).ToList(),
            };
        }

        public Task SyncUserWithVenueBookings(ApplicationUser oldUser, ApplicationUser newUser)
        {
            if (oldUser?.FullName != newUser.FullName || oldUser?.PhoneNumber != newUser.PhoneNumber ||
                oldUser?.Email != newUser.Email || oldUser?.Gender != newUser.Gender || oldUser?.ProfileImage != newUser.ProfileImage)
            {
                var userIdFilter = Builders<VenueBooking>.Filter.Eq(v => v.User.Id, newUser.Id);
                var updateNameDef = Builders<VenueBooking>.Update.Set(v => v.User.FullName, newUser.FullName)
                                                                 .Set(v => v.User.PhoneNumber, newUser.PhoneNumber)
                                                                 .Set(v => v.User.Email, newUser.Email)
                                                                 .Set(v => v.User.Gender, newUser.Gender)
                                                                 .Set(v => v.User.ProfileImage, newUser.ProfileImage);
                var updates = new List<UpdateDefinition<VenueBooking>> { updateNameDef };
                return _collection.UpdateManyAsync(userIdFilter, updateNameDef);
            }
            return Task.CompletedTask;
        }
    }
}
