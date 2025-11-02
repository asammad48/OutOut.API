using AutoMapper;
using OutOut.Constants.Enums;
using OutOut.Constants.Errors;
using OutOut.Models.Exceptions;
using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Providers;
using OutOut.ViewModels.Requests.Areas;
using OutOut.ViewModels.Requests.Cities;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Responses.Cities;
using OutOut.ViewModels.Responses.Countries;
using OutOut.ViewModels.Wrappers;
using System.Net;

namespace OutOut.Core.Services
{
    public class CityService
    {
        private readonly ICityRepository _cityRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IVenueRepository _venueRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IUserDetailsProvider _userDetailsProvider;
        private readonly VenueService _venueService;
        private readonly EventService _eventService;
        private readonly IMapper _mapper;

        public CityService(ICityRepository cityRepository,
                           IUserDetailsProvider userDetailsProvider,
                           IMapper mapper,
                           ICountryRepository countryRepository,
                           IVenueRepository venueRepository,
                           IEventRepository eventRepository,
                           VenueService venueService, 
                           EventService eventService)
        {
            _cityRepository = cityRepository;
            _userDetailsProvider = userDetailsProvider;
            _mapper = mapper;
            _countryRepository = countryRepository;
            _venueRepository = venueRepository;
            _eventRepository = eventRepository;
            _venueService = venueService;
            _eventService = eventService;
        }

        public async Task<Page<CityResponse>> GetCitiesPage(PaginationRequest paginationRequest, FilterationRequest filterRequest)
        {
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var cities = await _cityRepository.GetCitiesPage(paginationRequest, filterRequest);
            return _mapper.Map<Page<CityResponse>>(cities);
        }

        public async Task<List<CityResponse>> GetActiveCities()
        {
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var cities = await _cityRepository.GetActiveCities();
            return _mapper.Map<List<CityResponse>>(cities);
        }

        public async Task<CityResponse> GetCity(string id)
        {
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var city = await _cityRepository.GetById(id);
            if (city == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            return _mapper.Map<CityResponse>(city);
        }

        public async Task<CityResponse> CreateCity(CreateCityRequest createCityRequest)
        {
            var country = await _countryRepository.GetById(createCityRequest.CountryId);
            if (country == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            var cityExists = await _cityRepository.CityExists(createCityRequest?.Name);
            if (cityExists)
                throw new OutOutException(ErrorCodes.CityAlreadyExists);

            var city = _mapper.Map<City>(createCityRequest);
            city.Country = country;

            city = await _cityRepository.Create(city);

            return _mapper.Map<CityResponse>(city);
        }

        public async Task<CityResponse> UpdateCity(string id, UpdateCityRequest updateCityRequest)
        {
            var country = await _countryRepository.GetById(updateCityRequest.CountryId);
            if (country == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            var existingCity = await _cityRepository.GetById(id);
            if (existingCity == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            var cityExists = await _cityRepository.CityExists(updateCityRequest?.Name) && existingCity.Name.ToLower() != updateCityRequest?.Name.ToLower();
            if (cityExists)
                throw new OutOutException(ErrorCodes.CityAlreadyExists);

            if (existingCity.IsActive && !updateCityRequest.IsActive)
            {
                var associatedVenues = await GetAssociatedVenues(id);
                associatedVenues.ForEach(async (venue) => await _venueService.HandleDeactivateVenue(venue, Availability.CityInactive));

                var associatedEventsIds = await GetAssociatedEventsIds(id);
                await _eventRepository.UpdateEventsStatus(associatedEventsIds, Availability.CityInactive);
                await _eventService.HandleDeactivateDeleteEvent(associatedEventsIds);
            }

            var city = _mapper.Map(updateCityRequest, existingCity);
            city.Country = country;
            city = await _cityRepository.UpdateCity(city);
            return _mapper.Map<CityResponse>(city);
        }

        public async Task<bool> DeleteCity(string id)
        {
            var city = await _cityRepository.GetById(id);
            if (city == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            var deleteAcknowledged = await _cityRepository.Delete(id);

            if (deleteAcknowledged)
            {
                var associatedEventsIds = await GetAssociatedEventsIds(id);
                var associatedVenues = await GetAssociatedVenues(id);

                await _venueRepository.DeleteLocationFromVenue(id);
                await _eventRepository.DeleteLocationFromEvent(id);

                associatedVenues.ForEach(async (venue) =>
                     await _venueService.HandleDeactivateVenue(venue, Availability.CityDeleted));

                await _eventRepository.UpdateEventsStatus(associatedEventsIds, Availability.CityDeleted);
                await _eventService.HandleDeactivateDeleteEvent(associatedEventsIds);
            }
            return deleteAcknowledged;
        }

        public async Task<string> CreateArea(string cityId, AreaRequest request)
        {
            var city = await _cityRepository.GetById(cityId);
            if (city == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            var areaExists = city.Areas.ConvertAll(a => a.ToLower()).Contains(request.Area.ToLower());
            if (areaExists)
                throw new OutOutException(ErrorCodes.AreaAlreadyExists);

            city.Areas.Add(request.Area);
            await _cityRepository.Update(city);

            return request.Area;
        }

        public async Task<bool> UpdateArea(string cityId, UpdateAreaRequest request)
        {
            var city = await _cityRepository.GetById(cityId);
            if (city == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            var areaExists = city.Areas.ConvertAll(a => a.ToLower()).Contains(request.NewArea.ToLower()) && request.NewArea.ToLower() != request.OldArea.ToLower();
            if (areaExists)
                throw new OutOutException(ErrorCodes.AreaAlreadyExists);

            var isUpdated = await _cityRepository.UpdateArea(cityId, request);
            if (isUpdated)
            {
                await _venueRepository.UpdateVenuesArea(cityId, request);
                await _eventRepository.UpdateEventsArea(cityId, request);
            }
            return isUpdated;
        }

        public async Task<bool> DeleteArea(string cityId, AreaRequest request)
        {
            var city = await _cityRepository.GetById(cityId);
            if (city == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            var deleteAcknowledged = await _cityRepository.DeleteArea(cityId, request);

            if (deleteAcknowledged)
            {
                var associatedEventsIds = await GetAssociatedEventsIds(cityId, request.Area);
                var associatedVenues = await GetAssociatedVenues(cityId, request.Area);
                var associatedVenuesIds = associatedVenues.Select(a => a.Id).ToList();

                await _venueRepository.DeleteLocationFromVenue(cityId, request.Area);
                await _eventRepository.DeleteLocationFromEvent(cityId, request.Area);

                await _eventRepository.UpdateEventsStatus(associatedEventsIds, Availability.AreaDeleted);
                await _eventService.HandleDeactivateDeleteEvent(associatedEventsIds);

                associatedVenues.ForEach(async (venue) =>
                     await _venueService.HandleDeactivateVenue(venue, Availability.AreaDeleted));
            }
            return deleteAcknowledged;
        }

        private async Task<List<Venue>> GetAssociatedVenues(string cityId, string area = null)
        {
            var associatedVenues = await _venueRepository.GetVenuesByCityId(cityId, area);
            return associatedVenues.ToList();
        }

        private async Task<List<string>> GetAssociatedEventsIds(string cityId, string area = null)
        {
            var associatedEvents = await _eventRepository.GetEventsByCityId(cityId, area);
            return associatedEvents.Select(a => a.Id).ToList();
        }

        public async Task<List<CountryResponse>> GetAllCountries()
        {
            var countries = await _countryRepository.GetAll();
            return _mapper.Map<List<CountryResponse>>(countries);
        }

        public async Task<CountryResponse> GetCountry(string id)
        {
            var country = await _countryRepository.GetById(id);
            if (country == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            return _mapper.Map<CountryResponse>(country);
        }

    }
}
