using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OutOut.Constants.Errors;
using OutOut.Models;
using OutOut.Models.Domains;
using OutOut.Models.Exceptions;
using OutOut.Models.Identity;
using OutOut.Models.Models;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Providers;
using OutOut.ViewModels.Requests.Users;
using OutOut.ViewModels.Requests.Venues;
using OutOut.ViewModels.Responses.Users;
using System.Net;

namespace OutOut.Core.Services
{
    public class LocationService
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserLocationRepository _userLocationRepo;
        private readonly IUserDetailsProvider _userDetailsProvider;
        private readonly ILogger<LocationService> _logger;
        private readonly AppSettings _appSettings;

        public LocationService(IMapper mapper, UserManager<ApplicationUser> userManager, IUserLocationRepository userLocationRepo, IUserDetailsProvider userDetailsProvider, IOptions<AppSettings> appSettings, ILogger<LocationService> logger)
        {
            _mapper = mapper;
            _userManager = userManager;
            _userLocationRepo = userLocationRepo;
            _userDetailsProvider = userDetailsProvider;
            _logger = logger;
            _appSettings = appSettings.Value;
        }

        public async Task<ApplicationUserResponse> GetUserLocation()
        {
            var user = await _userManager.FindByIdAsync(_userDetailsProvider.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var updatedUserLocation = await _userLocationRepo.GetUserLocation(user.Id);

            return _mapper.Map<ApplicationUserResponse>(updatedUserLocation);
        }

        public async Task<ApplicationUserResponse> UpdateUserLocation(UserLocationRequest userLocationRequest)
        {
            if (!await IsLocationInAllowedCountriesAsync(new LocationRequest { Latitude = userLocationRequest.Latitude, Longitude = userLocationRequest.Longitude }))
                throw new OutOutException(ErrorCodes.UnsupportedCountry);

            var user = await _userManager.FindByIdAsync(_userDetailsProvider.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var userLocation = _mapper.Map<UserLocation>(userLocationRequest);
            var updatedUserLocation = await _userLocationRepo.UpdateUserLocation(user.Id, userLocation);

            return _mapper.Map<ApplicationUserResponse>(updatedUserLocation);
        }

        public async Task<bool> IsLocationInAllowedCountriesAsync(LocationRequest locationRequest)
        {
            string baseUrl = "https://maps.googleapis.com/maps/api/geocode/json?latlng=";
            string plusUrl = "&key=" + _appSettings.GeoLocationAPIKey + "&sensor=false";
            
            var json = await new WebClient().DownloadStringTaskAsync(baseUrl + locationRequest.Latitude.ToString().Replace(" ", "") + ","
                + locationRequest.Longitude.ToString().Replace(" ", "") + plusUrl);
            GoogleGeoCodeResponse jsonResult = JsonConvert.DeserializeObject<GoogleGeoCodeResponse>(json);

            string geoLocation = "";

            if (jsonResult.status == "OK")
            {
                for (int i = 0; i < jsonResult.results.Length; i++)
                    geoLocation += jsonResult.results[i].formatted_address;

                if (_appSettings.AllowedCountries.Any(country => geoLocation.Contains(country)))
                    return true;
                else
                    throw new OutOutException(ErrorCodes.UnsupportedCountry);
            }
            else
                return false;
        }
    }
}

