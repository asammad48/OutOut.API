using AutoMapper;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using OutOut.Constants;
using OutOut.Constants.Enums;
using OutOut.Constants.Errors;
using OutOut.Core.Utils;
using OutOut.Infrastructure.Services;
using OutOut.Models.Exceptions;
using OutOut.Models.Identity;
using OutOut.Models.Models;
using OutOut.Models.Models.Embedded;
using OutOut.Models.Utils;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Providers;
using OutOut.ViewModels.Requests.Loyalties;
using OutOut.ViewModels.Requests.LoyaltyTypes;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Responses.Loyalties;
using OutOut.ViewModels.Responses.LoyaltyTypes;
using OutOut.ViewModels.Responses.Venues;
using OutOut.ViewModels.Wrappers;
using System.Net;

namespace OutOut.Core.Services
{
    public class LoyaltyService
    {
        private readonly IVenueRepository _venueRepository;
        private readonly IVenueRequestRepository _venueRequestRepository;
        private readonly ILoyaltyTypeRepository _loyaltyTypeRepository;
        private readonly IMapper _mapper;
        private readonly IUserDetailsProvider _userDetailsProvider;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserLoyaltyRepository _userLoyaltyRepository;
        private readonly ITimeProvider _timeProvider;
        private readonly IUserRepository _userRepository;
        private readonly NotificationComposerService _notificationComposerService;
        private readonly INotificationRepository _notificationRepository;

        public LoyaltyService(IVenueRepository venueRepository,
                              IMapper mapper,
                              IUserDetailsProvider userDetailsProvider,
                              UserManager<ApplicationUser> userManager,
                              IUserLoyaltyRepository userLoyaltyRepository,
                              ILoyaltyTypeRepository loyaltyTypeRepository,
                              IVenueRequestRepository venueRequestRepository,
                              ITimeProvider timeProvider,
                              IUserRepository userRepository,
                              NotificationComposerService notificationComposerService,
                              INotificationRepository notificationRepository)
        {
            _venueRepository = venueRepository;
            _mapper = mapper;
            _userDetailsProvider = userDetailsProvider;
            _userManager = userManager;
            _userLoyaltyRepository = userLoyaltyRepository;
            _loyaltyTypeRepository = loyaltyTypeRepository;
            _venueRequestRepository = venueRequestRepository;
            _timeProvider = timeProvider;
            _userRepository = userRepository;
            _notificationComposerService = notificationComposerService;
            _notificationRepository = notificationRepository;
        }

        public async Task<Page<LoyaltyTypeResponse>> GetLoyaltyTypesPage(PaginationRequest paginationRequest, FilterationRequest filterationRequest)
        {
            var response = await _loyaltyTypeRepository.GetLoyaltyTypesPage(paginationRequest, filterationRequest);
            return _mapper.Map<Page<LoyaltyTypeResponse>>(response);
        }

        public async Task<List<LoyaltyTypeSummaryResponse>> GetAllLoyaltyTypes()
        {
            var response = await _loyaltyTypeRepository.GetAllLoyaltyTypes();
            return _mapper.Map<List<LoyaltyTypeSummaryResponse>>(response);
        }

        public async Task<LoyaltyTypeResponse> GetLoyaltyType(string id)
        {
            var loyaltyType = await _loyaltyTypeRepository.GetById(id);
            if (loyaltyType == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            return _mapper.Map<LoyaltyTypeResponse>(loyaltyType);
        }

        public async Task<LoyaltyTypeResponse> AddLoyaltyType(CreateLoyaltyTypeRequest request)
        {
            var loyaltyType = _mapper.Map<LoyaltyType>(request);
            var response = await _loyaltyTypeRepository.Create(loyaltyType);
            return _mapper.Map<LoyaltyTypeResponse>(response);
        }

        public async Task<LoyaltyTypeResponse> UpdateLoyaltyType(string id, CreateLoyaltyTypeRequest request)
        {
            var loyaltyType = await _loyaltyTypeRepository.GetById(id);
            if (loyaltyType == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            loyaltyType = _mapper.Map(request, loyaltyType);
            loyaltyType = await _loyaltyTypeRepository.Update(loyaltyType);
            return _mapper.Map<LoyaltyTypeResponse>(loyaltyType);
        }

        public async Task<bool> DeleteLoyaltyType(string id)
        {
            var loyaltyType = await _loyaltyTypeRepository.GetById(id);
            if (loyaltyType == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            await _venueRepository.DeleteLoyalty(id);
            await _userLoyaltyRepository.DeleteUserLoyaltyByType(id);

            return await _loyaltyTypeRepository.Delete(id);
        }

        public async Task<Page<AssignedLoyaltyResponse>> GetAssignedLoyaltyPage(PaginationRequest paginationRequest, FilterationRequest filterationRequest)
        {
            await _userDetailsProvider.ReInitialize();

            var assignedLoyaltyPage = await _venueRepository.GetVenuesWithAssignedLoyalty(paginationRequest, filterationRequest, _userDetailsProvider.User.AccessibleVenues, _userDetailsProvider.IsSuperAdmin);
            return _mapper.Map<Page<AssignedLoyaltyResponse>>(assignedLoyaltyPage);
        }

        public async Task<AssignedLoyaltyResponse> GetAssignedLoyalty(string loyaltyId)
        {
            var venueWithAssignedLoyalty = await _venueRepository.GetByLoyaltyId(loyaltyId);

            if (venueWithAssignedLoyalty == null)
                throw new OutOutException(ErrorCodes.VenueNotFound);
            if (venueWithAssignedLoyalty?.Loyalty == null)
                throw new OutOutException(ErrorCodes.LoyaltyNotFound);
            await _userDetailsProvider.ReInitialize();
            if (!_userDetailsProvider.HasAccessToVenue(venueWithAssignedLoyalty.Id) && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisVenue, HttpStatusCode.Forbidden);

            return _mapper.Map<AssignedLoyaltyResponse>(venueWithAssignedLoyalty);
        }

        public async Task<AssignedLoyaltyResponse> AssignLoyaltyToVenue(AssignedLoyaltyRequest request)
        {
            var type = await _loyaltyTypeRepository.GetById(request.TypeId);
            if (type == null)
                throw new OutOutException(ErrorCodes.LoyaltyTypeNotFound);

            var venue = await _venueRepository.GetById(request.VenueId);
            if (venue == null)
                throw new OutOutException(ErrorCodes.VenueNotFound);

            await _userDetailsProvider.ReInitialize();
            if (!_userDetailsProvider.HasAccessToVenue(venue.Id) && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisVenue, HttpStatusCode.Forbidden);

            if (venue.Status != Availability.Active)
                throw new OutOutException(ErrorCodes.UnavailableVenue);
            if (venue.Loyalty != null)
                throw new OutOutException(ErrorCodes.VenueHasAssignedLoyalty);

            var newAssignedLoyalty = new Loyalty
            {
                Type = type,
                Stars = request.Stars,
                IsActive = request.IsActive,
                MaxUsage = request.MaxUsage,
                ValidOn = _mapper.Map<List<AvailableTime>>(request.ValidOn)
            };

            var updatedVenue = venue;
            updatedVenue.Loyalty = newAssignedLoyalty;

            await _venueRequestRepository.UpsertVenueRequest(updatedVenue, venue, RequestType.AssignLoyalty, newAssignedLoyalty.Id);
            var venueRequest = await _venueRequestRepository.GetVenueRequestByVenueId(venue.Id, RequestType.AssignLoyalty);

            if (!_userDetailsProvider.IsSuperAdmin)
                await SendAssignLoyaltyToVenueNotification(venueRequest.Id, venue.Name);

            if (_userDetailsProvider.IsSuperAdmin)
            {
                await _venueRepository.AssignLoyalty(venue.Id, newAssignedLoyalty);
                await _venueRequestRepository.DeleteVenueRequest(venue.Id, RequestType.AssignLoyalty);
            }

            return _mapper.Map<AssignedLoyaltyResponse>(venue);
        }

        public async Task<bool> UpdateAssignedLoyalty(string loyaltyId, AssignedLoyaltyRequest request)
        {
            var venueWithLoyalty = await _venueRepository.GetByLoyaltyId(loyaltyId);
            if (venueWithLoyalty == null)
                throw new OutOutException(ErrorCodes.LoyaltyNotFound);

            var venue = await _venueRepository.GetById(request.VenueId);
            if (venue == null)
                throw new OutOutException(ErrorCodes.VenueNotFound);

            await _userDetailsProvider.ReInitialize();
            if (!_userDetailsProvider.HasAccessToVenue(venue.Id) && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisVenue, HttpStatusCode.Forbidden);

            var type = await _loyaltyTypeRepository.GetById(request.TypeId);
            if (type == null)
                throw new OutOutException(ErrorCodes.LoyaltyTypeNotFound);

            bool isLoyaltyDeactivated = venueWithLoyalty.Loyalty.IsActive && !request.IsActive;
            if (venueWithLoyalty.Loyalty.Stars != request.Stars || venueWithLoyalty.Loyalty?.Type.Id != request.TypeId)
            {
                var usersLoyalies = await _userLoyaltyRepository.GetUserLoyaltiesByLoyaltyId(venueWithLoyalty.Loyalty.Id);
                foreach (var userLoyalty in usersLoyalies)
                {
                    userLoyalty.Redemptions = new List<Redemption>();
                    userLoyalty.Loyalty.Stars = request.Stars;
                    userLoyalty.Loyalty.Type = type;
                    await _userLoyaltyRepository.Update(userLoyalty);
                }
            }
            venueWithLoyalty.Loyalty.Type = type;
            venueWithLoyalty.Loyalty.Stars = request.Stars;
            venueWithLoyalty.Loyalty.IsActive = request.IsActive;
            venueWithLoyalty.Loyalty.MaxUsage = request.MaxUsage;
            venueWithLoyalty.Loyalty.ValidOn = _mapper.Map<List<AvailableTime>>(request.ValidOn);

            if (venueWithLoyalty.Loyalty.ToBsonDocument().Equals(venue.Loyalty.ToBsonDocument()))
                throw new OutOutException(ErrorCodes.NoChangesHaveBeenMade);

            if (venueWithLoyalty.Id != request.VenueId)
            {
                await UnAssignLoyaltyFromVenue(venueWithLoyalty.Id, loyaltyId);

                var updatedVenue = venue;
                updatedVenue.Loyalty = venueWithLoyalty.Loyalty;
                venue.Loyalty.Id = ObjectId.GenerateNewId().ToString();
                venue.Loyalty.AssignDate = DateTime.UtcNow;

                await _venueRequestRepository.UpsertVenueRequest(updatedVenue, venue, RequestType.AssignLoyalty, venue.Loyalty.Id);
                var venueRequest = await _venueRequestRepository.GetVenueRequestByVenueId(venue.Id, RequestType.AssignLoyalty);

                if (!_userDetailsProvider.IsSuperAdmin)
                    await SendAssignLoyaltyToVenueNotification(venueRequest.Id, updatedVenue.Name);

                if (_userDetailsProvider.IsSuperAdmin)
                {
                    await _venueRepository.AssignLoyalty(venue.Id, venue.Loyalty);
                    await _venueRequestRepository.DeleteVenueRequest(venue.Id, RequestType.AssignLoyalty);
                }
            }
            else
            {
                var updatedVenue = venue;
                updatedVenue.Loyalty = venueWithLoyalty.Loyalty;
                await _venueRequestRepository.UpsertVenueRequest(updatedVenue, venue, RequestType.UpdateLoyalty, loyaltyId);
                var venueRequest = await _venueRequestRepository.GetVenueRequestByVenueId(venue.Id, RequestType.UpdateLoyalty);

                if (isLoyaltyDeactivated && !_userDetailsProvider.IsSuperAdmin)
                    await SendDeactivatedLoyaltyNotificationToSuperAdmin(venueRequest.Id, updatedVenue.Name);

                if (_userDetailsProvider.IsSuperAdmin)
                {
                    await _venueRepository.UpdateAssignedLoyalty(venue.Id, venue.Loyalty);
                    if (isLoyaltyDeactivated)
                    {
                        var userLoyaltyList = await _userLoyaltyRepository.GetUserLoyaltyByAssignedLoyalty(loyaltyId, request.VenueId);
                        await SendDeactivatedLoyaltyNotification(userLoyaltyList);
                    }
                    await _venueRequestRepository.DeleteVenueRequest(venue.Id, RequestType.UpdateLoyalty);
                }
            }
            return true;
        }

        public async Task<bool> UnAssignLoyaltyFromVenue(string venueId, string loyaltyId)
        {
            var venue = await _venueRepository.GetById(venueId);
            if (venue == null)
                throw new OutOutException(ErrorCodes.VenueNotFound);

            await _userDetailsProvider.ReInitialize();
            if (!_userDetailsProvider.HasAccessToVenue(venue.Id) && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisVenue, HttpStatusCode.Forbidden);

            var venueWithLoyalty = await _venueRepository.GetByLoyaltyId(loyaltyId);
            if (venueWithLoyalty == null)
                throw new OutOutException(ErrorCodes.LoyaltyNotFound);

            var updatedVenue = venue;
            updatedVenue.Loyalty = null;
            await _venueRequestRepository.UpsertVenueRequest(updatedVenue, venue, RequestType.UnassignLoyalty, loyaltyId);

            if (_userDetailsProvider.IsSuperAdmin)
            {
                await _venueRepository.UnAssignLoyalty(venue.Id);
                await _venueRequestRepository.DeleteVenueRequest(venue.Id, RequestType.UnassignLoyalty);

                await _userLoyaltyRepository.DeleteUserLoyaltyByAssignedLoyalty(loyaltyId, venueId);

                await _notificationComposerService.SendSignalRNotification(NotificationAction.VenueDetails,
                                                                      $"Loyalty has been removed by Super Admin for “{venue.Name}”",
                                                                      venue.Id,
                                                                      Roles.VenueAdmin,
                                                                      accessibleVenue: venue.Id);
            }
            if (!_userDetailsProvider.IsSuperAdmin)
            {
                var unAssignRequest = await _venueRequestRepository.GetVenueRequestByVenueId(venueId, RequestType.UnassignLoyalty);
                await SendUnAssignLoyaltyFromVenueNotification(unAssignRequest.Id, venue.Name);

            }
            return true;
        }

        public async Task<bool> UnAssignAllLoyalty()
        {
            bool result = false;

            await _userDetailsProvider.ReInitialize();
            if (_userDetailsProvider.IsSuperAdmin)
                result = await _venueRepository.UnAssignAllLoyalty() &
                         await _userLoyaltyRepository.DeleteAllUserLoyalty() &
                         await _venueRequestRepository.DeleteVenueRequestsByType(RequestType.UnassignAllLoyalty);

            else
            {
                foreach (var venueId in _userDetailsProvider.User.AccessibleVenues)
                {
                    var venue = await _venueRepository.GetById(venueId);
                    if (venue == null)
                        throw new OutOutException(ErrorCodes.VenueNotFound);
                    if (venue.Loyalty != null)
                    {
                        var updatedVenue = venue;
                        updatedVenue.Loyalty = null;
                        result = await _venueRequestRepository.UpsertVenueRequest(updatedVenue, venue, RequestType.UnassignAllLoyalty, venue?.Loyalty?.Id);
                    }
                }
            }
            return result;
        }

        public async Task<LoyaltyResponse> ApplyLoyalty(UserLoyaltyRequest request)
        {
            var user = await _userManager.FindByIdAsync(_userDetailsProvider.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var venue = await _venueRepository.GetByLoyaltyId(request.LoyaltyId);
            if (venue == null)
                throw new OutOutException(ErrorCodes.VenueNotFound);

            if (venue.Status != Availability.Active)
                throw new OutOutException(ErrorCodes.UnavailableVenue);

            if (!venue.OpenTimes.IsInRangeOf(_timeProvider.Now))
                throw new OutOutException(ErrorCodes.VenueIsNotOpenNow);

            if (venue.Loyalty == null)
                throw new OutOutException(ErrorCodes.LoyaltyNotFound);

            if (request.LoyaltyCode != venue.LoyaltyCode)
                throw new OutOutException(ErrorCodes.InvalidLoyaltyCode);

            var userLoyalty = await _userLoyaltyRepository.GetLatestUserLoyalty(user.Id, venue.Loyalty?.Id);

            if (userLoyalty == null || userLoyalty.IsConsumed)
                return await AddNewLoyalty(venue, user.Id, request.LoyaltyCode, userLoyalty);
            else if (userLoyalty != null && !userLoyalty.CanGet)
                return await AddNewStar(userLoyalty, venue, request.LoyaltyCode);
            else if (userLoyalty != null && userLoyalty.CanGet)
                return await ConsumeLoyalty(userLoyalty);

            else
                throw new OutOutException(ErrorCodes.CouldNotRedeemLoyalty);
        }

        private async Task<LoyaltyResponse> AddNewLoyalty(Venue venue, string userId, string code, UserLoyalty userLoyalty)
        {
            var redeemsToday = ExtractRedeemsToday(userLoyalty);

            if (!IsLoyaltyApplicable(venue.Loyalty, redeemsToday))
                throw new OutOutException(ErrorCodes.LoyaltyCannotBeRedeemRightNow_InApplicable);

            var newUserLoyalty = new UserLoyalty(userId, venue.Loyalty, code, UAEDateTime.Now);
            newUserLoyalty.Venue = _mapper.Map<VenueLoyaltySummary>(venue);

            newUserLoyalty.LastModifiedDate = DateTime.UtcNow;

            await _userLoyaltyRepository.Create(newUserLoyalty);
            return _mapper.Map<LoyaltyResponse>(newUserLoyalty);
        }

        private async Task<LoyaltyResponse> AddNewStar(UserLoyalty userLoyalty, Venue venue, string code)
        {
            var redeemsToday = ExtractRedeemsToday(userLoyalty);

            if (!IsLoyaltyApplicable(venue.Loyalty, redeemsToday, userLoyalty.CanGet))
                throw new OutOutException(ErrorCodes.LoyaltyCannotBeRedeemRightNow_InApplicable);

            userLoyalty.Redemptions.Add(new Redemption(code, UAEDateTime.Now));
            userLoyalty.CanGet = userLoyalty.Redemptions.Count() == (int)venue.Loyalty.Stars;

            userLoyalty.LastModifiedDate = DateTime.UtcNow;

            var updatedUserLoyalty = await _userLoyaltyRepository.Update(userLoyalty);
            return _mapper.Map<LoyaltyResponse>(updatedUserLoyalty);
        }

        private async Task<LoyaltyResponse> ConsumeLoyalty(UserLoyalty userLoyalty)
        {
            if (!IsLoyaltyApplicable(userLoyalty.Loyalty))
                throw new OutOutException(ErrorCodes.LoyaltyCannotBeRedeemRightNow_InApplicable);

            userLoyalty.IsConsumed = true;
            userLoyalty.CanGet = false;

            userLoyalty.LastModifiedDate = DateTime.UtcNow;

            var updatedUserLoyalty = await _userLoyaltyRepository.Update(userLoyalty);

            await _notificationComposerService.SendSignalRNotification(NotificationAction.CustomerProfile,
                                                                        $"Loyalty redeemed for “{userLoyalty.Venue.Name}”",
                                                                        userLoyalty.UserId,
                                                                        Roles.VenueAdmin,
                                                                        accessibleVenue: userLoyalty.Venue.Id);

            return _mapper.Map<LoyaltyResponse>(updatedUserLoyalty);
        }

        public async Task<VenueLoyaltySummaryResponse> GetVenueLoyaltyForUser(Venue venue)
        {
            if (venue.Loyalty == null || !venue.Loyalty.IsActive)
                return null;

            var userLoyalty = await _userLoyaltyRepository.GetLatestUserLoyalty(_userDetailsProvider.UserId, venue.Loyalty.Id);
            if (userLoyalty == null)
                return _mapper.Map<VenueLoyaltySummaryResponse>(venue.Loyalty);

            return _mapper.Map<VenueLoyaltySummaryResponse>(userLoyalty);
        }

        public static int ExtractRedeemsToday(UserLoyalty userLoyalty) =>
             userLoyalty?.Redemptions?.Count(a => a.Date.Day == UAEDateTime.Now.Day) ?? 0;

        public bool IsLoyaltyApplicable(Loyalty loyalty, int redeemsToday = 0, bool canGet = false)
        {
            var loyaltyAvailability = loyalty.ValidOn;

            if (loyaltyAvailability.IsInRangeOf(_timeProvider.Now) && loyalty.IsActive)
            {
                if (canGet)
                    return true;
                if (!IsExceededMaxUsage(loyalty, redeemsToday))
                    return true;
            }
            return false;
        }

        public bool IsExceededMaxUsage(Loyalty loyalty, int redeemsToday = 0)
        {
            if ((loyalty.MaxUsage == MaxUsage.OncePerDay && redeemsToday < 1) || loyalty.MaxUsage == MaxUsage.Unlimited)
                return false;
            return true;
        }

        public static int GetStarsCount(UserLoyalty userLoyalty)
        {
            if (userLoyalty == null || userLoyalty.IsConsumed)
                return 0;
            return userLoyalty.Redemptions.Count;
        }

        public async Task SendDeactivatedLoyaltyNotification(List<UserLoyalty> userLoyaltyList)
        {
            var notificationTasks = new List<Task>();
            var groupedUserLoyalty = userLoyaltyList.GroupBy(a => new { UserId = a.UserId, LoyaltyId = a.Loyalty.Id }).Select(a => a.FirstOrDefault()).ToList();

            foreach (var userLoyalty in groupedUserLoyalty)
            {
                var user = await _userRepository.GetUserById(userLoyalty.UserId);
                var notification = new Notification(NotificationType.Notification,
                                                    NotificationAction.DeactivateLoyalty,
                                                    user?.Id,
                                                    "Loyalty Deactivated",
                                                    $"“{userLoyalty.Loyalty.Type.Name}“ loyalty in {userLoyalty.Venue.Name} venue is currently inactive",
                                                    "venue.png"
                                                                );
                await _notificationRepository.Create(notification);
                notificationTasks.Add(_notificationComposerService.SendNotification(notification, user, NotificationAction.DeactivateLoyalty, userLoyalty.Loyalty.Id));
            }

            await Task.WhenAll(notificationTasks);
        }

        public async Task SendDeactivatedLoyaltyNotificationToSuperAdmin(string venueRequestId, string venueName)
        {
            await _notificationComposerService.SendSignalRNotification(NotificationAction.VenueRequestDetails,
                                                                         $"Loyalty has been deactivated by “{venueName}”",
                                                                         venueRequestId,
                                                                         Roles.SuperAdmin);
        }

        public async Task SendAssignLoyaltyToVenueNotification(string venueRequestId, string venueName)
        {
            await _notificationComposerService.SendSignalRNotification(NotificationAction.VenueRequestDetails,
                                                                         $"New Loyalty has been added to “{venueName}”",
                                                                         venueRequestId,
                                                                         Roles.SuperAdmin);
        }
        public async Task SendUnAssignLoyaltyFromVenueNotification(string venueRequestId, string venueName)
        {
            await _notificationComposerService.SendSignalRNotification(NotificationAction.UnAssignLoyalty,
                                                                         $"Un-Assign Loyalty request has been added to “{venueName}”",
                                                                         venueRequestId,
                                                                         Roles.SuperAdmin);
        }
    }
}
