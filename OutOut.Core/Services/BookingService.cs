using AutoMapper;
using OutOut.Constants.Errors;
using OutOut.Core.Utils;
using OutOut.Models.Exceptions;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Extensions;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Providers;
using OutOut.ViewModels.Requests.Bookings;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Responses.Bookings;
using OutOut.ViewModels.Responses.EventBookings;
using OutOut.ViewModels.Responses.Events;
using OutOut.ViewModels.Responses.Excel;
using OutOut.ViewModels.Responses.VenueBooking;
using OutOut.ViewModels.Wrappers;
using System.Net;

namespace OutOut.Core.Services
{
    public class BookingService
    {
        private readonly IMapper _mapper;
        private readonly IVenueBookingRepository _venueBookingRepository;
        private readonly IVenueRepository _venueRepository;
        private readonly IEventBookingRepository _eventBookingRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IUserDetailsProvider _userDetailsProvider;
        public BookingService(IMapper mapper, IVenueBookingRepository venueBookingRepository, IEventBookingRepository eventBookingRepository, IUserDetailsProvider userDetailsProvider, IVenueRepository venueRepository, IEventRepository eventRepository)
        {
            _mapper = mapper;
            _venueBookingRepository = venueBookingRepository;
            _eventBookingRepository = eventBookingRepository;
            _userDetailsProvider = userDetailsProvider;
            _venueRepository = venueRepository;
            _eventRepository = eventRepository;
        }

        public async Task<Page<BookingResponse>> GetAllBookingsPage(PaginationRequest paginationRequest, BookingFilterationRequest filterationRequest)
        {
            var allBookings = new List<BookingResponse>();

            await _userDetailsProvider.ReInitialize();

            var venueBookings = await _venueBookingRepository.GetAllBookings(filterationRequest, _userDetailsProvider.User.AccessibleVenues, _userDetailsProvider.IsSuperAdmin);
            var eventBookings = await _eventBookingRepository.GetAllPaidAndRejectedBookings(filterationRequest, _userDetailsProvider.GetAccessibleEvents(), _userDetailsProvider.IsSuperAdmin);

            venueBookings.ForEach(venue =>
            {
                var existingVenue = _venueRepository.GetVenueById(venue.Id);
                venue.Name = existingVenue?.Name;
                venue.City = existingVenue?.Location?.City?.Name;
            });

            eventBookings.ForEach(eventObj =>
            {
                var existingEvent = _eventRepository.GetEventById(eventObj.Id);
                eventObj.Name = existingEvent?.Name;
                eventObj.City = existingEvent?.Location?.City?.Name;
            });

            if (filterationRequest.SortBy == SortBooking.Alphabetical)
            {
                allBookings.AddRange(eventBookings);
                allBookings.AddRange(venueBookings);
                allBookings = allBookings.OrderBy(a => a.Name).ToList();
            }

            else if (filterationRequest.SortBy == SortBooking.Newest)
            {
                allBookings.AddRange(eventBookings);
                allBookings.AddRange(venueBookings);
                allBookings = allBookings.OrderByDescending(a => a.LastBookingDate).ToList();
            }

            else if (filterationRequest.SortBy == SortBooking.Event)
            {
                allBookings.AddRange(eventBookings);
                allBookings.AddRange(venueBookings);
            }

            else if (filterationRequest.SortBy == SortBooking.Venue)
            {
                allBookings.AddRange(venueBookings);
                allBookings.AddRange(eventBookings);
            }

            return allBookings.GetPaged(paginationRequest);
        }

        public async Task<Page<VenueBookingResponse>> GetVenueBookingsPage(string venueId, PaginationRequest paginationRequest, FilterationRequest filterationRequest)
        {
            var existingVenue = await _venueRepository.GetById(venueId);
            if (existingVenue == null)
                throw new OutOutException(ErrorCodes.VenueNotFound);

            await _userDetailsProvider.ReInitialize();

            if (!_userDetailsProvider.HasAccessToVenue(venueId) && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisVenue, HttpStatusCode.Forbidden);

            var venueBookings = await _venueBookingRepository.GetBookingsByVenueId(venueId, filterationRequest);
            return _mapper.Map<Page<VenueBookingResponse>>(venueBookings.GetPaged(paginationRequest));
        }

        public async Task<FileResponse> ExportAllVenueBookingsToExcel(string venueId)
        {
            var existingVenue = await _venueRepository.GetById(venueId);
            if (existingVenue == null)
                throw new OutOutException(ErrorCodes.VenueNotFound);

            await _userDetailsProvider.ReInitialize();

            if (!_userDetailsProvider.HasAccessToVenue(venueId) && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisVenue, HttpStatusCode.Forbidden);

            var venueBookings = await _venueBookingRepository.GetBookingsByVenueId(venueId);
            var data = _mapper.Map<List<VenueBookingSummaryResponseDTO>>(venueBookings);

            var file = ExcelUtils.ExportToExcel(data, venueBookings.FirstOrDefault().Venue?.Name).ToArray();
            return new FileResponse(file, $"{existingVenue.Name} Venue Bookings.xlsx");
        }

        public async Task<VenueBookingResponse> GetVenueBooking(string bookingId)
        {
            var existingBooking = await _venueBookingRepository.GetById(bookingId);
            if (existingBooking == null)
                throw new OutOutException(ErrorCodes.BookingNotFound);

            await _userDetailsProvider.ReInitialize();

            if (!_userDetailsProvider.HasAccessToVenue(existingBooking.Venue.Id) && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisVenue, HttpStatusCode.Forbidden);

            return _mapper.Map<VenueBookingResponse>(existingBooking);
        }

        public async Task<FileResponse> ExportVenueBookingToExcel(string venueBookingId)
        {
            var existingBooking = await _venueBookingRepository.GetById(venueBookingId);
            if (existingBooking == null)
                throw new OutOutException(ErrorCodes.BookingNotFound);

            await _userDetailsProvider.ReInitialize();

            if (!_userDetailsProvider.HasAccessToVenue(existingBooking.Venue.Id) && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisVenue, HttpStatusCode.Forbidden);

            var data = _mapper.Map<VenueBookingResponseDTO>(existingBooking);

            var file = ExcelUtils.ExportToExcel(new List<VenueBookingResponseDTO> { data }, existingBooking.Venue?.Name).ToArray();
            return new FileResponse(file, $"{existingBooking.User?.FullName}'s Booking at {existingBooking.Venue?.Name} Venue.xlsx");
        }

        public async Task<Page<EventBookingSummaryResponse>> GetEventBookingsPage(string eventId, PaginationRequest paginationRequest, FilterationRequest filterationRequest)
        {
            var existingEvent = await _eventRepository.GetById(eventId);
            if (existingEvent == null)
                throw new OutOutException(ErrorCodes.EventNotFound);

            await _userDetailsProvider.ReInitialize();

            if (!_userDetailsProvider.HasAccessToEvent(eventId) && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisEvent, HttpStatusCode.Forbidden);

            var eventBookings = await _eventBookingRepository.GetBookingsByEventId(eventId, filterationRequest);
            return _mapper.Map<Page<EventBookingSummaryResponse>>(eventBookings.GetPaged(paginationRequest));
        }

        public async Task<FileResponse> ExportAllEventBookingsToExcel(string eventId)
        {
            var existingEvent = await _eventRepository.GetById(eventId);
            if (existingEvent == null)
                throw new OutOutException(ErrorCodes.EventNotFound);

            await _userDetailsProvider.ReInitialize();

            if (!_userDetailsProvider.HasAccessToEvent(eventId) && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisEvent, HttpStatusCode.Forbidden);

            var eventBookings = await _eventBookingRepository.GetBookingsByEventId(eventId);
            var data = _mapper.Map<List<EventBookingSummaryResponseDTO>>(eventBookings);

            var file = ExcelUtils.ExportToExcel(data, eventBookings.FirstOrDefault().Event?.Name).ToArray();
            return new FileResponse(file, $"{existingEvent.Name} Event Bookings.xlsx");
        }

        public async Task<EventBookingSummaryResponse> GetEventBooking(string bookingId)
        {
            var existingBooking = await _eventBookingRepository.GetById(bookingId);
            if (existingBooking == null)
                throw new OutOutException(ErrorCodes.BookingNotFound);

            await _userDetailsProvider.ReInitialize();

            if (!_userDetailsProvider.HasAccessToEvent(existingBooking.Event.Id) && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisEvent, HttpStatusCode.Forbidden);

            return _mapper.Map<EventBookingSummaryResponse>(existingBooking);
        }

        public async Task<Page<TicketResponse>> GetTicketsByBookingId(string bookingId, PaginationRequest paginationRequest)
        {
            var existingBooking = await _eventBookingRepository.GetById(bookingId);
            if (existingBooking == null)
                throw new OutOutException(ErrorCodes.BookingNotFound);

            await _userDetailsProvider.ReInitialize();

            if (!_userDetailsProvider.HasAccessToEvent(existingBooking.Event.Id) && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisEvent, HttpStatusCode.Forbidden);

            var tickets = await _eventBookingRepository.GetTicketsPage(bookingId, paginationRequest);
            var result = _mapper.Map<Page<TicketResponse>>(tickets);
            long index = (result.PageNumber * result.PageSize) + 1;
            foreach (var item in result.Records)
            {
                item.Index = index;
                index++;
            }
            return result;
        }

        public async Task<FileResponse> ExportEventBookingToExcel(string eventBookingId)
        {
            var existingBooking = await _eventBookingRepository.GetById(eventBookingId);
            if (existingBooking == null)
                throw new OutOutException(ErrorCodes.BookingNotFound);

            await _userDetailsProvider.ReInitialize();

            if (!_userDetailsProvider.HasAccessToEvent(existingBooking.Event.Id) && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisEvent, HttpStatusCode.Forbidden);

            var data = _mapper.Map<EventBookingResponseDTO>(existingBooking);

            var file = ExcelUtils.ExportToExcel(new List<EventBookingResponseDTO> { data }, existingBooking.Event?.Name).ToArray();
            return new FileResponse(file, $"{existingBooking.User?.FullName}'s Booking at {existingBooking.Event?.Name} Event.xlsx");
        }
    }
}
