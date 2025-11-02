using AutoMapper;
using Microsoft.AspNetCore.Identity;
using OutOut.Constants.Errors;
using OutOut.Models.Exceptions;
using OutOut.Models.Identity;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Providers;
using OutOut.ViewModels.Wrappers;
using OutOut.ViewModels.Responses.Offers;
using OutOut.ViewModels.Requests.Offers;
using OutOut.Models.Models;
using System.Net;
using OutOut.Core.Utils;
using OutOut.ViewModels.Responses.OfferTypes;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Requests.OfferTypes;
using OutOut.Constants.Enums;
using OutOut.Models;
using OutOut.Infrastructure.Services;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using OutOut.ViewModels.Responses.Venues;
using OutOut.Models.Models.Embedded;
using OutOut.Models.Utils;
using OutOut.Constants;
using OutOut.Persistence.Extensions;

namespace OutOut.Core.Services
{
    public class OfferService
    {
        private readonly IOfferRepository _offerRepository;
        private readonly NotificationService _notificationService;
        private readonly IOfferTypeRepository _offerTypeRepository;
        private readonly IVenueRepository _venueRepository;
        private readonly IVenueRequestRepository _venueRequestRepository;
        private readonly IUserOfferRepository _userOfferRepository;
        private readonly IMapper _mapper;
        private readonly IUserDetailsProvider _userDetailsProvider;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FileUploaderService _fileUploaderService;
        private readonly AppSettings _appSettings;
        private readonly IUserRepository _userRepository;
        private readonly NotificationComposerService _notificationComposerService;
        private readonly INotificationRepository _notificationRepository;

        public OfferService(IMapper mapper,
                            IUserDetailsProvider userDetailsProvider,
                            UserManager<ApplicationUser> userManager,
                            IOfferRepository offerRepository,
                            IUserOfferRepository userOfferRepository,
                            IVenueRepository venueRepository,
                            IOfferTypeRepository offerTypeRepository,
                            FileUploaderService fileUploaderService,
                            IOptions<AppSettings> appSettings,
                            IVenueRequestRepository venueRequestRepository,
                            IUserRepository userRepository,
                            NotificationComposerService notificationComposerService,
                            INotificationRepository notificationRepository, NotificationService notificationService)
        {
            _mapper = mapper;
            _userDetailsProvider = userDetailsProvider;
            _userManager = userManager;
            _offerRepository = offerRepository;
            _userOfferRepository = userOfferRepository;
            _venueRepository = venueRepository;
            _offerTypeRepository = offerTypeRepository;
            _fileUploaderService = fileUploaderService;
            _appSettings = appSettings.Value;
            _venueRequestRepository = venueRequestRepository;
            _userRepository = userRepository;
            _notificationComposerService = notificationComposerService;
            _notificationRepository = notificationRepository;
            _notificationService = notificationService;
        }

        public async Task<List<OfferTypeSummaryResponse>> GetOfferTypes()
        {
            var offerTypes = await _offerTypeRepository.GetAllOfferTypes();
            return _mapper.Map<List<OfferTypeSummaryResponse>>(offerTypes);
        }

        public async Task<Page<OfferTypeResponse>> GetOfferTypesPage(PaginationRequest paginationRequest, FilterationRequest filterationRequest)
        {
            var response = await _offerTypeRepository.GetOfferTypesPage(paginationRequest, filterationRequest);
            return _mapper.Map<Page<OfferTypeResponse>>(response);
        }

        public async Task<OfferTypeResponse> GetOfferType(string id)
        {
            OfferType offerType = await _offerTypeRepository.GetById(id);
            if (offerType == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            return _mapper.Map<OfferTypeResponse>(offerType);
        }

        public async Task<OfferTypeResponse> AddOfferType(OfferTypeRequest request)
        {
            var offerType = _mapper.Map<OfferType>(request);
            var response = await _offerTypeRepository.Create(offerType);
            return _mapper.Map<OfferTypeResponse>(response);
        }

        public async Task<OfferTypeResponse> UpdateOfferType(string id, OfferTypeRequest request)
        {
            var offerType = await _offerTypeRepository.GetById(id);
            if (offerType == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            offerType = _mapper.Map(request, offerType);
            offerType = await _offerTypeRepository.Update(offerType);
            return _mapper.Map<OfferTypeResponse>(offerType);
        }

        public async Task<bool> DeleteOfferType(string id)
        {
            var offerType = await _offerTypeRepository.GetById(id);
            if (offerType == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            await _venueRepository.DeleteOffer(id);
            await _userOfferRepository.DeleteUserOfferByType(id);

            return await _offerTypeRepository.Delete(id);
        }

        public async Task<Page<OfferWithVenueResponse>> GetAssignedOffersPage(PaginationRequest paginationRequest, FilterationRequest filterationRequest, bool getUpcoming = false)
        {
            await _userDetailsProvider.ReInitialize();
            var assignedOffersPage = await _offerRepository.GetAssignedOffersPage(paginationRequest, filterationRequest, _userDetailsProvider.User.AccessibleVenues, _userDetailsProvider.IsSuperAdmin);

            return _mapper.Map<Page<OfferWithVenueResponse>>(assignedOffersPage);
        }

        public async Task<Page<OfferWithUsageResponse>> GetAssignedUpcomingOffersPage(PaginationRequest paginationRequest, FilterationRequest filterationRequest)
        {
            await _userDetailsProvider.ReInitialize();
            var assignedUpcomingOffersPage = await _offerRepository.GetAssignedUpcomingOffersPage(paginationRequest, filterationRequest, _userDetailsProvider.User.AccessibleVenues, _userDetailsProvider.IsSuperAdmin);

            return _mapper.Map<Page<OfferWithUsageResponse>>(assignedUpcomingOffersPage);
        }

        public async Task<OfferWithUsageResponse> GetAssignedOffer(string offerId)
        {
            var assignedOffer = await _offerRepository.GetOfferById(offerId);
            if (assignedOffer == null)
                throw new OutOutException(ErrorCodes.OfferNotFound);

            await _userDetailsProvider.ReInitialize();
            if (!_userDetailsProvider.HasAccessToVenue(assignedOffer.Id) && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisVenue, HttpStatusCode.Forbidden);

            return _mapper.Map<OfferWithUsageResponse>(assignedOffer);
        }

        public async Task<bool> IsOfferExpiredOrInActive(string offerId)
        {
            if (offerId == null)
                throw new OutOutException(ErrorCodes.InvalidNullParameters);
            var assignedOffer = await _offerRepository.GetOfferById(offerId);
            if (assignedOffer == null)
                throw new OutOutException(ErrorCodes.OfferNotFound);

            return !assignedOffer.Offer.IsActive || assignedOffer.Offer.ExpiryDate.AddDays(1).AddSeconds(-1) < UAEDateTime.Now;
        }


        public async Task<OfferWithVenueResponse> AssignOfferToVenue(AssignedOfferRequest request)
        {
            var type = await _offerTypeRepository.GetById(request.TypeId);
            if (type == null)
                throw new OutOutException(ErrorCodes.OfferTypeNotFound);

            var venue = await _venueRepository.GetById(request.VenueId);
            if (venue == null)
                throw new OutOutException(ErrorCodes.VenueNotFound);

            await _userDetailsProvider.ReInitialize();
            if (!_userDetailsProvider.HasAccessToVenue(venue.Id) && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisVenue, HttpStatusCode.Forbidden);

            var newAssignedOffer = new Offer
            {
                Image = request.Image != null ? await _fileUploaderService.UploadFile(_appSettings.Directories.OffersImages, request.Image) : null,
                Type = type,
                IsActive = request.IsActive,
                ExpiryDate = request.ExpiryDate.Date,
                ValidOn = _mapper.Map<List<AvailableTime>>(request.ValidOn),
                MaxUsagePerYear = request.MaxUsagePerYear
            };

            if (_userDetailsProvider.IsSuperAdmin)
            {
                await _offerRepository.AssignOffer(venue.Id, newAssignedOffer);
                if (newAssignedOffer.IsActive)
                    await _notificationService.SendNewOfferNearYouNotifications(newAssignedOffer, venue);
            }
            else
            {
                var updatedVenue = venue;
                updatedVenue.Offers.Add(newAssignedOffer);
                var venueRequest = new VenueRequest(updatedVenue, venue, new LastModificationRequest(_userDetailsProvider.UserId, RequestType.AssignOffer, newAssignedOffer.Id));
                await _venueRequestRepository.Create(venueRequest);

                await SendAssignOfferToVenueNotification(venueRequest.Id, updatedVenue.Name);
            }

            var result = new OfferWithVenueResponse
            {
                Id = venue.Id,
                Venue = _mapper.Map<VenueSummaryResponse>(venue),
                Image = newAssignedOffer.Image,
                IsActive = newAssignedOffer.IsActive,
                Type = _mapper.Map<OfferTypeSummaryResponse>(newAssignedOffer.Type)
            };

            return _mapper.Map<OfferWithVenueResponse>(result);
        }

        public async Task<bool> UpdateAssignedOffer(string offerId, AssignedOfferRequest request)
        {
            var existingVenue = await _venueRepository.GetByOfferId(offerId);
            if (existingVenue == null)
                throw new OutOutException(ErrorCodes.OfferNotFound);

            var type = await _offerTypeRepository.GetById(request.TypeId);
            if (type == null)
                throw new OutOutException(ErrorCodes.OfferTypeNotFound);

            var venue = await _venueRepository.GetById(request.VenueId);
            if (venue == null)
                throw new OutOutException(ErrorCodes.VenueNotFound);

            await _userDetailsProvider.ReInitialize();
            if (!_userDetailsProvider.HasAccessToVenue(venue.Id) && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisVenue, HttpStatusCode.Forbidden);

            var updatedOffer = existingVenue.Offers.Where(a => a.Id == offerId).FirstOrDefault();

            bool isOfferDeactivated = updatedOffer.IsActive && !request.IsActive;

            updatedOffer.Type = type;
            updatedOffer.IsActive = request.IsActive;
            updatedOffer.ExpiryDate = request.ExpiryDate.Date;
            updatedOffer.ValidOn = _mapper.Map<List<AvailableTime>>(request.ValidOn);
            updatedOffer.MaxUsagePerYear = request.MaxUsagePerYear;

            if (request.Image != null)
                updatedOffer.Image = await _fileUploaderService.UploadFile(_appSettings.Directories.OffersImages, request.Image);

            var oldOffer = venue.Offers.Where(a => a.Id == offerId).FirstOrDefault();

            if (updatedOffer.ToBsonDocument().Equals(oldOffer.ToBsonDocument()))
                throw new OutOutException(ErrorCodes.NoChangesHaveBeenMade);

            if (existingVenue.Id != request.VenueId)
            {
                await UnAssignOfferFromVenue(existingVenue.Id, offerId);

                updatedOffer.Id = ObjectId.GenerateNewId().ToString();
                updatedOffer.AssignDate = DateTime.UtcNow;

                if (_userDetailsProvider.IsSuperAdmin)
                {
                    await _offerRepository.AssignOffer(venue.Id, updatedOffer);
                    if (updatedOffer.IsActive)
                        await _notificationService.SendNewOfferNearYouNotifications(updatedOffer, venue);
                    await _venueRequestRepository.DeleteVenueRequest(venue.Id, RequestType.AssignOffer);
                }
                else
                {
                    var updatedVenue = venue;
                    updatedVenue.Offers.Add(updatedOffer);
                    var venueRequest = new VenueRequest(updatedVenue, venue, new LastModificationRequest(_userDetailsProvider.UserId, RequestType.AssignOffer, offerId));
                    await _venueRequestRepository.Create(venueRequest);

                    await SendAssignOfferToVenueNotification(venueRequest.Id, updatedVenue.Name);
                }
            }
            else
            {
                if (_userDetailsProvider.IsSuperAdmin)
                {
                    await _offerRepository.UpdateAssignedOffer(existingVenue.Id, updatedOffer);
                    if (isOfferDeactivated)
                    {
                        var userOffers = await _userOfferRepository.GetUserOffersByAssignedOffer(offerId);
                        await SendDeactivatedOfferNotification(userOffers);
                    }
                }
                else
                {
                    var venueRequest = new VenueRequest(existingVenue, venue, new LastModificationRequest(_userDetailsProvider.UserId, RequestType.UpdateOffer, offerId));
                    await _venueRequestRepository.Create(venueRequest);

                    if (isOfferDeactivated)
                        await SendDeactivatedOfferNotificationToSuperAdmin(venueRequest.Id, existingVenue.Name);
                }
            }
            return true;
        }

        public async Task<bool> UnAssignOfferFromVenue(string venueId, string offerId)
        {
            var venue = await _venueRepository.GetById(venueId);
            if (venue == null)
                throw new OutOutException(ErrorCodes.VenueNotFound);

            await _userDetailsProvider.ReInitialize();
            if (!_userDetailsProvider.HasAccessToVenue(venue.Id) && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisVenue, HttpStatusCode.Forbidden);

            var venueWithAssignedOffer = await _offerRepository.GetOfferById(offerId);
            if (venueWithAssignedOffer == null)
                throw new OutOutException(ErrorCodes.OfferNotFound);

            if (_userDetailsProvider.IsSuperAdmin)
            {
                await _offerRepository.UnAssignOffer(venueId, offerId);
                await _userOfferRepository.DeleteUserOffersByAssignedOffer(offerId);
                await _notificationComposerService.SendSignalRNotification(NotificationAction.VenueDetails,
                                                                    $"Offer has been removed by Super Admin for “{venue.Name}”",
                                                                    venueId,
                                                                    Roles.VenueAdmin,
                                                                    accessibleVenue: venueId);
            }
            else
            {
                var updatedVenue = venue;
                updatedVenue.Offers.RemoveAll(a => a.Id == venueWithAssignedOffer.Offer.Id);
                var venueRequest = new VenueRequest(updatedVenue, venue, new LastModificationRequest(_userDetailsProvider.UserId, RequestType.UnassignOffer, offerId));
                await _venueRequestRepository.Create(venueRequest);
                await _notificationComposerService.SendSignalRNotification(NotificationAction.UnAssignOffer,
                                                             $"Un-Assign Offer request has been added to “{updatedVenue.Name}”",
                                                             venueRequest.Id,
                                                             Roles.SuperAdmin);
            }
            return true;
        }

        public async Task<bool> UnAssignAllOffers()
        {
            bool result = false;

            await _userDetailsProvider.ReInitialize();
            if (_userDetailsProvider.IsSuperAdmin)
                result = await _venueRepository.UnAssignAllOffers() &
                         await _userOfferRepository.DeleteAllUserOffers() &
                         await _venueRequestRepository.DeleteVenueRequestsByType(RequestType.UnassignAllOffers);

            else
            {
                foreach (var venueId in _userDetailsProvider.User.AccessibleVenues)
                {
                    var venue = await _venueRepository.GetById(venueId);
                    if (venue == null)
                        throw new OutOutException(ErrorCodes.VenueNotFound);
                    if (venue.Offers.Any() && venue.Offers != null)
                    {
                        var updatedVenue = venue;
                        updatedVenue.Offers = new List<Offer>();
                        result = await _venueRequestRepository.UpsertVenueRequest(updatedVenue, venue, RequestType.UnassignAllOffers, null);
                    }
                }
            }
            return result;
        }

        public async Task<Page<OfferWithVenueResponse>> GetActiveNonExpiredOffers(PaginationRequest paginationRequest, OfferFilterationRequest filterationRequest)
        {
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var venuesPage = await _offerRepository.GetActiveNonExpiredOffers(paginationRequest, user.Location, filterationRequest, _userDetailsProvider.UserId);

            return _mapper.Map<Page<OfferWithVenueResponse>>(venuesPage);
        }

        public async Task<Page<OfferWithVenueResponse>> GetActiveNonExpiredUpcomingOffers(PaginationRequest paginationRequest, OfferFilterationRequest filterationRequest)
        {
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var venuesPage = await _offerRepository.GetActiveNonExpiredOffers(paginationRequest, user.Location, filterationRequest, _userDetailsProvider.UserId, true);

            return _mapper.Map<Page<OfferWithVenueResponse>>(venuesPage);
        }



        public async Task<List<OfferWithUsageResponse>> GetNewestOffers()
        {
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            await _userDetailsProvider.ReInitialize();

            var offers = await _offerRepository.GetNewestOffers(user.AccessibleVenues, _userDetailsProvider.IsSuperAdmin);
            return _mapper.Map<List<OfferWithUsageResponse>>(offers.Take(10));
        }

        public async Task<List<OfferResponse>> GetOffersInVenue(Venue venue, bool isUpComing = false)
        {
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var listOfOffers = new List<OfferResponse>();

            foreach (var offer in venue.Offers)
            {
                if (HasExpired(offer.ExpiryDate.Date))
                    continue;
                if (!offer.IsActive)
                    continue;
                if (isUpComing)
                {
                    if (offer.ValidOn.Any(l => l.Days.Contains(UAEDateTime.Now.DayOfWeek) && (UAEDateTime.Now.TimeOfDay > l.From && UAEDateTime.Now.TimeOfDay < l.To)))
                        continue;
                }
                else
                {
                    if (!offer.ValidOn.Any(l => l.Days.Contains(UAEDateTime.Now.DayOfWeek) && (UAEDateTime.Now.TimeOfDay > l.From && UAEDateTime.Now.TimeOfDay < l.To)))
                        continue;
                }


                var userOffer = await _userOfferRepository.GetUserRedeems(user.Id, offer.Id, UAEDateTime.Now.Date);
                OfferResponse offerResponse;

                if (userOffer == null)
                    offerResponse = _mapper.Map<OfferResponse>(offer);
                else
                    offerResponse = _mapper.Map<OfferResponse>(userOffer);

                listOfOffers.Add(offerResponse);
            }

            return listOfOffers.OrderBy(o => o.IsApplicable ? 0 : 1).ToList();
        }

        public async Task<bool> RedeemOffer(string offerId, string pinCode)
        {
            var user = await _userManager.FindByIdAsync(_userDetailsProvider.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var venue = await _venueRepository.GetVenueByOfferId(offerId);
            if (venue == null)
                throw new OutOutException(ErrorCodes.VenueNotFound);

            if (venue.Status != Availability.Active)
                throw new OutOutException(ErrorCodes.UnavailableVenue);

            if (!venue.OpenTimes.IsInRangeOf(UAEDateTime.Now))
                throw new OutOutException(ErrorCodes.VenueIsNotOpenNow);

            var offerToBeRedeemed = venue?.Offer;
            if (venue == null || offerToBeRedeemed == null)
                throw new OutOutException(ErrorCodes.OfferNotFound);

            if (venue.OffersCode != pinCode)
                throw new OutOutException(ErrorCodes.InvalidPINCodeForOffer);

            if (!offerToBeRedeemed.ValidOn.IsInRangeOf(UAEDateTime.Now))
                throw new OutOutException(ErrorCodes.OfferCannotBeRedeemedRightNow);
            if (HasExpired(offerToBeRedeemed.ExpiryDate.Date))
                throw new OutOutException(ErrorCodes.OfferHasExpired);
            if (!offerToBeRedeemed.IsActive)
                throw new OutOutException(ErrorCodes.OfferIsNotActive);

            var offersRedeemedThisYear = await _userOfferRepository.GetUserRedeemsThisYear(user.Id, offerId);
            var redeemsCountThisYear = offersRedeemedThisYear.SelectMany(a => a.Log).Count();

            if (redeemsCountThisYear == (int)offerToBeRedeemed.MaxUsagePerYear)
                throw new OutOutException(ErrorCodes.ExceededMaxUsagesPerYear);

            var redeemsToday = await _userOfferRepository.GetUserRedeems(user.Id, offerId, UAEDateTime.Now.Date);
            if (redeemsToday != null)
            {
                if (IsExceededMaxUsage(offerToBeRedeemed, redeemsToday.Log.Count, redeemsCountThisYear))
                    throw new OutOutException(ErrorCodes.ExceededMaxUsagesPerDay);

                redeemsToday.Log.Add(new UserOfferRedeemLog(pinCode));

                redeemsToday.HasReachedLimit = IsExceededMaxUsage(offerToBeRedeemed, redeemsToday.Log.Count, redeemsCountThisYear + 1);

                await _userOfferRepository.Update(redeemsToday);
            }
            else
            {
                await _userOfferRepository.Create(new UserOffer
                {
                    Offer = offerToBeRedeemed,
                    UserId = user.Id,
                    Day = UAEDateTime.Now.Date,
                    HasReachedLimit = IsExceededMaxUsage(offerToBeRedeemed, 1, 1),
                    Log = new List<UserOfferRedeemLog> { new UserOfferRedeemLog(pinCode) },
                    Venue = _mapper.Map<VenueSummary>(venue),
                });
            }

            await _notificationComposerService.SendSignalRNotification(NotificationAction.CustomerProfile,
                                                                       $"Offer redeemed for “{venue.Name}” by “{user.FullName}”",
                                                                       user.Id,
                                                                       Roles.VenueAdmin,
                                                                       accessibleVenue: venue.Id);
            return true;
        }

        private bool HasExpired(DateTime expiryDate) => expiryDate < UAEDateTime.Now.Date;

        public bool IsExceededMaxUsage(Offer offer, int redeemsToday = 0, int redeemsThisYear = 0) =>
            redeemsThisYear < (int)offer.MaxUsagePerYear && redeemsToday < 1 ? false : true;

        public async Task SendDeactivatedOfferNotification(List<UserOffer> userOffersList)
        {
            var notificationTasks = new List<Task>();
            var groupedUserOffers = userOffersList.GroupBy(a => new { UserId = a.UserId, OfferId = a.Offer.Id }).Select(a => a.FirstOrDefault()).ToList();

            foreach (var userOffer in groupedUserOffers)
            {
                var user = await _userRepository.GetUserById(userOffer.UserId);
                var notification = new Notification(NotificationType.Notification,
                                                user?.Id,
                                                "Offer Deactivated",
                                                $"“{userOffer.Offer.Type.Name}“ offer in {userOffer.Venue.Name} venue is currently inactive",
                                                "venue.png", NotificationAction.DeactivateOffer);
                await _notificationRepository.Create(notification);
                notificationTasks.Add(_notificationComposerService.SendNotification(notification, user, NotificationAction.DeactivateOffer, userOffer.Offer.Id));
            }

            await Task.WhenAll(notificationTasks);
        }

        public async Task SendDeactivatedOfferNotificationToSuperAdmin(string venueRequestId, string venueName)
        {
            await _notificationComposerService.SendSignalRNotification(NotificationAction.VenueRequestDetails,
                                                                          $"Offer has been deactivated by “{venueName}”",
                                                                          venueRequestId,
                                                                          Roles.SuperAdmin);
        }

        public async Task SendAssignOfferToVenueNotification(string venueRequestId, string venueName)
        {
            await _notificationComposerService.SendSignalRNotification(NotificationAction.VenueRequestDetails,
                                                                         $"New Offer has been added to “{venueName}”",
                                                                         venueRequestId,
                                                                         Roles.SuperAdmin);
        }

        public async Task<Page<OfferWithUsageResponse>> GetMyOffers(PaginationRequest paginationRequest, FilterationRequest filterationRequest)
        {
            var myOffersPage = await _userOfferRepository.GetUserOffersByUserId(_userDetailsProvider.UserId, PaginationRequest.Max, filterationRequest);
            var groupedResultByVenue = myOffersPage.Records.OrderByDescending(a => a.Day.Add(a.Log[0].Time)).ThenBy(a => a.Venue.Name).GroupBy(a => new { VenueId = a.Venue.Id, Offer = a.Offer.Type.Name }).Select(a => a.FirstOrDefault()).ToList();
            return _mapper.Map<Page<OfferWithUsageResponse>>(groupedResultByVenue.GetPaged(PaginationRequest.Max));
        }
    }
}
