using MongoDB.Driver;
using OutOut.Constants.Enums;
using OutOut.Models.Domains;
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
using OutOut.ViewModels.Requests.EventBooking;
using OutOut.ViewModels.Requests.Fitlers;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Requests.Ticket;
using OutOut.ViewModels.Requests.Venues;
using OutOut.ViewModels.Responses.Bookings;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Services
{
    public class EventBookingRepository : GenericNonSqlRepository<EventBooking>, IEventBookingRepository
    {
        protected readonly IUserDetailsProvider _userDetailsProvider;

        public EventBookingRepository(ApplicationNonSqlDbContext dbContext, IEnumerable<ISyncRepository<EventBooking>> syncRepositories, IUserDetailsProvider userDetailsProvider) : base(dbContext, syncRepositories)
        {
            _userDetailsProvider = userDetailsProvider;
        }

        public async Task<List<ApplicationUserSummary>> GetUsersByEventIds(List<string> eventIds, FilterationRequest filterRequest = null)
        {
            var eventsFilter = Builders<EventBooking>.Filter.In(a => a.Event.Id, eventIds)
                & (Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.Rejected) | Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.Paid) | Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.OnHold));

            var searchFilter = Builders<EventBooking>.Filter.Empty;
            if (filterRequest != null && !string.IsNullOrEmpty(filterRequest.SearchQuery))
                searchFilter = Builders<EventBooking>.Filter.SearchContains(a => a.User.FullName, filterRequest.SearchQuery);

            //var roleFilter = Builders<ApplicationUser>.Filter.Size(a => a.Roles, 0);

            return await _collection.Find(eventsFilter & searchFilter).Project(x => x.User).ToListAsync();
        }

        public async Task<EventBooking> UpdateEventBooking(EventBooking eventBooking)
        {
            eventBooking.LastModifiedDate = DateTime.UtcNow;
            eventBooking.ModifiedBy = _userDetailsProvider.UserId;

            EventBooking oldEntity = null;
            if (_syncRepositories.Any())
            {
                oldEntity = await GetById(eventBooking.Id);
            }

            var eventBookingFilter = Builders<EventBooking>.Filter.Eq(a => a.Id, eventBooking.Id);
            await _collection.ReplaceOneAsync(eventBookingFilter, eventBooking);

            if (_syncRepositories.Any())
            {
                await Sync(oldEntity, eventBooking);
            }
            return eventBooking;
        }

        private FilterDefinition<EventBooking> SearchFilter(string searchQuery)
        {
            var searchFilter = Builders<EventBooking>.Filter.Empty;
            if (!string.IsNullOrEmpty(searchQuery))
                searchFilter = Builders<EventBooking>.Filter.SearchContains(c => c.Event.Name, searchQuery);
            return searchFilter;
        }

        private FilterDefinition<EventBooking> TimeFilter(MyBookingFilterationRequest filterRequest)
        {
            var timeFilter = Builders<EventBooking>.Filter.Empty;

            if (filterRequest != null && filterRequest.MyBooking == MyBookingFilteration.History)
                timeFilter = Builders<EventBooking>.Filter.HistoryBooking(a => a.Event.Occurrence);

            else if (filterRequest != null && filterRequest.MyBooking == MyBookingFilteration.Recent)
                timeFilter = Builders<EventBooking>.Filter.RecentBooking(a => a.Event.Occurrence);

            return timeFilter;
        }

        private FilterDefinition<EventBooking> UserFilter(MyBookingFilterationRequest filterRequest, string userId)
        {
            var userFilter = Builders<EventBooking>.Filter.Empty;
            if (filterRequest != null)
                userFilter = Builders<EventBooking>.Filter.Eq(a => a.User.Id, userId);
            return userFilter;
        }

        public Task<Page<EventBooking>> GetMyBooking(PaginationRequest paginationRequest, MyBookingFilterationRequest filterRequest, string userId)
        {
            var userFilter = UserFilter(filterRequest, userId);
            var searchFilter = SearchFilter(filterRequest?.SearchQuery);
            var timeFilter = TimeFilter(filterRequest);
            var paymentStatusFilter = Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.Paid) |
                                      Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.Rejected) |
                                      Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.OnHold);

            var eventBookingFilters = Builders<EventBooking>.Filter.And(searchFilter & userFilter & timeFilter & paymentStatusFilter);

            var records = filterRequest.MyBooking == MyBookingFilteration.Recent ?
                _collection.Find(eventBookingFilters).Sort(EventOccurrenceTimeFilters.GetAscendingDateTimeSort()) :
                _collection.Find(eventBookingFilters).Sort(EventOccurrenceTimeFilters.GetDescendingDateTimeSort());

            return records.GetPaged(paginationRequest);
        }

        public async Task<EventBooking> GetEventBooking(string userId, string eventBookingId)
        {
            var filter = Builders<EventBooking>.Filter.Eq(c => c.User.Id, userId) &
                         Builders<EventBooking>.Filter.Eq(c => c.Id, eventBookingId) &
                         Builders<EventBooking>.Filter.Eq(c => c.Status, PaymentStatus.Paid);
            return await FindFirst(filter);
        }

        public async Task<Page<SingleEventBookingTicket>> GetMySharedTickets(PaginationRequest paginationRequest, MyBookingFilterationRequest filterRequest, List<SharedTicket> sharedTickets)
        {
            var sharedTicketsFilter = Builders<EventBooking>.Filter.In(a => a.Id, sharedTickets.Select(a => a.BookingId));
            var searchFilter = SearchFilter(filterRequest?.SearchQuery);
            var timeFilter = TimeFilter(filterRequest);
            var paidFilter = Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.Paid) |
                             Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.Rejected);

            var eventBookingFilters = Builders<EventBooking>.Filter.And(sharedTicketsFilter & searchFilter & timeFilter & paidFilter);

            var records = new List<SingleEventBookingTicket>();
            if (filterRequest.MyBooking == MyBookingFilteration.Recent)
                records = await _collection.Aggregate(new AggregateOptions { AllowDiskUse = true })
                                     .Match(eventBookingFilters)
                                     .Sort(EventOccurrenceTimeFilters.GetAscendingDateTimeSort())
                                     .Unwind<EventBooking, SingleEventBookingTicket>(a => a.Tickets)
                                     .ToListAsync();

            else
                records = await _collection.Aggregate(new AggregateOptions { AllowDiskUse = true })
                                     .Match(eventBookingFilters)
                                     .Sort(EventOccurrenceTimeFilters.GetDescendingDateTimeSort())
                                     .Unwind<EventBooking, SingleEventBookingTicket>(a => a.Tickets)
                                     .ToListAsync();

            var ticketIds = sharedTickets.Select(a => a.TicketId).ToList();
            records = records.Select(a => a).Where(a => ticketIds.Contains(a.Ticket.Id)).ToList();

            records.ForEach(r =>
            {
                r.Reminders = sharedTickets.Where(a => a.BookingId == r.Id).FirstOrDefault().Reminders;
            });

            return records.GetPaged(paginationRequest);
        }

        public async Task<EventBooking> GetEventBookingByTicketId(string ticketId)
        {
            var filter = Builders<EventBooking>.Filter.ElemMatch(a => a.Tickets, a => a.Id == ticketId) &
                         Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.Paid);
            return await FindFirst(filter);
        }

        public async Task<SingleEventBookingTicket> GetTicketDetails(string ticketId)
        {
            var filter = Builders<SingleEventBookingTicket>.Filter.Eq(a => a.Ticket.Id, ticketId);

            var query = await _collection.Aggregate(new AggregateOptions { AllowDiskUse = true })
                                .Unwind<EventBooking, SingleEventBookingTicket>(a => a.Tickets)
                                .Match(filter)
                                .FirstOrDefaultAsync();
            return query;
        }

        public async Task<Page<SingleEventBookingTicket>> GetTicketsRedeemedByUser(PaginationRequest pageRequest, TicketFilterationRequest request, string userId)
        {
            var filter = Builders<SingleEventBookingTicket>.Filter.Eq(a => a.Ticket.QrRedeemedBy, userId)
                | Builders<SingleEventBookingTicket>.Filter.Eq(a => a.Ticket.RejectedBy, userId);

            var searchFilter = Builders<SingleEventBookingTicket>.Filter.Empty;
            if (request != null)
            {
                if (!string.IsNullOrEmpty(request?.SearchQuery))
                {

                    searchFilter = Builders<SingleEventBookingTicket>.Filter.SearchContains(a => a.Event.Name, request.SearchQuery)
                                  | Builders<SingleEventBookingTicket>.Filter.SearchContains(a => a.User.Email, request.SearchQuery);

                }
            }

            var sortDefinition = Builders<SingleEventBookingTicket>.Sort.Descending(a => a.Ticket.RedemptionDate).Ascending(a => a.User.FullName);

            IAggregateFluent<SingleEventBookingTicket> query;
            if (int.TryParse(request.SearchQuery, out int value))
            {
                query = _collection.Aggregate(new AggregateOptions { AllowDiskUse = true })
                                .Unwind<EventBooking, SingleEventBookingTicket>(a => a.Tickets)
                                .Match(filter)
                                .AppendStage<SingleEventBookingTicket>(@"{ $addFields: { orderNumberString : { $toString : '$OrderNumber'} } }")
                                .AppendStage<SingleEventBookingTicket>(@$"{{ $match: {{orderNumberString : /{Convert.ToInt32(request.SearchQuery)}/ }} }}")
                                .Sort(sortDefinition);
            }
            else
            {
                query = _collection.Aggregate(new AggregateOptions { AllowDiskUse = true })
                                    .Unwind<EventBooking, SingleEventBookingTicket>(a => a.Tickets)
                                    .Match(filter)
                                    .Match(searchFilter)
                                    .Sort(sortDefinition);
            }



            var records = await query.Skip(pageRequest.PageNumber * pageRequest.PageSize)
                                     .Limit(pageRequest.PageSize)
                                     .ToListAsync();

            var recordsTotalCount = (await query.ToListAsync()).Count;

            var page = new Page<SingleEventBookingTicket>(records, pageRequest.PageNumber, pageRequest.PageSize, recordsTotalCount);
            return page;
        }

        public async Task<EventBooking> GetEventBookingByTicket(string ticketId, string ticketSecret)
        {
            var filter = Builders<EventBooking>.Filter.ElemMatch(a => a.Tickets, a => a.Id == ticketId) &
                         Builders<EventBooking>.Filter.ElemMatch(a => a.Tickets, a => a.Secret == ticketSecret) &
                         Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.Paid);
            return await FindFirst(filter);
        }

        public async Task<List<EventBooking>> GetStalePendingBooking()
        {
            var filter = (Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.Pending) |
                         Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.OnHold)) &
                         Builders<EventBooking>.Filter.Lte(a => a.CreatedDate, DateTime.UtcNow.AddMinutes(-30));
            var response = await Find(filter);
            return response;
        }

        public long GetPaidTicketsCount(string eventId, string occurrenceId = null)
        {
            var eventFilter = Builders<EventBooking>.Filter.Eq(a => a.Event.Id, eventId) &
                              Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.Paid);
            var occurrenceFilter = occurrenceId == null ? Builders<EventBooking>.Filter.Empty :
                                        Builders<EventBooking>.Filter.ObjectIdEq("Event.Occurrence.Id", occurrenceId);
            var result = _collection.Aggregate(new AggregateOptions { AllowDiskUse = true })
                                    .Match(eventFilter)
                                    .Match(occurrenceFilter)
                                    .Unwind<EventBooking, SingleEventBookingTicket>(a => a.Tickets)
                                    .Count()
                                    .FirstOrDefault();
            return result == null ? 0 : result.Count;
        }

        public long GetPendingTicketsCount(string eventId)
        {
            var filter = Builders<EventBooking>.Filter.Eq(a => a.Event.Id, eventId) &
                         Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.Pending);
            return _collection.Find(filter).ToList().Sum(a => a.Quantity);
        }

        public long GetPaidBookingsCount(string eventId)
        {
            var filter = Builders<EventBooking>.Filter.Eq(a => a.Event.Id, eventId) &
                         Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.Paid);
            return _collection.Find(filter).CountDocuments();
        }

        public double GetRevenueForEvent(string eventId)
        {
            var eventFilter = Builders<EventBooking>.Filter.Eq(a => a.Event.Id, eventId) &
                              Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.Paid);

            return _collection.Find(eventFilter).ToList().Sum(a => a.TotalAmount);
        }

        public long GetAttendeesCountForEvent(string eventId) => GetPaidTicketsCountForEvent(eventId, true);

        public long GetAbsenteesCountForEvent(string eventId) => GetPaidTicketsCountForEvent(eventId, false);

        private long GetPaidTicketsCountForEvent(string eventId, bool attended)
        {
            var filter = Builders<EventBooking>.Filter.Eq(a => a.Event.Id, eventId) &
                         Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.Paid);
            var ticketFilter = attended ? Builders<SingleEventBookingTicket>.Filter.Ne(a => a.Ticket.RedeemedBy, null) :
                                          Builders<SingleEventBookingTicket>.Filter.Eq(a => a.Ticket.RedeemedBy, null) &
                                          (Builders<SingleEventBookingTicket>.Filter.Lt("Event.Occurrence.EndDate", UAEDateTime.Now.Date) |
                                          (Builders<SingleEventBookingTicket>.Filter.Lte("Event.Occurrence.EndDate", UAEDateTime.Now.Date) &
                                          Builders<SingleEventBookingTicket>.Filter.Lt("Event.Occurrence.EndTime", UAEDateTime.Now.TimeOfDay)));

            var result = _collection.Aggregate(new AggregateOptions { AllowDiskUse = true })
                                      .Match(filter)
                                      .Unwind<EventBooking, SingleEventBookingTicket>(a => a.Tickets)
                                      .Match(ticketFilter)
                                      .Count()
                                      .FirstOrDefault();
            return result == null ? 0 : result.Count;
        }

        public async Task<List<EventBooking>> GetBookingsByEventId(string eventId, FilterationRequest filterRequest = null)
        {
            var eventFilter = Builders<EventBooking>.Filter.Eq(a => a.Event.Id, eventId) &
                              (Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.Paid) |
                               Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.Rejected));

            var searchFilter = Builders<EventBooking>.Filter.Empty;
            if (filterRequest != null && !string.IsNullOrEmpty(filterRequest.SearchQuery))
                searchFilter = Builders<EventBooking>.Filter.SearchContains(a => a.User.FullName, filterRequest.SearchQuery);

            var collation = new Collation(locale: "en", strength: CollationStrength.Secondary);

            var sort = filterRequest?.SortBy switch
            {
                Sort.Alphabetical => Builders<EventBooking>.Sort.Ascending(a => a.User.FullName),
                Sort.Newest => Builders<EventBooking>.Sort.Descending(a => a.CreatedDate).Ascending(a => a.User.FullName),
                Sort.Date => Builders<EventBooking>.Sort.Descending("Event.Occurrence.StartDate").Descending("Event.Occurrence.StartTime").Ascending(a => a.User.FullName),
                (_) => Builders<EventBooking>.Sort.Ascending(a => a.User.FullName),
            };

            var records = new List<EventBooking>();
            if (int.TryParse(filterRequest?.SearchQuery, out int value))
                records = await _collection.Aggregate(new AggregateOptions { Collation = collation })
                                  .Match(eventFilter)
                                  .Sort(sort)
                                  .AppendStage<EventBooking>(@"{ $addFields: { orderNumberString : { $toString : '$OrderNumber'} } }")
                                  .AppendStage<EventBooking>(@$"{{ $match: {{orderNumberString : /{Convert.ToInt32(filterRequest.SearchQuery)}/ }} }}")
                                  .Project<EventBooking>(Builders<EventBooking>.Projection.Exclude("orderNumberString"))
                                  .ToListAsync();

            else
                records = await _collection.Aggregate(new AggregateOptions { Collation = collation })
                                  .Match(eventFilter & searchFilter)
                                  .Sort(sort)
                                  .ToListAsync();

            return filterRequest?.SortBy == Sort.Date ? GetNearestBookingsInDate(records) : records;
        }

        private List<EventBooking> GetNearestBookingsInDate(List<EventBooking> occurrences)
        {
            var orderedOccurrences = occurrences.OrderBy(a => a.Event.Occurrence.StartDate).ThenBy(a => a.Event.Occurrence.StartTime);
            var newEvents = orderedOccurrences.Where(a => a.Event.Occurrence.GetStartDateTime() >= UAEDateTime.Now).GroupBy(a => a.Id).Select(a => a.FirstOrDefault());
            var oldEvents = orderedOccurrences.Where(a => !newEvents.Any(e => e.Id == a.Id)).GroupBy(a => a.Id).Select(a => a.LastOrDefault());
            return newEvents.Concat(oldEvents.OrderByDescending(a => a.Event.Occurrence.GetStartDateTime())).ToList();
        }

        public async Task<List<EventBooking>> GetCustomerAttendedEvents(string userId, SearchFilterationRequest searchFilterationRequest, List<string> accessibleEvents, bool isSuperAdmin)
        {
            var searchFilter = Builders<EventBooking>.Filter.Empty;
            if (!string.IsNullOrEmpty(searchFilterationRequest?.SearchQuery))
                searchFilter = Builders<EventBooking>.Filter.SearchContains(c => c.Event.Name, searchFilterationRequest?.SearchQuery) |
                               Builders<EventBooking>.Filter.SearchContains(c => c.Event.Location.City.Name, searchFilterationRequest?.SearchQuery);

            var userFilter = Builders<EventBooking>.Filter.ElemMatch(a => a.Tickets, a => a.RedeemedBy == userId) &
                             Builders<EventBooking>.Filter.InOrParameterEmpty(a => a.Event.Id, accessibleEvents, isSuperAdmin);

            var records = await _collection.Aggregate(new AggregateOptions { AllowDiskUse = true })
                                     .Match(searchFilter)
                                     .Match(userFilter)
                                     .ToListAsync();
            records.ForEach(booking => booking.Tickets = booking.Tickets.OrderByDescending(t => t.RedemptionDate).ToList());
            return records;
        }

        public long GetCustomersRedeemedTicketsCountPerOccurrence(string userId, string eventOccurrenceId)
        {
            var filter = Builders<EventBooking>.Filter.ObjectIdEq("Event.Occurrence.Id", eventOccurrenceId) &
                         Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.Paid);
            var ticketsFilter = Builders<SingleEventBookingTicket>.Filter.Eq(a => a.Ticket.RedeemedBy, userId);
            var result = _collection.Aggregate(new AggregateOptions { AllowDiskUse = true })
                                     .Match(filter)
                                     .Unwind<EventBooking, SingleEventBookingTicket>(a => a.Tickets)
                                     .Match(ticketsFilter)
                                     .Count()
                                     .FirstOrDefault();
            return result == null ? 0 : result.Count;
        }

        public async Task<List<BookingResponse>> GetAllPaidAndRejectedBookings(BookingFilterationRequest filterationRequest, List<string> accessibleEvents, bool isSuperAdmin)
        {
            var searchFilter = SearchFilter(filterationRequest?.SearchQuery);
            var filter = (Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.Paid) |
                          Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.Rejected)) &
                         Builders<EventBooking>.Filter.InOrParameterEmpty(a => a.Event.Id, accessibleEvents, isSuperAdmin);
            var records = await _collection.Aggregate(new AggregateOptions { AllowDiskUse = true, Collation = new Collation(locale: "en", strength: CollationStrength.Secondary) })
                                           .Match(searchFilter)
                                           .Match(filter)
                                           .SortByDescending(a => a.CreatedDate)
                                           .Group(a => new { a.Event.Id }, v => new BookingResponse
                                           {
                                               Name = v.First().Event.Name,
                                               Id = v.First().Event.Id,
                                               LastBookingDate = v.First().CreatedDate,
                                               City = v.First().Event.Location.City.Name,
                                               BookingsCount = v.Count()
                                           })
                                           .SortBy(a => a.Name).ToListAsync();
            records.ForEach(a => a.Type = TypeFor.Event);
            return records;
        }

        public async Task<bool> DeleteBookingRemindersForDeactivatedEvent(List<string> eventsIds)
        {
            var updateFilter = Builders<EventBooking>.Filter.In(a => a.Event.Id, eventsIds);
            var update = Builders<EventBooking>.Update.Set(a => a.Reminders, new List<ReminderType>());
            var updateResult = await _collection.UpdateManyAsync(updateFilter, update);
            return updateResult.IsAcknowledged;
        }

        public Task<List<EventBooking>> GetEventBookingDetailedReport(string eventId, EventBookingReportFilterRequest filterRequest, List<string> bookingsIds = null)
        {
            var eventIdFilter = Builders<EventBooking>.Filter.Eq(a => a.Event.Id, eventId) &
                                (Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.Paid) |
                                 Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.Rejected));

            var searchFilter = Builders<EventBooking>.Filter.Empty;
            if (filterRequest != null && !string.IsNullOrEmpty(filterRequest?.SearchQuery))
                searchFilter = Builders<EventBooking>.Filter.SearchContains(a => a.User.FullName, filterRequest?.SearchQuery);

            var bookingsFilter = Builders<EventBooking>.Filter.Empty;
            if (bookingsIds != null && bookingsIds.Any())
                bookingsFilter = Builders<EventBooking>.Filter.In(a => a.Id, bookingsIds);

            var dateFilter = Builders<EventBooking>.Filter.Empty;

            if (filterRequest != null && filterRequest.From != null && filterRequest.To != null)
            {
                var fromDate = filterRequest.From.Value.Date;
                var toDate = filterRequest.To.Value.Date.AddDays(1);

                switch (filterRequest.FilteredField)
                {
                    case FilteredField.CreationDate:
                        dateFilter = Builders<EventBooking>.Filter.Lt(a => a.CreatedDate, toDate.Date) &
                                     Builders<EventBooking>.Filter.Gte(a => a.CreatedDate, fromDate.Date);
                        break;

                    case FilteredField.ReservationDate:
                        dateFilter = Builders<EventBooking>.Filter.Lt("Event.Occurrence.StartDate", toDate) &
                                     Builders<EventBooking>.Filter.Gte("Event.Occurrence.StartDate", fromDate);
                        break;

                    default:
                        dateFilter = Builders<EventBooking>.Filter.Lt("Event.Occurrence.StartDate", toDate) &
                                     Builders<EventBooking>.Filter.Gte("Event.Occurrence.StartDate", fromDate);
                        break;
                }
            }

            return _collection.Find(eventIdFilter & dateFilter & bookingsFilter & searchFilter).SortByDescending(a => a.CreatedDate).ToListAsync();
        }

        public long GetAttendeesCountForBooking(string bookingId) => GetTicketsCountForBooking(bookingId, true);

        public long GetAbsenteesCountForBooking(string bookingId) => GetTicketsCountForBooking(bookingId, false);

        private long GetTicketsCountForBooking(string bookingId, bool attended)
        {
            var filter = Builders<EventBooking>.Filter.Eq(a => a.Id, bookingId) &
                         Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.Paid);
            var ticketFilter = attended ? Builders<SingleEventBookingTicket>.Filter.Ne(a => a.Ticket.RedeemedBy, null) :
                                          Builders<SingleEventBookingTicket>.Filter.Eq(a => a.Ticket.RedeemedBy, null) &
                                          (Builders<SingleEventBookingTicket>.Filter.Lt("Event.Occurrence.EndDate", UAEDateTime.Now.Date) |
                                          (Builders<SingleEventBookingTicket>.Filter.Lte("Event.Occurrence.EndDate", UAEDateTime.Now.Date) &
                                          Builders<SingleEventBookingTicket>.Filter.Lt("Event.Occurrence.EndTime", UAEDateTime.Now.TimeOfDay)));

            var result = _collection.Aggregate(new AggregateOptions { AllowDiskUse = true })
                                      .Match(filter)
                                      .Unwind<EventBooking, SingleEventBookingTicket>(a => a.Tickets)
                                      .Match(ticketFilter)
                                      .Count()
                                      .FirstOrDefault();
            return result == null ? 0 : result.Count;
        }

        public long GetBookedTicketsCountPerPackage(string eventId, string packageId, EventBookingReportFilterRequest filterRequest)
        {
            var filter = Builders<EventBooking>.Filter.Eq(a => a.Event.Id, eventId) &
                         Builders<EventBooking>.Filter.Eq(a => a.Package.Id, packageId) &
                         Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.Paid);

            var dateFilter = Builders<EventBooking>.Filter.Empty;
            if (filterRequest != null && filterRequest?.From != null && filterRequest?.To != null)
                dateFilter = PackageDateFilter(filterRequest.From.Value, filterRequest.To.Value);

            return _collection.Aggregate(new AggregateOptions { AllowDiskUse = true })
                              .Match(filter)
                              .Match(dateFilter)
                              .Unwind<EventBooking, SingleEventBookingTicket>(a => a.Tickets)
                              .Count()
                              .FirstOrDefault()?.Count ?? 0;
        }

        public long GetRejectedTicketsCountPerPackage(string eventId, string packageId, EventBookingReportFilterRequest filterRequest)
        {
            var filter = Builders<EventBooking>.Filter.Eq(a => a.Event.Id, eventId) &
                         Builders<EventBooking>.Filter.Eq(a => a.Package.Id, packageId) &
                         Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.Rejected);

            var dateFilter = Builders<EventBooking>.Filter.Empty;
            if (filterRequest != null && filterRequest?.From != null && filterRequest?.To != null)
                dateFilter = PackageDateFilter(filterRequest.From.Value, filterRequest.To.Value);

            return _collection.Find(filter & dateFilter).ToList().Sum(a => a.Quantity);
        }

        private FilterDefinition<EventBooking> PackageDateFilter(DateTime from, DateTime to)
        {
            return Builders<EventBooking>.Filter.Gte(a => a.CreatedDate, from.Date) &
                       Builders<EventBooking>.Filter.Lt(a => a.CreatedDate, to.Date.AddDays(1));
        }

        public double GetTotalSalesPerPackage(string eventId, string packageId, EventBookingReportFilterRequest filterRequest)
        {
            var filter = Builders<EventBooking>.Filter.Eq(a => a.Event.Id, eventId) &
                              Builders<EventBooking>.Filter.Eq(a => a.Package.Id, packageId) &
                              Builders<EventBooking>.Filter.Eq(a => a.Status, PaymentStatus.Paid);

            var dateFilter = Builders<EventBooking>.Filter.Empty;
            if (filterRequest != null && filterRequest?.From != null && filterRequest?.To != null)
                dateFilter = PackageDateFilter(filterRequest.From.Value, filterRequest.To.Value);

            return _collection.Find(filter & dateFilter).ToList().Sum(a => a.TotalAmount);
        }

        public Task SyncUserWithEventBookings(ApplicationUser oldUser, ApplicationUser newUser)
        {
            if (oldUser?.FullName != newUser.FullName || oldUser?.PhoneNumber != newUser.PhoneNumber ||
                oldUser?.Email != newUser.Email || oldUser?.Gender != newUser.Gender || oldUser?.ProfileImage != newUser.ProfileImage)
            {
                var userIdFilter = Builders<EventBooking>.Filter.Eq(v => v.User.Id, newUser.Id);
                var updateNameDef = Builders<EventBooking>.Update.Set(v => v.User.FullName, newUser.FullName)
                                                                 .Set(v => v.User.PhoneNumber, newUser.PhoneNumber)
                                                                 .Set(v => v.User.Email, newUser.Email)
                                                                 .Set(v => v.User.Gender, newUser.Gender)
                                                                 .Set(v => v.User.ProfileImage, newUser.ProfileImage);
                var updates = new List<UpdateDefinition<EventBooking>> { updateNameDef };
                return _collection.UpdateManyAsync(userIdFilter, updateNameDef);
            }
            return Task.CompletedTask;
        }

        public async Task<bool> DeleteBookingsForDeletedEvent(string eventId)
        {
            var filter = Builders<EventBooking>.Filter.Eq(a => a.Event.Id, eventId);
            var deleteResult = await _collection.DeleteManyAsync(filter);
            return deleteResult.IsAcknowledged;
        }

        public async Task<Page<SingleEventBookingTicket>> GetTicketsPage(string bookingId, PaginationRequest paginationRequest)
        {
            var filter = Builders<EventBooking>.Filter.Eq(a => a.Id, bookingId);

            var records = await _collection.Aggregate(new AggregateOptions { AllowDiskUse = true })
                                 .Match(filter)
                                 .Unwind<EventBooking, SingleEventBookingTicket>(a => a.Tickets)
                                 .SortBy(a => a.Ticket.Id)
                                 .Skip(paginationRequest.PageNumber * paginationRequest.PageSize)
                                 .Limit(paginationRequest.PageSize)
                                 .ToListAsync();
            var recordsCount = _collection.Aggregate().Match(filter).Unwind<EventBooking, SingleEventBookingTicket>(a => a.Tickets).Count().FirstOrDefault()?.Count ?? 0;
            return new Page<SingleEventBookingTicket>(records, paginationRequest.PageNumber, paginationRequest.PageSize, recordsCount);
        }
    }
}
