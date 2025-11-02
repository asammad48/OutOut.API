using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OutOut.Constants;
using OutOut.Constants.Errors;
using OutOut.Infrastructure.Services;
using OutOut.Models;
using OutOut.Models.Exceptions;
using OutOut.Models.Identity;
using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Providers;
using OutOut.ViewModels.Requests.AdminProfile;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Responses.Events;
using OutOut.ViewModels.Responses.Users;
using OutOut.ViewModels.Responses.Venues;
using OutOut.ViewModels.Wrappers;
using System.Net;

namespace OutOut.Core.Services
{
    public class AdminProfileService
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly AppSettings _appSettings;
        private readonly IUserDetailsProvider _userDetailsProvider;
        private readonly FileUploaderService _fileUploader;
        private readonly IVenueRepository _venueRepository;
        private readonly IEventRepository _eventRepository;

        public AdminProfileService(IMapper mapper,
                              UserManager<ApplicationUser> userManager,
                              IOptions<AppSettings> appSettings,
                              IUserDetailsProvider userDetailsProvider,
                              FileUploaderService fileUploader,
                              IVenueRepository venueRepository,
                              IEventRepository eventRepository,
                              IUserRepository userRepository)
        {
            _mapper = mapper;
            _userManager = userManager;
            _appSettings = appSettings.Value;
            _userDetailsProvider = userDetailsProvider;
            _fileUploader = fileUploader;
            _venueRepository = venueRepository;
            _eventRepository = eventRepository;
            _userRepository = userRepository;
        }

        public ApplicationUserAdminResponse GetMyAccountInfo()
        {
            var currentUser = _userDetailsProvider.User;
            if (currentUser == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var userResult = _mapper.Map<ApplicationUserAdminResponse>(currentUser);
            userResult.Roles = _userDetailsProvider.UserRoles;

            return userResult;
        }

        public async Task<Page<VenueSummaryWithBookingResponse>> GetVenuesCreatedByMe(PaginationRequest paginationRequest)
        {
            var currentUser = _userDetailsProvider.User;
            if (currentUser == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            return await GetUserVenues(paginationRequest, _userDetailsProvider.UserId);
        }

        public async Task<Page<EventSummaryWithBookingResponse>> GetEventsCreatedByMe(PaginationRequest paginationRequest)
        {
            var currentUser = _userDetailsProvider.User;
            if (currentUser == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            return await GetUserEvents(paginationRequest, _userDetailsProvider.UserId);
        }

        public async Task<ApplicationUserAdminResponse> UpdateMyAccountInfo(UpdateAdminAccountRequest request)
        {
            var currentUser = _userDetailsProvider.User;
            if (currentUser == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            if (_userRepository.EmailExistsForOtherUsers(currentUser.Id, request.Email))
                throw new OutOutException(ErrorCodes.DuplicateEmail);

            currentUser.FullName = request.FullName;
            currentUser.PhoneNumber = request.PhoneNumber;
            currentUser.CompanyName = request.CompanyName;
            currentUser.Email = request.Email;

            if (request.ProfileImage != null)
            {
                var oldProfileImage = currentUser.ProfileImage;
                var newProfileImage = await _fileUploader.UploadFile(_appSettings.Directories.ProfileImages, request.ProfileImage);

                if (string.IsNullOrEmpty(newProfileImage))
                    throw new OutOutException(ErrorCodes.CouldntUpdateProfileImage, HttpStatusCode.InternalServerError);

                currentUser.ProfileImage = newProfileImage;
                _fileUploader.DeleteFile(_appSettings.Directories.ProfileImages, oldProfileImage);
            }

            var identityResult = await _userManager.UpdateAsync(currentUser);
            if (!identityResult.Succeeded)
                throw new OutOutException(identityResult);

            var response = _mapper.Map<ApplicationUserAdminResponse>(currentUser);
            response.Roles = _userDetailsProvider.UserRoles;

            return response;
        }

        public async Task<Page<ApplicationUserAdminResponse>> GetUsersPage(PaginationRequest paginationRequest, FilterationRequest filterationRequest)
        {
            var usersPage = await _userRepository.GetUsers(paginationRequest, filterationRequest, _userDetailsProvider.UserId);
            foreach (var user in usersPage.Records)
            {
                var roles = await _userManager.GetRolesAsync(user);
                user.Roles = roles.ToList();
            }
            return _mapper.Map<Page<ApplicationUserAdminResponse>>(usersPage);
        }

        public async Task<ApplicationUserAdminResponse> GetUserAccountInfo(string userId)
        {
            var currentUser = _userDetailsProvider.User;
            if (currentUser == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var userResult = _mapper.Map<ApplicationUserAdminResponse>(user);

            var userRoles = await _userManager.GetRolesAsync(user);
            userResult.Roles = userRoles.ToList();

            return userResult;
        }

        public async Task<Page<VenueSummaryWithBookingResponse>> GetUserVenues(PaginationRequest paginationRequest, string userId)
        {
            var currentUser = _userDetailsProvider.User;
            if (currentUser == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var venues = await _venueRepository.GetVenuesByUserId(paginationRequest, userId);
            return _mapper.Map<Page<VenueSummaryWithBookingResponse>>(venues);
        }

        public async Task<Page<EventSummaryWithBookingResponse>> GetUserEvents(PaginationRequest paginationRequest, string userId)
        {
            var currentUser = _userDetailsProvider.User;
            if (currentUser == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var events = await _eventRepository.GetEventsByUserId(paginationRequest, userId);

            return _mapper.Map<Page<EventSummaryWithBookingResponse>>(events);
        }

        public async Task<Page<VenueSummaryWithBookingResponse>> GetVenuesUserAdminOn(PaginationRequest paginationRequest, string userId)
        {
            var currentUser = _userDetailsProvider.User;
            if (currentUser == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var user = await _userRepository.GetUserById(userId);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var venues = await _venueRepository.GetVenuesUserAdminOn(paginationRequest, user.AccessibleVenues);
            return _mapper.Map<Page<VenueSummaryWithBookingResponse>>(venues);
        }

        public async Task<ApplicationUserAdminResponse> CreateUser(AdminProfileRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                throw new OutOutException(ErrorCodes.DuplicateEmail);

            if (request.AccessibleVenues != null && request.AccessibleVenues.Any())
            {
                foreach (var venueId in request.AccessibleVenues)
                {
                    var existingVenue = await _venueRepository.GetById(venueId);
                    if (existingVenue == null)
                        throw new OutOutException(ErrorCodes.VenueNotFound);
                }
            }
            if (request.AccessibleEvents != null && request.AccessibleEvents.Any())
            {
                foreach (var eventId in request.AccessibleEvents)
                {
                    var existingEvent = await _eventRepository.GetById(eventId);
                    if (existingEvent == null)
                        throw new OutOutException(ErrorCodes.EventNotFound);
                }
            }

            var newUser = _mapper.Map<ApplicationUser>(request);

            if (request.ProfileImage != null)
            {
                var newProfileImage = await _fileUploader.UploadFile(_appSettings.Directories.ProfileImages, request.ProfileImage);

                if (string.IsNullOrEmpty(newProfileImage))
                    throw new OutOutException(ErrorCodes.CouldntUpdateProfileImage, HttpStatusCode.InternalServerError);

                newUser.ProfileImage = newProfileImage;
            }

            var isNewRoleValid = Roles.ALL_ROLES.Contains(request.Role);
            if (!isNewRoleValid)
                throw new OutOutException(ErrorCodes.InvalidRole);

            newUser.EmailConfirmed = true;
            newUser.Location = new UserLocation(_appSettings.DefaultUserLocation.Longitude, _appSettings.DefaultUserLocation.Latitude, _appSettings.DefaultUserLocation.Description);
            var identityResult = await _userManager.CreateAsync(newUser, request.Password);
            await _userManager.AddToRoleAsync(newUser, request.Role);

            if (!identityResult.Succeeded)
                throw new OutOutException(identityResult);

            var response = _mapper.Map<ApplicationUserAdminResponse>(newUser);
            var newRoles = await _userManager.GetRolesAsync(newUser);
            response.Roles = newRoles.ToList();

            return response;
        }

        public async Task<ApplicationUserAdminResponse> UpdateUser(string id, UpdateAdminProfileRequest request)
        {
            var existingUser = await _userManager.FindByIdAsync(id);
            if (existingUser == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            if (_userRepository.EmailExistsForOtherUsers(id, request.Email))
                throw new OutOutException(ErrorCodes.DuplicateEmail);

            if (request.AccessibleVenues != null && request.AccessibleVenues.Any())
            {
                foreach (var venueId in request.AccessibleVenues)
                {
                    var existingVenue = await _venueRepository.GetById(venueId);
                    if (existingVenue == null)
                        throw new OutOutException(ErrorCodes.VenueNotFound);
                }
            }
            if (request.AccessibleEvents != null && request.AccessibleEvents.Any())
            {
                foreach (var eventId in request.AccessibleEvents)
                {
                    var existingEvent = await _eventRepository.GetById(eventId);
                    if (existingEvent == null)
                        throw new OutOutException(ErrorCodes.EventNotFound);
                }
            }

            var isNewRoleValid = Roles.ALL_ROLES.Contains(request.Role);
            if (!isNewRoleValid)
                throw new OutOutException(ErrorCodes.InvalidRole);

            existingUser.FullName = request.FullName;
            existingUser.PhoneNumber = request.PhoneNumber;
            existingUser.CompanyName = request.CompanyName;
            existingUser.Email = request.Email;
            existingUser.AccessibleVenues = request.AccessibleVenues ?? new List<string>();
            existingUser.AccessibleEvents = request.AccessibleEvents ?? new List<string>();

            if (request.Role.Equals(Roles.SuperAdmin))
            {
                existingUser.AccessibleVenues.Clear();
                existingUser.AccessibleEvents.Clear();
            }

            else if (request.Role.Equals(Roles.EventAdmin))
                existingUser.AccessibleVenues.Clear();

            if (request.ProfileImage != null)
            {
                var oldProfileImage = existingUser.ProfileImage;
                var newProfileImage = await _fileUploader.UploadFile(_appSettings.Directories.ProfileImages, request.ProfileImage);

                if (string.IsNullOrEmpty(newProfileImage))
                    throw new OutOutException(ErrorCodes.CouldntUpdateProfileImage, HttpStatusCode.InternalServerError);

                existingUser.ProfileImage = newProfileImage;
                _fileUploader.DeleteFile(_appSettings.Directories.ProfileImages, oldProfileImage);
            }

            if (!string.IsNullOrEmpty(request.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(existingUser);
                var identityResultResetPassword = await _userManager.ResetPasswordAsync(existingUser, token, request.Password);
                if (!identityResultResetPassword.Succeeded)
                    throw new OutOutException(identityResultResetPassword);
            }

            var existingRoles = await _userManager.GetRolesAsync(existingUser);
            await _userManager.RemoveFromRolesAsync(existingUser, existingRoles.ToList());
            await _userManager.AddToRoleAsync(existingUser, request.Role);

            var identityResult = await _userManager.UpdateAsync(existingUser);
            if (!identityResult.Succeeded)
                throw new OutOutException(identityResult);

            var newRoles = await _userManager.GetRolesAsync(existingUser);
            var response = _mapper.Map<ApplicationUserAdminResponse>(existingUser);
            response.Roles = newRoles.ToList();

            return response;
        }

        public async Task<bool> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return false;

            return true;
        }

        public List<string> GetSystemRoles() => Roles.ALL_ROLES.ToList();

        public async Task<List<string>> GetMyAccessibleVenues()
        {
            await _userDetailsProvider.ReInitialize();
            return _userDetailsProvider.User.AccessibleVenues;
        }

        public async Task<List<string>> GetMyAccessibleEvents()
        {
            await _userDetailsProvider.ReInitialize();
            return _userDetailsProvider.GetAccessibleEvents();
        }

    }
}
