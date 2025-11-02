using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OutOut.Constants.Enums;
using OutOut.Constants.Errors;
using OutOut.Core.Utils;
using OutOut.Infrastructure.Services;
using OutOut.Models;
using OutOut.Models.Domains;
using OutOut.Models.Exceptions;
using OutOut.Models.Identity;
using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Data;
using OutOut.Persistence.Extensions;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Providers;
using OutOut.ViewModels.Requests.Customers;
using OutOut.ViewModels.Requests.Loyalties;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Requests.Venues;
using OutOut.ViewModels.Responses.Customers;
using OutOut.ViewModels.Responses.EventBookings;
using OutOut.ViewModels.Responses.Events;
using OutOut.ViewModels.Responses.Excel;
using OutOut.ViewModels.Responses.Loyalties;
using OutOut.ViewModels.Responses.Users;
using OutOut.ViewModels.Responses.VenueBooking;
using OutOut.ViewModels.Responses.Venues;
using OutOut.ViewModels.Wrappers;
using System.Net;

namespace OutOut.Core.Services
{
    public class CustomerService
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppSettings _appSettings;
        private readonly IUserDetailsProvider _userDetailsProvider;
        private readonly IUserRepository _userRepository;
        private readonly FileUploaderService _fileUploader;
        private readonly IVenueRepository _venuesRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IVenueBookingRepository _venueBookingRepository;
        private readonly IEventBookingRepository _eventBookingRepository;
        private readonly IUserLoyaltyRepository _userLoyaltyRepository;
        private readonly IUserOfferRepository _userOfferRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly ApplicationNonSqlDbContext _dbContext;
        private IMongoCollection<ApplicationUser> _usersCollection;
        public CustomerService(IMapper mapper,
                               UserManager<ApplicationUser> userManager,
                               IOptions<AppSettings> appSettings,
                               IUserDetailsProvider userDetailsProvider,
                               FileUploaderService fileUploader,
                               IVenueRepository venuesRepository,
                               IUserLoyaltyRepository userLoyaltyRepository,
                               IVenueBookingRepository venueBookingRepository,
                               IEventRepository eventRepository,
                               INotificationRepository notificationRepository,
                               IEventBookingRepository eventBookingRepository,
                               IUserRepository userRepository,
                               IUserOfferRepository userOfferRepository,
                               ApplicationNonSqlDbContext dbContext)
        {
            _mapper = mapper;
            _userManager = userManager;
            _appSettings = appSettings.Value;
            _userDetailsProvider = userDetailsProvider;
            _fileUploader = fileUploader;
            _venuesRepository = venuesRepository;
            _userLoyaltyRepository = userLoyaltyRepository;
            _venueBookingRepository = venueBookingRepository;
            _eventRepository = eventRepository;
            _notificationRepository = notificationRepository;
            _eventBookingRepository = eventBookingRepository;
            _userRepository = userRepository;
            _userOfferRepository = userOfferRepository;
            _dbContext = dbContext;
            _usersCollection = _dbContext.GetCollection<ApplicationUser>();
        }

        public ApplicationUserResponse GetAccountInfo()
        {
            var currentUser = _userDetailsProvider.User;
            if (currentUser == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            return _mapper.Map<ApplicationUserResponse>(currentUser);
        }

        public async Task<ApplicationUserResponse> UpdateAccountInfoAsync(CustomerUpdateAccountRequest request)
        {
            var currentUser = _userDetailsProvider.User;
            if (currentUser == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            currentUser.FullName = request.FullName != null ? request.FullName : currentUser.FullName;
            currentUser.PhoneNumber = request.PhoneNumber;
            currentUser.Gender = request.Gender;

            if (request.ProfileImage != null)
            {
                var oldProfileImage = currentUser.ProfileImage;
                var newProfileImage = await _fileUploader.UploadFile(_appSettings.Directories.ProfileImages, request.ProfileImage);

                if (string.IsNullOrEmpty(newProfileImage))
                    throw new OutOutException(ErrorCodes.CouldntUpdateProfileImage, HttpStatusCode.InternalServerError);

                currentUser.ProfileImage = newProfileImage;
                _fileUploader.DeleteFile(_appSettings.Directories.ProfileImages, oldProfileImage);
            }
            if (request.RemoveProfileImage)
            {
                _fileUploader.DeleteFile(_appSettings.Directories.ProfileImages, currentUser.ProfileImage);
                currentUser.ProfileImage = null;
            }

            var existingUser = await _userRepository.GetUserById(_userDetailsProvider.UserId);

            var identityResult = await _userManager.UpdateAsync(currentUser);
            if (!identityResult.Succeeded)
                throw new OutOutException(identityResult);

            await SyncUserInfoInBookings(existingUser, currentUser);

            return _mapper.Map<ApplicationUserResponse>(currentUser);
        }

        public async Task<Page<ApplicationUserSummaryResponse>> GetCustomersPage(PaginationRequest paginationRequest, FilterationRequest filterRequest)
        {
            if (_userDetailsProvider.IsSuperAdmin)
            {
                var customersPage = await _userRepository.GetCustomersPage(paginationRequest, filterRequest);
                return _mapper.Map<Page<ApplicationUserSummaryResponse>>(customersPage);
            }

            else
            {
                var customersBookedVenues = await _venueBookingRepository.GetUsersByVenueIds(_userDetailsProvider.User.AccessibleVenues, filterRequest);
                var customersBookedEvents = await _eventBookingRepository.GetUsersByEventIds(_userDetailsProvider.GetAccessibleEvents(), filterRequest);
                
                
                var users = customersBookedVenues.Select(u => u.Id).Concat(customersBookedEvents.Select(u => u.Id)).ToList();

                var customersPage = await _userRepository.GetCustomersByIds(paginationRequest, filterRequest, users);
                return _mapper.Map<Page<ApplicationUserSummaryResponse>>(customersPage);
            }
        }

        public async Task<FileResponse> ExportAllCustomersInfoToExcel()
        {
            if (_userDetailsProvider.IsSuperAdmin)
            {
                var options = new FindOptions<ApplicationUser> { BatchSize = 10 };
                var cursor = await _usersCollection.FindAsync(Builders<ApplicationUser>.Filter.Size(a => a.Roles, 0), options);
                var file = ExcelUtils.ExportToExcel(cursor, user => _mapper.Map<CustomerDTO>(user)).ToArray();
                return new FileResponse(file, "Customers.xlsx");
            }
            else
            {
                var customersBookedVenues = await _venueBookingRepository.GetUsersByVenueIds(_userDetailsProvider.User.AccessibleVenues);
                var customersBookedEvents = await _eventBookingRepository.GetUsersByEventIds(_userDetailsProvider.GetAccessibleEvents());
                var customers = customersBookedVenues.Concat(customersBookedEvents).GroupBy(a => a?.Id).Select(a => a.FirstOrDefault());

                var customersDTO = _mapper.Map<List<CustomerDTO>>(customers.OrderBy(a => a.FullName).ToList());
                var file = ExcelUtils.ExportToExcel(customersDTO).ToArray();

                return new FileResponse(file, "Customers.xlsx");
            }
        }

        public async Task<ApplicationUserSummaryResponse> GetCustomer(string id)
        {
            var customer = await _userManager.FindByIdAsync(id);
            if (customer == null)
                throw new OutOutException(ErrorCodes.CustomerNotFound);

            return _mapper.Map<ApplicationUserSummaryResponse>(customer);
        }

        public async Task<FileResponse> ExportCustomerInfoToExcel(string id)
        {
            var customer = await _userManager.FindByIdAsync(id);
            if (customer == null || customer?.Roles?.Count > 0)
                throw new OutOutException(ErrorCodes.CustomerNotFound);

            var data = _mapper.Map<CustomerDTO>(customer);
            var file = ExcelUtils.ExportToExcel(new List<CustomerDTO> { data }).ToArray();
            return new FileResponse(file, $"{customer.FullName} Info.xlsx");
        }

        public async Task<Page<CustomerEventBookingResponse>> GetCustomersAttendedEventsPage(string userId, PaginationRequest paginationRequest, SearchFilterationRequest searchFilterationRequest)
        {
            var customer = await _userManager.FindByIdAsync(userId);
            if (customer == null || customer?.Roles?.Count > 0)
                throw new OutOutException(ErrorCodes.CustomerNotFound);

            var eventBookings = await _eventBookingRepository.GetCustomerAttendedEvents(userId, searchFilterationRequest, _userDetailsProvider.GetAccessibleEvents(), _userDetailsProvider.IsSuperAdmin);
            eventBookings = eventBookings.GroupBy(a => a.Event.Occurrence.Id).Select(a => a.FirstOrDefault()).ToList();

            var result = _mapper.Map<Page<CustomerEventBookingResponse>>(eventBookings.OrderByDescending(b => b.Tickets.FirstOrDefault().RedemptionDate).GetPaged(paginationRequest));
            result.Records.ForEach(booking => booking.RedeemedTicketsCount = _eventBookingRepository.GetCustomersRedeemedTicketsCountPerOccurrence(userId, booking.EventOccurrenceId));

            return result;
        }

        public async Task<Page<CustomerLoyaltyResponse>> GetCustomersAvailedLoyaltyPage(string userId, PaginationRequest paginationRequest, SearchFilterationRequest searchFilterationRequest)
        {
            var customer = await _userManager.FindByIdAsync(userId);
            if (customer == null || customer?.Roles?.Count > 0)
                throw new OutOutException(ErrorCodes.CustomerNotFound);

            var records = _userLoyaltyRepository.GetCustomersAvailedLoyalty(userId, paginationRequest, searchFilterationRequest, _userDetailsProvider.User.AccessibleVenues, _userDetailsProvider.IsSuperAdmin);
            return _mapper.Map<Page<CustomerLoyaltyResponse>>(records);
        }

        public async Task<Page<CustomerOfferResponse>> GetCustomersAvailedOffersPage(string userId, PaginationRequest paginationRequest, SearchFilterationRequest searchFilterationRequest)
        {
            var customer = await _userManager.FindByIdAsync(userId);
            if (customer == null || customer?.Roles?.Count > 0)
                throw new OutOutException(ErrorCodes.CustomerNotFound);

            var records = await _userOfferRepository.GetCustomersAvailedOffers(userId, paginationRequest, searchFilterationRequest, _userDetailsProvider.User.AccessibleVenues, _userDetailsProvider.IsSuperAdmin);
            return _mapper.Map<Page<CustomerOfferResponse>>(records);
        }

        public async Task<ApplicationUserResponse> UpdateNotificationsAllowed(UpdateNotificationsAllowedRequest request)
        {
            var currentUser = _userDetailsProvider.User;
            if (currentUser == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            currentUser.NotificationsAllowed = request.NotificationsAllowed;

            var identityResult = await _userManager.UpdateAsync(currentUser);
            if (!identityResult.Succeeded)
                throw new OutOutException(identityResult);

            return _mapper.Map<ApplicationUserResponse>(currentUser);
        }

        public async Task<ApplicationUserResponse> UpdateRemindersAllowed(UpdateRemindersAllowedRequest request)
        {
            var currentUser = _userDetailsProvider.User;
            if (currentUser == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            currentUser.RemindersAllowed = request.RemindersAllowed;

            var identityResult = await _userManager.UpdateAsync(currentUser);
            if (!identityResult.Succeeded)
                throw new OutOutException(identityResult);

            return _mapper.Map<ApplicationUserResponse>(currentUser);
        }

        public async Task<bool> DeleteUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return false;

            return true;
        }

        public async Task<Page<VenueSummaryResponse>> GetFavoriteVenues(PaginationRequest paginationRequest, SearchFilterationRequest filterRequest)
        {
            var user = await _userManager.FindByIdAsync(_userDetailsProvider.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var favoriteVenueIds = user.FavoriteVenues.AsEnumerable().Reverse().ToList();
            var paginatedFavoriteVenueIds = favoriteVenueIds.Paginate(paginationRequest);

            var favoriteVenues = await _venuesRepository.GetUsersFavoriteVenues(paginatedFavoriteVenueIds, filterRequest);

            var favoriteVenuesPage = new Page<Venue>(favoriteVenues, paginationRequest.PageNumber, paginationRequest.PageSize, favoriteVenues.Count());
            return _mapper.Map<Page<VenueSummaryResponse>>(favoriteVenuesPage);
        }

        public async Task<Page<EventSummaryResponse>> GetFavoriteEventsOccurrences(PaginationRequest paginationRequest, SearchFilterationRequest filterRequest)
        {
            var user = await _userManager.FindByIdAsync(_userDetailsProvider.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var favoriteEventOccurrenceIds = user.FavoriteEvents.AsEnumerable().Reverse().ToList();
            var paginatedFavoriteEventIds = favoriteEventOccurrenceIds.Paginate(paginationRequest);

            var favoriteEvents = await _eventRepository.GetUsersFavoriteEvents(paginatedFavoriteEventIds, filterRequest);

            var favoriteEventsPage = new Page<SingleEventOccurrence>(favoriteEvents, paginationRequest.PageNumber, paginationRequest.PageSize, favoriteEvents.Count());
            return _mapper.Map<Page<EventSummaryResponse>>(favoriteEventsPage);
        }

        public async Task<Page<LoyaltyResponse>> GetMyLoyalty(PaginationRequest paginationRequest, LoyaltyFilterationRequest request)
        {
            var user = await _userManager.FindByIdAsync(_userDetailsProvider.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var userLoyaltyList = await _userLoyaltyRepository.GetUserLoyalty(request, user.Id);
            userLoyaltyList.ToList().ForEach(userLoyalty =>
                {
                    var venue = _venuesRepository.GetVenueById(userLoyalty.Venue.Id);
                    if (venue.Status != Availability.Active)
                        userLoyaltyList.Remove(userLoyalty);
                });
            return _mapper.Map<Page<LoyaltyResponse>>(userLoyaltyList.GetPaged(paginationRequest));
        }

        public async Task<Page<VenueBookingResponse>> GetMyVenueBookings(PaginationRequest paginationRequest, MyBookingFilterationRequest request)
        {
            var user = await _userManager.FindByIdAsync(_userDetailsProvider.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var venueBooking = await _venueBookingRepository.GetMyBooking(paginationRequest, request, user.Id);
            return _mapper.Map<Page<VenueBookingResponse>>(venueBooking);
        }

        public async Task<Page<EventBookingSummaryResponse>> GetMyEventBookings(PaginationRequest paginationRequest, MyBookingFilterationRequest request)
        {
            var user = await _userManager.FindByIdAsync(_userDetailsProvider.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var eventBooking = await _eventBookingRepository.GetMyBooking(paginationRequest, request, user.Id);
            return _mapper.Map<Page<EventBookingSummaryResponse>>(eventBooking);
        }

        public async Task<Page<SingleEventBookingTicketSummaryResponse>> GetMyEventSharedTickets(PaginationRequest paginationRequest, MyBookingFilterationRequest request)
        {
            var user = await _userManager.FindByIdAsync(_userDetailsProvider.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var eventBooking = await _eventBookingRepository.GetMySharedTickets(paginationRequest, request, user.SharedTickets);
            return _mapper.Map<Page<SingleEventBookingTicketSummaryResponse>>(eventBooking);
        }

        public async Task SyncUserInfoInBookings(ApplicationUser oldUser, ApplicationUser newUser)
        {
            await _eventBookingRepository.SyncUserWithEventBookings(oldUser, newUser);
            await _venueBookingRepository.SyncUserWithVenueBookings(oldUser, newUser);
        }
    }
}
