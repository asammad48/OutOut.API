using AutoMapper;
using OutOut.Constants.Errors;
using OutOut.Models.Exceptions;
using OutOut.Models.Utils;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Extensions;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Providers;
using OutOut.ViewModels.Requests.HomePage;
using OutOut.ViewModels.Responses.Events;
using OutOut.ViewModels.Responses.HomePage;
using OutOut.ViewModels.Responses.Offers;
using OutOut.ViewModels.Responses.Venues;
using OutOut.ViewModels.Wrappers;
using System.Net;

namespace OutOut.Core.Services
{
    public class HomeScreenService
    {
        private readonly IVenueRepository _venueRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IOfferRepository _offerRepository;

        private readonly IUserDetailsProvider _userDetailsProvider;
        private readonly IMapper _mapper;
        public HomeScreenService(IUserDetailsProvider userDetailsProvider,
                                 IMapper mapper,
                                 IVenueRepository venueRepository,
                                 IEventRepository eventRepository,
                                 IOfferRepository offerRepository)
        {
            _userDetailsProvider = userDetailsProvider;
            _mapper = mapper;
            _venueRepository = venueRepository;
            _eventRepository = eventRepository;
            _offerRepository = offerRepository;
        }

        public async Task<Page<VenueSummaryResponse>> DashboardVenuesSearchFilter(HomePageWebFilterationRequest filterRequest, PaginationRequest paginationRequest)
        {
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);
            
            await _userDetailsProvider.ReInitialize();

            var venues = await _venueRepository.DashboardFilter(paginationRequest, filterRequest, user.AccessibleVenues, _userDetailsProvider.IsSuperAdmin);
            var venuesResult = _mapper.Map<Page<VenueSummaryResponse>>(venues);

            if (IsRequiredFiltersNotProvided(filterRequest) && (filterRequest?.VenueCategories == null || !filterRequest.VenueCategories.Any()) && string.IsNullOrEmpty(filterRequest?.OfferTypeId))
            {
                venuesResult?.Records?.Clear();
                venuesResult = new Page<VenueSummaryResponse>(venuesResult?.Records, paginationRequest.PageNumber, paginationRequest.PageSize);
            }

            return venuesResult;
        }

        public async Task<Page<EventSummaryResponse>> DashboardEventsSearchFilter(HomePageWebFilterationRequest filterRequest, PaginationRequest paginationRequest)
        {
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            await _userDetailsProvider.ReInitialize();

            var eventsOcccurrences = await _eventRepository.DashboardFilter(filterRequest, _userDetailsProvider.GetAccessibleEvents(), _userDetailsProvider.IsSuperAdmin);
            var paginatedEvents = eventsOcccurrences.GetPaged(paginationRequest);
            var eventsResult = _mapper.Map<Page<EventSummaryResponse>>(paginatedEvents);

            if (IsRequiredFiltersNotProvided(filterRequest) && (filterRequest?.EventCategories == null || !filterRequest.EventCategories.Any()) && !filterRequest.FeaturedEvents)
            {
                eventsResult?.Records?.Clear();
                eventsResult = new Page<EventSummaryResponse>(eventsResult?.Records, paginationRequest.PageNumber, paginationRequest.PageSize);
            }

            return eventsResult;
        }

        public async Task<Page<OfferWithVenueResponse>> DashboardOffersSearchFilter(HomePageWebFilterationRequest filterRequest, PaginationRequest paginationRequest)
        {
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            await _userDetailsProvider.ReInitialize();

            var offers = await _offerRepository.DashboardFilter(paginationRequest, filterRequest, user.AccessibleVenues, _userDetailsProvider.IsSuperAdmin);
            var offersResult = _mapper.Map<Page<OfferWithVenueResponse>>(offers);

            if (IsRequiredFiltersNotProvided(filterRequest) && string.IsNullOrEmpty(filterRequest?.OfferTypeId) && (filterRequest?.VenueCategories == null || !filterRequest.VenueCategories.Any()))
            {
                offersResult?.Records?.Clear();
                offersResult = new Page<OfferWithVenueResponse>(offersResult?.Records, paginationRequest.PageNumber, paginationRequest.PageSize);
            }

            return offersResult;
        }

        public async Task<HomePageResponse> HomeSearchFilter(HomePageFilterationRequest filterRequest)
        {
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var venues = await _venueRepository.HomeFilter(user.Location, filterRequest);
            var venuesResult = _mapper.Map<List<VenueSummaryResponse>>(venues.Take(5).ToList());

            var events = await _eventRepository.HomeFilter(filterRequest);
            events = events.OrderBy(a => a.Occurrence.GetStartDateTime())
                           .Where(a => a.Occurrence.GetStartDateTime() >= UAEDateTime.Now)
                           .GroupBy(a => a.Id)
                           .Select(a => a.FirstOrDefault())
                           .ToList();
            var eventsResult = _mapper.Map<List<EventSummaryResponse>>(events.Take(5).ToList());

            var offers = await _offerRepository.HomeFilter(user.Location, filterRequest);
            var offersResult = _mapper.Map<List<OfferWithVenueResponse>>(offers.Take(5).ToList());

            if (IsRequiredFiltersNotProvided(filterRequest) && (filterRequest?.VenueCategories == null || !filterRequest.VenueCategories.Any()) && string.IsNullOrEmpty(filterRequest?.OfferTypeId))
                venuesResult?.Clear();
            if (IsRequiredFiltersNotProvided(filterRequest) && (filterRequest?.EventCategories == null || !filterRequest.EventCategories.Any()))
                eventsResult?.Clear();
            if (IsRequiredFiltersNotProvided(filterRequest) && string.IsNullOrEmpty(filterRequest?.OfferTypeId) && (filterRequest?.VenueCategories == null || !filterRequest.VenueCategories.Any()))
                offersResult?.Clear();

            return new HomePageResponse { Venues = venuesResult, Events = eventsResult, Offers = offersResult };
        }

        public bool IsRequiredFiltersNotProvided(HomePageWebFilterationRequest filterRequest) =>
            ((filterRequest?.VenueCategories != null && filterRequest.VenueCategories.Any()) || (filterRequest?.EventCategories != null && filterRequest.EventCategories.Any()) || filterRequest.FeaturedEvents || !string.IsNullOrEmpty(filterRequest?.OfferTypeId))
            && string.IsNullOrEmpty(filterRequest.SearchQuery) && string.IsNullOrEmpty(filterRequest.CityId) && filterRequest.From == null && filterRequest.To == null;

        public bool IsRequiredFiltersNotProvided(HomePageFilterationRequest filterRequest) =>
            ((filterRequest?.VenueCategories != null && filterRequest.VenueCategories.Any()) || (filterRequest?.EventCategories != null && filterRequest.EventCategories.Any()) || !string.IsNullOrEmpty(filterRequest?.OfferTypeId))
             && string.IsNullOrEmpty(filterRequest.SearchQuery) && string.IsNullOrEmpty(filterRequest.CityId) && (filterRequest.Areas == null || !filterRequest.Areas.Any()) && (filterRequest.From == null || filterRequest.From == DateTime.MinValue) && (filterRequest.To == null|| filterRequest.To == DateTime.MinValue);
    }
}
