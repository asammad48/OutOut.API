using AutoMapper;
using Microsoft.AspNetCore.Identity;
using OutOut.Constants.Errors;
using OutOut.Models.Exceptions;
using OutOut.Models.Identity;
using OutOut.Models.Models;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Providers;
using OutOut.ViewModels.Requests.Users;
using OutOut.ViewModels.Requests.Venues;
using OutOut.ViewModels.Responses.Users;

namespace OutOut.Core.Services
{
    public class UserLocationService
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserLocationRepository _userLocationRepo;
        private readonly IUserDetailsProvider _userDetailsProvider;
        private readonly LocationService _locationService;

        public UserLocationService(IMapper mapper, UserManager<ApplicationUser> userManager, IUserLocationRepository userLocationRepo, IUserDetailsProvider userDetailsProvider, LocationService locationService)
        {
            _mapper = mapper;
            _userManager = userManager;
            _userLocationRepo = userLocationRepo;
            _userDetailsProvider = userDetailsProvider;
            _locationService = locationService;
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
            if (!await _locationService.IsLocationInAllowedCountriesAsync(new LocationRequest { Latitude = userLocationRequest.Latitude , Longitude = userLocationRequest.Longitude}))
                throw new OutOutException(ErrorCodes.UnsupportedCountry);

            var user = await _userManager.FindByIdAsync(_userDetailsProvider.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var userLocation = _mapper.Map<UserLocation>(userLocationRequest);
            var updatedUserLocation = await _userLocationRepo.UpdateUserLocation(user.Id, userLocation);

            return _mapper.Map<ApplicationUserResponse>(updatedUserLocation);
        }
    }
}
