using AutoMapper;
using Microsoft.AspNetCore.Identity;
using OutOut.Constants.Enums;
using OutOut.Constants.Errors;
using OutOut.Models.Exceptions;
using OutOut.Models.Identity;
using OutOut.Models.Models;
using OutOut.Models.Models.Embedded;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Providers;
using OutOut.ViewModels.Requests.Venues;
using OutOut.ViewModels.Responses.TermsAndConditions;
using OutOut.ViewModels.Responses.Venues;
using OutOut.ViewModels.Wrappers;
using OutOut.Infrastructure.Services;
using OutOut.Models;
using Microsoft.Extensions.Options;
using OutOut.ViewModels.Responses.Events;
using OutOut.Core.Utils;
using System.Net;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Responses.Offers;
using OutOut.Persistence.Extensions;
using OutOut.Models.Domains;
using OutOut.Constants;
using MongoDB.Bson;
using OutOut.ViewModels.Responses.VenueBooking;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using OutOut.ViewModels.Responses.Excel;
using OutOut.ViewModels.Requests.VenueBooking;
using OutOut.Models.Utils;

namespace OutOut.Core.Services
{
    public class VenueService
    {
        private readonly IVenueRepository _venueRepository;
        private readonly IVenueRequestRepository _venueRequestRepository;
        private readonly IMapper _mapper;
        private readonly IUserDetailsProvider _userDetailsProvider;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITermsAndConditionsRepository _termsAndConditionsRepo;
        private readonly IVenueBookingRepository _venueBookingRepository;
        private readonly ICategoryRepository _categoryRepo;
        private readonly FileUploaderService _fileUploaderService;
        private readonly AppSettings _appSettings;
        private readonly LocationService _locationService;
        private readonly OfferService _offerService;
        private readonly LoyaltyService _loyaltyService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IUserLoyaltyRepository _userLoyaltyRepository;
        private readonly IUserOfferRepository _userOfferRepository;
        private readonly IOfferRepository _offerRepository;
        private readonly ICityRepository _cityRepository;
        private readonly IUserRepository _userRepository;
        private readonly NotificationComposerService _notificationComposerService;
        private readonly NotificationService _notificationService;
        private readonly ILogger<VenueService> _logger;
        private readonly EventService _eventService;

        public VenueService(IVenueRepository venueRepository,
                            IMapper mapper,
                            IUserDetailsProvider userDetailsProvider,
                            UserManager<ApplicationUser> userManager,
                            ITermsAndConditionsRepository termsAndConditionsRepo,
                            FileUploaderService fileUploaderService,
                            IOptions<AppSettings> appSettings,
                            ICategoryRepository categoryRepo,
                            IVenueBookingRepository venueBookingRepository,
                            LoyaltyService loyaltyService,
                            OfferService offerService,
                            INotificationRepository notificationRepo,
                            IEventRepository eventRepository,
                            NotificationComposerService notificationComposerService,
                            ICityRepository cityRepository,
                            IUserLoyaltyRepository userLoyaltyRepository,
                            IUserOfferRepository userOfferRepository,
                            IVenueRequestRepository venueRequestRepository,
                            IOfferRepository offerRepository,
                            LocationService locationService,
                            ILogger<VenueService> logger,
                            IUserRepository userRepository,
                            NotificationService notificationService,
                            EventService eventService)
        {
            _venueRepository = venueRepository;
            _mapper = mapper;
            _userDetailsProvider = userDetailsProvider;
            _userManager = userManager;
            _termsAndConditionsRepo = termsAndConditionsRepo;
            _fileUploaderService = fileUploaderService;
            _appSettings = appSettings.Value;
            _categoryRepo = categoryRepo;
            _venueBookingRepository = venueBookingRepository;
            _offerService = offerService;
            _notificationRepository = notificationRepo;
            _eventRepository = eventRepository;
            _notificationComposerService = notificationComposerService;
            _loyaltyService = loyaltyService;
            _cityRepository = cityRepository;
            _userLoyaltyRepository = userLoyaltyRepository;
            _userOfferRepository = userOfferRepository;
            _venueRequestRepository = venueRequestRepository;
            _offerRepository = offerRepository;
            _locationService = locationService;
            _logger = logger;
            _userRepository = userRepository;
            _notificationService = notificationService;
            _eventService = eventService;
        }

        public async Task<FullVenueResponse> CreateVenue(CreateVenueRequest request)
        {
            var venue = _mapper.Map<Venue>(request);

            venue.OpenTimes = _mapper.Map<List<AvailableTime>>(request.AvailableTimes);
            if (!venue.OpenTimes.Any() || venue.OpenTimes == null)
                throw new OutOutException(ErrorCodes.OpenTimesIsRequired);

            await _locationService.IsLocationInAllowedCountriesAsync(request.Location);

            var city = await _cityRepository.GetByArea(request.Location.CityId, request.Location.Area);
            if (city == null)
                throw new OutOutException(ErrorCodes.CityNotFound);
            var citySummary = _mapper.Map<CitySummary>(city);
            venue.Location = new Location(request.Location.Longitude, request.Location.Latitude, citySummary, request.Location.Area, request.Location.Description);

            if (request.Logo != null)
                venue.Logo = await _fileUploaderService.UploadFile(_appSettings.Directories.VenuesLogos, request.Logo);

            if (request.TableLogo != null)
                venue.TableLogo = await _fileUploaderService.UploadFile(_appSettings.Directories.VenuesLogos, request.TableLogo);

            if (request.DetailsLogo != null)
                venue.DetailsLogo = await _fileUploaderService.UploadFile(_appSettings.Directories.VenuesLogos, request.DetailsLogo);

            if (request.Background != null)
                venue.Background = await _fileUploaderService.UploadFile(_appSettings.Directories.VenuesGallery, request.Background);

            if (request.Menu != null)
                venue.Menu = await _fileUploaderService.UploadFile(_appSettings.Directories.VenuesMenus, request.Menu);

            if (request.Gallery == null) venue.Gallery = new List<string>();
            else
            {
                venue.Gallery = new List<string>();
                foreach (var gallery in request.Gallery)
                    venue.Gallery.Add(await _fileUploaderService.UploadFile(_appSettings.Directories.VenuesGallery, gallery));
            }
            if (request.SelectedTermsAndConditions != null && request.SelectedTermsAndConditions.Any())
            {
                foreach (var tc in request.SelectedTermsAndConditions)
                {
                    var existingTermCondition = await _termsAndConditionsRepo.GetById(tc);
                    if (existingTermCondition == null)
                        throw new OutOutException(ErrorCodes.TermAndConditionNotFound);
                }
            }

            foreach (var categoryId in request.CategoriesIds)
            {
                var existingCategory = await _categoryRepo.GetById(categoryId);
                if (existingCategory == null || existingCategory.TypeFor != TypeFor.Venue)
                    throw new OutOutException(ErrorCodes.CategoryNotFound);
                venue.Categories.Add(existingCategory);
            }

            if (request.EventsIds != null && request.EventsIds.Any())
            {
                foreach (var eventId in request.EventsIds)
                {
                    var existingEvent = await _eventRepository.GetById(eventId);
                    if (existingEvent == null)
                        throw new OutOutException(ErrorCodes.EventNotFound);
                    venue.Events.Add(eventId);
                }
            }
            venue.Events = request.EventsIds == null || !request.EventsIds.Any() ? new List<string>() : request.EventsIds;

            venue.Name = request.Name.Trim();
            venue.CreatedBy = _userDetailsProvider.UserId;
            venue.Id = ObjectId.GenerateNewId().ToString();

            if (!request.IsActive) venue.Status = Availability.Inactive;

            await _venueRequestRepository.UpsertVenueRequest(venue, null, RequestType.AddVenue, venue.Id);

            var venueRequest = await _venueRequestRepository.GetVenueRequestByVenueId(venue.Id, RequestType.AddVenue);

            await _userDetailsProvider.ReInitialize();
            if (!_userDetailsProvider.IsSuperAdmin)
                await _notificationComposerService.SendSignalRNotification(NotificationAction.VenueRequestDetails,
                                                                           $"New Venue Added, by “{_userDetailsProvider.User.FullName}”",
                                                                           venueRequest.Id,
                                                                           Roles.SuperAdmin);

            if (_userDetailsProvider.IsSuperAdmin)
            {
                var isApproved = await ApproveVenue(venueRequest?.Id);
                if (!isApproved)
                    throw new OutOutException(ErrorCodes.CouldNotCreateVenue);
            }

            return _mapper.Map<FullVenueResponse>(venue);
        }

        public async Task<FullVenueResponse> UpdateVenue(string id, UpdateVenueRequest request)
        {
            var existingVenue = await CheckAccessibleVenue(id);

            await _locationService.IsLocationInAllowedCountriesAsync(request.Location);

            var city = await _cityRepository.GetByArea(request.Location.CityId, request.Location.Area);
            if (city == null)
                throw new OutOutException(ErrorCodes.CityNotFound);
            var citySummary = _mapper.Map<CitySummary>(city);
            existingVenue.Location = new Location(request.Location.Longitude, request.Location.Latitude, citySummary, request.Location.Area, request.Location.Description);
            if (request.SelectedTermsAndConditions != null && request.SelectedTermsAndConditions.Any())
            {
                foreach (var tc in request.SelectedTermsAndConditions)
                {
                    var existingTermCondition = await _termsAndConditionsRepo.GetById(tc);
                    if (existingTermCondition == null)
                        throw new OutOutException(ErrorCodes.TermAndConditionNotFound);
                }
            }

            var newCategories = new List<Category>();
            foreach (var categoryId in request.CategoriesIds)
            {
                var existingCategory = await _categoryRepo.GetById(categoryId);
                if (existingCategory == null || existingCategory.TypeFor != TypeFor.Venue)
                    throw new OutOutException(ErrorCodes.CategoryNotFound);
                newCategories.Add(existingCategory);
            }
            existingVenue.Categories = newCategories;

            if (request.EventsIds != null && request.EventsIds.Any())
            {
                foreach (var eventId in request.EventsIds)
                {
                    var existingEvent = await _eventRepository.GetById(eventId);
                    if (existingEvent == null)
                        throw new OutOutException(ErrorCodes.EventNotFound);
                }
            }
            var oldEventList = existingVenue.Events;

            var unassignedEventIds = request.EventsIds != null ? oldEventList.Where(l => !request.EventsIds.Contains(l)).ToList() : oldEventList;
            foreach (var unassignedEvent in unassignedEventIds)
            {
                var existingEvent = await _eventRepository.GetById(unassignedEvent);
                existingEvent.Venue = null;
                await _eventRepository.Update(existingEvent);
            }
            existingVenue.Events = request.EventsIds == null || !request.EventsIds.Any() ? new List<string>() : request.EventsIds;

            if (request.Logo != null)
                existingVenue.Logo = await _fileUploaderService.UploadFile(_appSettings.Directories.VenuesLogos, request.Logo);

            if (request.TableLogo != null)
                existingVenue.TableLogo = await _fileUploaderService.UploadFile(_appSettings.Directories.VenuesLogos, request.TableLogo);

            if (request.DetailsLogo != null)
                existingVenue.DetailsLogo = await _fileUploaderService.UploadFile(_appSettings.Directories.VenuesLogos, request.DetailsLogo);

            if (request.Background != null)
                existingVenue.Background = await _fileUploaderService.UploadFile(_appSettings.Directories.VenuesGallery, request.Background);

            if (request.Menu != null)
                existingVenue.Menu = await _fileUploaderService.UploadFile(_appSettings.Directories.VenuesMenus, request.Menu);

            if (request.Gallery != null && request.Gallery.Any())
            {
                foreach (var img in request.Gallery)
                    existingVenue.Gallery.Add(await _fileUploaderService.UploadFile(_appSettings.Directories.VenuesGallery, img));
            }

            existingVenue.OpenTimes = _mapper.Map<List<AvailableTime>>(request.AvailableTimes);
            if (!existingVenue.OpenTimes.Any() || existingVenue.OpenTimes == null)
                throw new OutOutException(ErrorCodes.OpenTimesIsRequired);

            if (!request.IsActive && existingVenue.Status == Availability.Active)
                existingVenue.Status = Availability.Inactive;
            else if (request.IsActive)
                existingVenue.Status = Availability.Active;

            if (request.IsActive && (!city.IsActive || city.Areas.Where(a => a == request.Location.Area).FirstOrDefault() == null))
                throw new OutOutException(ErrorCodes.CantActivateVenueInAnInactiveCityOrArea);


            var oldVenue = _venueRepository.GetVenueById(id);

            var venue = _mapper.Map(request, existingVenue);

            venue.Name = request.Name.Trim();

            if (oldVenue.ToBsonDocument().Equals(venue.ToBsonDocument()))
                throw new OutOutException(ErrorCodes.NoChangesHaveBeenMade);

            await _venueRequestRepository.UpsertVenueRequest(venue, oldVenue, RequestType.UpdateVenue, id);
            var venueRequest = await _venueRequestRepository.GetVenueRequestByVenueId(venue.Id, RequestType.UpdateVenue);

            await _userDetailsProvider.ReInitialize();
            if (!_userDetailsProvider.IsSuperAdmin)
                await _notificationComposerService.SendSignalRNotification(NotificationAction.VenueRequestDetails,
                                                                          $"“{venueRequest.OldVenue.Name}” has been edited, Waiting for approval",
                                                                          venueRequest.Id,
                                                                          Roles.SuperAdmin);

            if (_userDetailsProvider.IsSuperAdmin)
            {
                var isApproved = await ApproveVenue(venueRequest?.Id);
                if (!isApproved)
                    throw new OutOutException(ErrorCodes.CouldNotUpdateVenue);
                await _notificationComposerService.SendSignalRNotification(NotificationAction.VenueDetails,
                                                                           $"“{oldVenue.Name}” has been updated by Super Admin",
                                                                           oldVenue.Id,
                                                                           Roles.VenueAdmin,
                                                                           accessibleVenue: oldVenue.Id);
            }
            return _mapper.Map<FullVenueResponse>(venue);
        }

        public async Task<bool> DeleteGalleryImages(string venueId, List<string> images)
        {
            var venue = await _venueRepository.GetById(venueId);
            if (venue == null)
                throw new OutOutException(ErrorCodes.VenueNotFound);

            bool result = false;
            if (images.Any() && images != null)
            {
                if (_userDetailsProvider.IsSuperAdmin)
                {
                    result = await _venueRepository.DeleteGalleryImages(venueId, images);
                    await _venueRequestRepository.DeleteVenueRequest(venueId, RequestType.UpdateVenue);
                }
                else
                {
                    var updatedVenue = venue;
                    updatedVenue.Gallery.RemoveAll(existingImage => images.Contains(existingImage));
                    var existingRequest = await _venueRequestRepository.GetVenueRequest(venueId, RequestType.UpdateVenue, _userDetailsProvider.UserId);
                    if (existingRequest == null)
                        result = await _venueRequestRepository.UpsertVenueRequest(updatedVenue, venue, RequestType.UpdateVenue, venue.Id);
                    else
                        result = await _venueRequestRepository.DeleteGalleryImages(existingRequest.Id, images);
                }
            }
            return result;
        }

        public async Task<bool> DeleteVenue(string id)
        {
            var venue = await CheckAccessibleVenue(id);

            var isAcknowledged = await _venueRequestRepository.UpsertVenueRequest(venue, venue, RequestType.DeleteVenue, id);

            if (_userDetailsProvider.IsSuperAdmin)
            {
                var venueRequest = await _venueRequestRepository.GetVenueRequestByVenueId(id, RequestType.DeleteVenue);
                await _notificationComposerService.SendSignalRNotification(NotificationAction.DeleteVenue,
                                                                         $"“{venue.Name}” has been deleted by Super Admin",
                                                                         id,
                                                                         Roles.VenueAdmin,
                                                                         accessibleVenue: id);
                await ApproveVenue(venueRequest?.Id);
            }
            return isAcknowledged;
        }

        public async Task HandleDeactivateVenue(Venue venue, Availability status = Availability.VenueInactive, bool updateAssociatedVenue = true)
        {
            if (!updateAssociatedVenue && status == Availability.Inactive)
                await _venueRepository.UpdateVenueStatus(venue.Id, Availability.Inactive);
            else if (updateAssociatedVenue)
                await _venueRepository.UpdateVenueStatus(venue.Id, status);

            if (venue.Loyalty != null)
            {
                await _venueRepository.UpdateAssignedLoyaltyStatus(venue.Id, false);

                var userLoyaltyList = await _userLoyaltyRepository.GetUserLoyaltyByAssignedLoyalty(venue.Loyalty.Id, venue.Id);
                await _loyaltyService.SendDeactivatedLoyaltyNotification(userLoyaltyList);
            }
            if (venue.Offers.Any() && venue.Offers != null)
            {
                await _venueRepository.UpdateAssignedOffersStatus(venue.Id, false);

                var useroffersList = await _userOfferRepository.GetUserOffersByVenueId(venue.Id);
                await _offerService.SendDeactivatedOfferNotification(useroffersList);
            }
            await HandleVenueBookingsOfInactiveVenues(new List<string> { venue.Id });
            await _eventRepository.UpdateEventsStatus(venue.Events, Availability.VenueInactive);
            await _eventService.HandleDeactivateDeleteEvent(venue.Events);
        }

        public async Task HandleReactivateVenue(Venue venue)
        {
            await _venueRepository.UpdateVenueStatus(venue.Id, Availability.Active);

            if (venue.Loyalty != null)
                await _venueRepository.UpdateAssignedLoyaltyStatus(venue.Id, true);
            if (venue.Offers.Any() && venue.Offers != null)
                await _venueRepository.UpdateAssignedOffersStatus(venue.Id, true);
        }

        public async Task HandleAddVenue(Venue venue)
        {
            if (venue.Status != Availability.Active)
            {
                if (venue.Events.Any() && venue.Events != null)
                    await _eventRepository.UpdateEventsStatus(venue.Events, Availability.VenueInactive);
            }

            if (venue.Events.Any())
                venue.Events.ForEach(async (eventId) =>
                {
                    await _eventRepository.UpdateAssignedVenue(eventId, _mapper.Map<VenueSummary>(venue));
                    await _venueRepository.RemoveEventFromOldAssignedVenues(venue.Id, eventId);
                });

            if (venue.Status == Availability.Active)
                await _notificationService.SendNewVenueNearYouNotifications(venue);
        }

        public async Task HandleDeleteVenue(Venue venue)
        {
            await HandleVenueBookingsOfInactiveVenues(new List<string> { venue.Id });
            await _venueBookingRepository.DeleteBookingsForDeletedVenue(venue.Id);

            await _eventRepository.UpdateEventsStatus(venue.Events, Availability.VenueDeleted);
            venue.Events.ForEach(async (eventId) => await _eventRepository.UpdateAssignedVenue(eventId, null));
            await _eventService.HandleDeactivateDeleteEvent(venue.Events);

            await _userLoyaltyRepository.DeleteUserLoyaltyByVenue(venue.Id);
            venue.Offers.ForEach(async (offer) => await _userOfferRepository.DeleteUserOffersByAssignedOffer(offer.Id));

            await _userRepository.DeleteVenueIdFromAccessibleVenues(venue.Id);
        }

        public async Task HandleVenueBookingsOfInactiveVenues(List<string> associatedVenuesIds)
        {
            try
            {
                var rejectedBookings = await _venueBookingRepository.RejectBookingsForDeactivatedVenues(associatedVenuesIds);
                await _notificationRepository.DeleteRemindersWithDeactivatedVenues(associatedVenuesIds);

                var notificationTasks = new List<Task>();
                var groupedRejectedBookings = rejectedBookings.GroupBy(a => new { UserId = a.User.Id, VenueId = a.Venue.Id }).Select(a => a.FirstOrDefault()).ToList();
                foreach (var booking in groupedRejectedBookings)
                {
                    var bookingOwner = await _userRepository.GetUserById(booking.User.Id);
                    var notification = new Notification(NotificationType.Notification,
                                                        NotificationAction.VenueBookingRejection,
                                                    bookingOwner?.Id,
                                                    "Booking Rejection",
                                                    $"Unfortunately “{booking.Venue.Name}“ booking is rejected, you can try booking again at another time",
                                                    "venue.png");
                    await _notificationRepository.Create(notification);
                    notificationTasks.Add(_notificationComposerService.SendNotification(notification, bookingOwner, NotificationAction.VenueBookingRejection, booking.Venue.Id));
                }
                await Task.WhenAll(notificationTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send rejection notification of deactivated venues");
            }
        }

        public async Task<bool> ApproveVenue(string requestId)
        {
            var venueRequest = await _venueRequestRepository.GetVenueRequestById(requestId);
            if (venueRequest == null)
                throw new OutOutException(ErrorCodes.VenueRequestNotFound);

            bool result = false;
            var venueCreator = await _userRepository.GetUserById(venueRequest.LastModificationRequest.CreatedBy);
            if (venueRequest.LastModificationRequest.Type == RequestType.AddVenue)
            {
                result = await _venueRequestRepository.ApproveVenue(requestId, venueRequest.Venue);
                if (result)
                    await HandleAddVenue(venueRequest.Venue);

                if (await _userManager.IsInRoleAsync(venueCreator, Roles.VenueAdmin))
                    await _userRepository.AddVenueIdToAccessibleVenues(venueCreator.Id, venueRequest.Venue.Id);
            }

            var existingVenue = await _venueRepository.GetById(venueRequest.Venue.Id);
            if (existingVenue == null)
                throw new OutOutException(ErrorCodes.VenueNotFound);

            if (venueRequest.LastModificationRequest.Type == RequestType.UpdateVenue)
            {
                result = await _venueRequestRepository.ApproveVenue(requestId, venueRequest.Venue);
                if (result)
                {
                    if (venueRequest.Venue.Events.Any())
                        venueRequest.Venue.Events.ForEach(async (eventId) =>
                        {
                            await _eventRepository.UpdateAssignedVenue(eventId, _mapper.Map<VenueSummary>(venueRequest.Venue));
                            await _venueRepository.RemoveEventFromOldAssignedVenues(venueRequest.Venue.Id, eventId);
                        });
                }

                if (venueRequest.Venue.Status != Availability.Active && result)
                    await HandleDeactivateVenue(venueRequest.Venue, Availability.VenueInactive, false);

                if (existingVenue?.Status != Availability.Active && venueRequest.Venue.Status == Availability.Active && result)
                    await HandleReactivateVenue(existingVenue);
            }

            else if (venueRequest.LastModificationRequest.Type == RequestType.DeleteVenue)
            {
                var isDeleted = await _venueRepository.Delete(venueRequest.Venue.Id);
                if (isDeleted)
                {
                    await HandleDeleteVenue(venueRequest.Venue);
                    result = await _venueRequestRepository.DeleteVenueRequest(venueRequest.Id);

                    if (!await _userManager.IsInRoleAsync(venueCreator, Roles.SuperAdmin))
                        await _notificationComposerService.SendSignalRNotificationToUser(NotificationAction.DeleteVenue,
                                                          $"Delete request for “{venueRequest.Venue.Name}” has been approved by Super Admin",
                                                          venueRequest.Venue.Id,
                                                          venueRequest.LastModificationRequest.CreatedBy);
                }
                return result;
            }

            else if (venueRequest.LastModificationRequest.Type == RequestType.AssignLoyalty)
            {
                var assignedLoyalty = await _venueRepository.AssignLoyalty(venueRequest.Venue.Id, venueRequest.Venue.Loyalty);
                if (assignedLoyalty != null)
                    await _venueRequestRepository.DeleteVenueRequest(venueRequest.Id);
                result = assignedLoyalty != null;
            }

            else if (venueRequest.LastModificationRequest.Type == RequestType.UnassignLoyalty || venueRequest.LastModificationRequest.Type == RequestType.UnassignAllLoyalty)
            {
                result = await _venueRepository.UnAssignLoyalty(venueRequest.Venue.Id);
                if (result)
                {
                    await _venueRequestRepository.DeleteVenueRequest(venueRequest.Id);
                    await _userLoyaltyRepository.DeleteUserLoyaltyByAssignedLoyalty(existingVenue?.Loyalty?.Id, existingVenue.Id);
                }
            }

            else if (venueRequest.LastModificationRequest.Type == RequestType.UpdateLoyalty)
            {
                result = await _venueRepository.UpdateAssignedLoyalty(venueRequest.Venue.Id, venueRequest.Venue.Loyalty);

                if (result)
                {
                    if (venueRequest.OldVenue.Loyalty.IsActive && !venueRequest.Venue.Loyalty.IsActive)
                    {
                        var userLoyaltyList = await _userLoyaltyRepository.GetUserLoyaltyByAssignedLoyalty(venueRequest.OldVenue.Loyalty.Id, venueRequest.OldVenue.Id);
                        await _loyaltyService.SendDeactivatedLoyaltyNotification(userLoyaltyList);
                    }
                    await _venueRequestRepository.DeleteVenueRequest(venueRequest.Id);
                }
            }

            else if (venueRequest.LastModificationRequest.Type == RequestType.AssignOffer)
            {
                var offer = venueRequest.Venue.Offers.LastOrDefault();
                var assignedOffer = await _offerRepository.AssignOffer(venueRequest.Venue.Id, offer);
                if (assignedOffer != null) await _venueRequestRepository.DeleteVenueRequest(venueRequest.Id);
                if (offer.IsActive)
                    await _notificationService.SendNewOfferNearYouNotifications(offer, venueRequest.Venue);
                result = assignedOffer != null;
            }

            else if (venueRequest.LastModificationRequest.Type == RequestType.UnassignOffer)
            {
                result = await _offerRepository.UnAssignOffer(venueRequest.Venue.Id, venueRequest.LastModificationRequest.ModifiedFieldId);
                if (result)
                {
                    await _venueRequestRepository.DeleteVenueRequest(venueRequest.Id);
                    await _venueRequestRepository.DeleteVenueRequest(venueRequest.Venue.Id, RequestType.UpdateOffer, venueRequest.LastModificationRequest.ModifiedFieldId);

                    await _userOfferRepository.DeleteUserOffersByAssignedOffer(venueRequest.LastModificationRequest.ModifiedFieldId);
                }
            }

            else if (venueRequest.LastModificationRequest.Type == RequestType.UnassignAllOffers)
            {
                result = await _offerRepository.UnAssignOffersFromVenue(venueRequest.Venue.Id);
                if (result)
                {
                    await _venueRequestRepository.DeleteVenueRequest(venueRequest.Id);
                    await _venueRequestRepository.DeleteVenueRequest(venueRequest.Venue.Id, RequestType.UpdateOffer);

                    existingVenue.Offers.Select(a => a.Id).ToList().ForEach(async (offerId) =>
                        await _userOfferRepository.DeleteUserOffersByAssignedOffer(offerId));
                }
            }

            else if (venueRequest.LastModificationRequest.Type == RequestType.UpdateOffer)
            {
                var updatedOffer = venueRequest.Venue.Offers.Where(a => a.Id == venueRequest.LastModificationRequest.ModifiedFieldId).FirstOrDefault();
                var oldOffer = venueRequest.OldVenue.Offers.Where(a => a.Id == venueRequest.LastModificationRequest.ModifiedFieldId).FirstOrDefault();

                result = await _offerRepository.UpdateAssignedOffer(venueRequest.Venue.Id, updatedOffer);
                if (result)
                {
                    if (oldOffer.IsActive && !updatedOffer.IsActive)
                    {
                        var userOffers = await _userOfferRepository.GetUserOffersByAssignedOffer(venueRequest.LastModificationRequest.ModifiedFieldId);
                        await _offerService.SendDeactivatedOfferNotification(userOffers);
                    }
                    await _venueRequestRepository.DeleteVenueRequest(venueRequest.Id);
                }
            }


            if (!await _userManager.IsInRoleAsync(venueCreator, Roles.SuperAdmin))
            {
                var offer = venueRequest.Venue?.Offers?.LastOrDefault();
                if (venueRequest.LastModificationRequest.Type == RequestType.AssignOffer)
                {
                    if (offer.IsActive)
                    {
                        await _notificationComposerService.SendSignalRNotificationToUser(NotificationAction.VenueDetails,
                                                                      $"“{venueRequest.Venue.Name}” Offer “{offer.Type.Name}” has been approved by Super Admin",
                                                                      venueRequest.Venue.Id,
                                                                      venueRequest.LastModificationRequest.CreatedBy);
                    }

                }

                else
                {
                    await _notificationComposerService.SendSignalRNotificationToUser(NotificationAction.VenueDetails,
                                                                         $"“{venueRequest.Venue.Name}” has been approved by Super Admin",
                                                                         venueRequest.Venue.Id,
                                                                         venueRequest.LastModificationRequest.CreatedBy);
                }


                await _notificationComposerService.SendSignalRNotification(NotificationAction.VenueDetails,
                                                      $"“{venueRequest.Venue.Name}” has been updated by Super Admin",
                                                      venueRequest.Venue.Id,
                                                      Roles.VenueAdmin,
                                                      exceptedUserId: venueRequest.LastModificationRequest.CreatedBy,
                                                      accessibleVenue: venueRequest.Venue.Id);
            }
            return result;
        }

        public async Task<bool> RejectVenue(string id)
        {
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var venueRequest = await _venueRequestRepository.GetVenueRequestById(id);
            if (venueRequest == null)
                throw new OutOutException(ErrorCodes.VenueRequestNotFound);

            var existingVenue = await _venueRepository.GetById(venueRequest.Venue.Id);
            if (existingVenue == null)
                throw new OutOutException(ErrorCodes.VenueNotFound);

            return await _venueRequestRepository.DeleteVenueRequest(id);
        }

        public async Task<FullVenueResponse> GetVenue(string id)
        {
            var venue = await CheckAccessibleVenue(id);
            return _mapper.Map<FullVenueResponse>(venue);
        }

        public async Task<VenueMiniSummaryResponse> GetVenueById(string id)
        {
            var venue = await CheckAccessibleVenue(id);
            return _mapper.Map<VenueMiniSummaryResponse>(venue);
        }

        public async Task<List<VenueMiniSummaryResponse>> GetActiveVenuesWithNoLoyalty()
        {
            await _userDetailsProvider.ReInitialize();

            var venues = await _venueRepository.GetActiveVenuesWithNoLoyalty(_userDetailsProvider.User.AccessibleVenues, _userDetailsProvider.IsSuperAdmin);
            return _mapper.Map<List<VenueMiniSummaryResponse>>(venues);
        }
        public async Task<List<VenueMiniSummaryResponse>> GetActiveVenuesWithNoLoyaltyToAllAdmins()
        {
            await _userDetailsProvider.ReInitialize();

            var venues = await _venueRepository.GetActiveVenuesWithNoLoyaltyToAllAdmins();
            return _mapper.Map<List<VenueMiniSummaryResponse>>(venues);
        }
        public async Task<List<VenueSummaryWithBookingResponse>> GetNewestVenues()
        {
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            await _userDetailsProvider.ReInitialize();

            var venues = await _venueRepository.GetNewestVenues(user.AccessibleVenues, _userDetailsProvider.IsSuperAdmin);
            return _mapper.Map<List<VenueSummaryWithBookingResponse>>(venues);
        }

        public async Task<Page<VenueSummaryWithBookingResponse>> GetVenuesPage(PaginationRequest paginationRequest, FilterationRequest filterRequest)
        {
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            await _userDetailsProvider.ReInitialize();

            var venuesPage = await _venueRepository.GetVenuesPage(paginationRequest, filterRequest, user.AccessibleVenues, _userDetailsProvider.IsSuperAdmin);
            return _mapper.Map<Page<VenueSummaryWithBookingResponse>>(venuesPage);
        }

        public async Task<List<VenueSummaryResponse>> GetAllVenues(SearchFilterationRequest searchFilterationRequest)
        {
            await _userDetailsProvider.ReInitialize();

            var venues = await _venueRepository.GetAllVenues(searchFilterationRequest, _userDetailsProvider.User.AccessibleVenues, _userDetailsProvider.IsSuperAdmin);
            return _mapper.Map<List<VenueSummaryResponse>>(venues);
        }

        public async Task<List<VenueMiniSummaryResponse>> GetActiveAccessibleVenues(SearchFilterationRequest searchFilterationRequest)
        {
            await _userDetailsProvider.ReInitialize();

            var venues = await _venueRepository.GetActiveAccessibleVenues(searchFilterationRequest, _userDetailsProvider.User.AccessibleVenues, _userDetailsProvider.IsSuperAdmin);
            return _mapper.Map<List<VenueMiniSummaryResponse>>(venues);
        }

        public async Task<List<VenueMiniSummaryResponse>> GetActiveVenues(SearchFilterationRequest searchFilterationRequest)
        {
            var venues = await _venueRepository.GetActiveVenues(searchFilterationRequest);
            return _mapper.Map<List<VenueMiniSummaryResponse>>(venues);
        }

        public async Task<FullVenueResponse> GetVenueDetailsForAdmin(string id)
        {
            var venue = await CheckAccessibleVenue(id);
            return _mapper.Map<FullVenueResponse>(venue);
        }

        public async Task<Page<OfferWithUsageResponse>> GetOffersPageInVenue(string id, PaginationRequest paginationRequest)
        {
            var venueOneOffer = await _offerRepository.GetOfferByVenueId(id);
            if (venueOneOffer == null)
                throw new OutOutException(ErrorCodes.VenueNotFound);

            return _mapper.Map<Page<OfferWithUsageResponse>>(venueOneOffer.GetPaged(paginationRequest));
        }

        public async Task<Page<EventSummaryResponse>> GetUpcomingEventsPageInVenue(string id, PaginationRequest paginationRequest)
        {
            var venue = await CheckAccessibleVenue(id);
            return await PaginateUpcomingEventsInVenue(venue.Events, paginationRequest);
        }

        public async Task<Page<EventSummaryResponse>> PaginateUpcomingEventsInVenue(List<string> eventIds, PaginationRequest paginationRequest)
        {
            var upcomingEvents = new List<SingleEventOccurrence>();
            if (eventIds.Any())
            {
                var upcomingEventsOccurrences = await _eventRepository.GetUpcomingEvents(eventIds, _userDetailsProvider.GetAccessibleEvents(), _userDetailsProvider.IsSuperAdmin);
                upcomingEvents = upcomingEventsOccurrences.Where(a => a.Occurrence.GetStartDateTime() >= UAEDateTime.Now).GroupBy(a => a.Id).Select(a => a.FirstOrDefault()).ToList();
            }
            var eventsPage = upcomingEvents.GetPaged(paginationRequest);
            return _mapper.Map<Page<EventSummaryResponse>>(eventsPage);
        }

        public async Task<bool> UpdateVenueCode(string id, string code)
        {
            await CheckAccessibleVenue(id);
            return await _venueRepository.UpdateVenueCode(id, code);
        }

        public async Task<Page<VenueSummaryResponse>> GetVenues(PaginationRequest paginationRequest, VenueFilterationRequest filterRequest)
        {
            var user = await _userManager.FindByIdAsync(_userDetailsProvider.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var venuesPage = _venueRepository.GetVenues(paginationRequest, user.Location, filterRequest);
            return _mapper.Map<Page<VenueSummaryResponse>>(venuesPage);
        }

        public async Task<VenueResponse> GetVenueDetails(string venueId, VenueBooking booking = null)
        {
            var user = await _userManager.FindByIdAsync(_userDetailsProvider.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var venue = await _venueRepository.GetById(venueId);
            if (venue == null)
                throw new OutOutException(ErrorCodes.VenueNotFound);

            if (venue.Status != Availability.Active)
                throw new OutOutException(ErrorCodes.UnavailableVenue);

            var venueResponse = _mapper.Map<VenueResponse>(venue);

            venueResponse.Loyalty = await _loyaltyService.GetVenueLoyaltyForUser(venue);
            venueResponse.Offers = await _offerService.GetOffersInVenue(venue);
            venueResponse.UpcomingOffers = await _offerService.GetOffersInVenue(venue, true);

            if (booking != null)
                venueResponse.Booking = _mapper.Map<VenueBookingSummaryResponse>(booking);

            if (venue.Events.Any())
            {
                var eventResult = await _eventRepository.GetUpcomingEvents(venue.Events);
                venueResponse.UpcomingEvents = _mapper.Map<List<EventSummaryResponse>>(eventResult);
            }

            return venueResponse;
        }

        public async Task<bool> UpdateVenueTermsAndConditions(string venueId, List<string> selectedTermsAndConditions)
        {
            await CheckAccessibleVenue(venueId);
            foreach (var tc in selectedTermsAndConditions)
            {
                var existingTermCondition = await _termsAndConditionsRepo.GetById(tc);
                if (existingTermCondition == null)
                    throw new OutOutException(ErrorCodes.TermAndConditionNotFound);
            }

            return await _venueRepository.UpdateTermsAndConditions(venueId, selectedTermsAndConditions);
        }

        public async Task<List<TermsAndConditionsResponse>> GetVenueTermsAndConditions(string venueId)
        {
            var venue = await _venueRepository.GetById(venueId);
            if (venue == null)
                throw new OutOutException(ErrorCodes.VenueNotFound);

            var venueTermsAndConditions = await _termsAndConditionsRepo.GetVenueTermsAndConditions(venue.SelectedTermsAndConditions);

            return _mapper.Map<List<TermsAndConditionsResponse>>(venueTermsAndConditions);
        }

        public async Task<bool> FavoriteVenue(string venueId)
        {
            var user = await _userManager.FindByIdAsync(_userDetailsProvider.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var venue = await _venueRepository.GetById(venueId);
            if (venue == null)
                throw new OutOutException(ErrorCodes.VenueNotFound);

            user.FavoriteVenues.Add(venueId);
            user.FavoriteVenues = user.FavoriteVenues.Distinct().ToList();

            var identityResult = await _userManager.UpdateAsync(user);
            if (!identityResult.Succeeded)
                throw new OutOutException(identityResult);

            return true;
        }

        public async Task<bool> UnfavoriteVenue(string venueId)
        {
            var user = await _userManager.FindByIdAsync(_userDetailsProvider.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var venue = await _venueRepository.GetById(venueId);
            if (venue == null)
                throw new OutOutException(ErrorCodes.VenueNotFound);

            user.FavoriteVenues.Remove(venueId);
            user.FavoriteVenues = user.FavoriteVenues.Distinct().ToList();

            var identityResult = await _userManager.UpdateAsync(user);
            if (!identityResult.Succeeded)
                throw new OutOutException(identityResult);

            return true;
        }

        public async Task<VenueReportResponse> GetVenueReport(string venueId)
        {
            var venue = await CheckAccessibleVenue(venueId);
            return _mapper.Map<VenueReportResponse>(venue);
        }

        public async Task<Page<OfferReportResponse>> GetOffersOverviewForVenueReport(string venueId, PaginationRequest paginationRequest)
        {
            await CheckAccessibleVenue(venueId);
            var offers = await _venueRepository.GetOffersByVenueId(venueId);
            var offersGroupedPage = offers.GroupBy(a => a.Offer.Type.Id).Select(a => a.FirstOrDefault()).ToList().GetPaged(paginationRequest);
            return _mapper.Map<Page<OfferReportResponse>>(offersGroupedPage);
        }

        public async Task<FileResponse> ExportOffersOverviewForVenueReportToExcel(string venueId)
        {
            var existingVenue = await CheckAccessibleVenue(venueId);

            var offers = await _venueRepository.GetOffersByVenueId(venueId);
            var offersGrouped = offers.GroupBy(a => a.Offer.Type.Id).Select(a => a.FirstOrDefault()).ToList();
            var data = _mapper.Map<List<OfferReportResponse>>(offersGrouped);

            var file = ExcelUtils.ExportToExcel(data, $"{existingVenue.Name} - Offers Overview Report").ToArray();
            return new FileResponse(file, $"{existingVenue.Name} - Offers Overview Report.xlsx");
        }

        public async Task<Page<VenueBookingDetailedReportResponse>> GetVenueBookingDetailsReport(string id, PaginationRequest paginationRequest, VenueBookingReportFilterRequest filterRequest)
        {
            await CheckAccessibleVenue(id);

            var bookings = await _venueBookingRepository.GetVenueBookingDetailedReport(id, filterRequest);
            var responseList = _mapper.Map<List<VenueBookingDetailedReportResponse>>(bookings);

            return _mapper.Map<Page<VenueBookingDetailedReportResponse>>(responseList.GetPaged(paginationRequest));
        }

        public async Task<FileResponse> ExportAllVenueBookingsDetailsReportToExcel(string venueId) =>
            await ExportVenueBookingsDetailsReportToExcel(venueId);

        public async Task<FileResponse> ExportSelectedVenueBookingsDetailsReportToExcel(string venueId, string bookingId) =>
            await ExportVenueBookingsDetailsReportToExcel(venueId, new List<string> { bookingId });

        private async Task<FileResponse> ExportVenueBookingsDetailsReportToExcel(string venueId, List<string> bookingsIds = null)
        {
            var venue = await CheckAccessibleVenue(venueId);

            var bookings = await _venueBookingRepository.GetVenueBookingDetailedReport(venueId, null, bookingsIds);
            bookings = bookings.OrderBy(a => a.Status == VenueBookingStatus.Pending ? 0 : 1).ThenByDescending(a => a.Date).ThenBy(a => a.User.FullName).ToList();

            var data = _mapper.Map<List<VenueBookingDetailedReportDTO>>(bookings);

            var file = ExcelUtils.ExportToExcel(data, $"{venue.Name} - Booking Details Report").ToArray();
            return new FileResponse(file, $"{venue.Name} - Booking Details Report.xlsx");
        }

        private async Task<Venue> CheckAccessibleVenue(string id)
        {
            var venue = await _venueRepository.GetById(id);
            if (venue == null)
                throw new OutOutException(ErrorCodes.VenueNotFound);

            await _userDetailsProvider.ReInitialize();
            if (!_userDetailsProvider.HasAccessToVenue(id) && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisVenue, HttpStatusCode.Forbidden);

            return venue;
        }
    }
}
