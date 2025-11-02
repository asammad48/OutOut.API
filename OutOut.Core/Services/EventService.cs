using AutoMapper;
using Microsoft.AspNetCore.Identity;
using OutOut.Constants.Enums;
using OutOut.Constants.Errors;
using OutOut.Infrastructure.Services;
using OutOut.Models.Exceptions;
using OutOut.Models.Identity;
using OutOut.Models.Models;
using OutOut.Models.Models.Embedded;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Extensions;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Providers;
using OutOut.ViewModels.Requests.Events;
using OutOut.ViewModels.Responses.EventBookings;
using OutOut.ViewModels.Responses.Events;
using OutOut.ViewModels.Wrappers;
using System.Net;
using OutOut.ViewModels.Enums;
using OutOut.ViewModels.Requests.Venues;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.Constants;
using OutOut.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using OutOut.ViewModels.Requests.EventBooking;
using OutOut.ViewModels.Responses.Excel;
using OutOut.Core.Utils;
using OutOut.Models.Utils;
using Microsoft.Extensions.Logging;

namespace OutOut.Core.Services
{
    public class EventService
    {
        private readonly IMapper _mapper;
        private readonly IUserDetailsProvider _userDetailsProvider;
        private readonly IEventRepository _eventRepository;
        private readonly IEventBookingRepository _eventBookingRepository;
        private readonly IEventRequestRepository _eventRequestRepository;
        private readonly IVenueRepository _venueRepository;
        private readonly ICityRepository _cityRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly LocationService _locationService;
        private readonly StringLockProvider LockProvider;
        private readonly FileUploaderService _fileUploaderService;
        private readonly AppSettings _appSettings;
        private readonly ICategoryRepository _categoryRepo;
        private readonly NotificationService _notificationService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;
        private readonly NotificationComposerService _notificationComposerService;
        private readonly ILogger<EventService> _logger;

        public EventService(IMapper mapper,
                            IUserDetailsProvider userDetailsProvider,
                            IEventRepository eventRepository,
                            UserManager<ApplicationUser> userManager,
                            IVenueRepository venueRepository,
                            StringLockProvider LockProvider,
                            ICityRepository cityRepository,
                            FileUploaderService fileUploaderService,
                            IOptions<AppSettings> appSettings,
                            ICategoryRepository categoryRepo,
                            IEventRequestRepository eventRequestRepository,
                            LocationService locationService,
                            NotificationService notificationService,
                            INotificationRepository notificationRepository,
                            IEventBookingRepository eventBookingRepository,
                            IUserRepository userRepository,
                            NotificationComposerService notificationComposerService,
                            ILogger<EventService> logger)
        {
            _mapper = mapper;
            _userDetailsProvider = userDetailsProvider;
            _eventRepository = eventRepository;
            _userManager = userManager;
            _venueRepository = venueRepository;
            this.LockProvider = LockProvider;
            _cityRepository = cityRepository;
            _fileUploaderService = fileUploaderService;
            _appSettings = appSettings.Value;
            _categoryRepo = categoryRepo;
            _eventRequestRepository = eventRequestRepository;
            _locationService = locationService;
            _notificationService = notificationService;
            _notificationRepository = notificationRepository;
            _eventBookingRepository = eventBookingRepository;
            _userRepository = userRepository;
            _notificationComposerService = notificationComposerService;
            _logger = logger;
        }

        public async Task<FullEventResponse> CreateEvent(UpsertEventRequest request)
        {
            var newEvent = _mapper.Map<Event>(request);

            await _locationService.IsLocationInAllowedCountriesAsync(request.Location);

            var city = await _cityRepository.GetByArea(request.Location.CityId, request.Location.Area);
            if (city == null)
                throw new OutOutException(ErrorCodes.CityNotFound);
            var citySummary = _mapper.Map<CitySummary>(city);
            newEvent.Location = new Location(request.Location.Longitude, request.Location.Latitude, citySummary, request.Location.Area, request.Location.Description);

            if (request.Image != null)
                newEvent.Image = await _fileUploaderService.UploadFile(_appSettings.Directories.EventsImages, request.Image);

            if (request.HeaderImage != null)
                newEvent.HeaderImage = await _fileUploaderService.UploadFile(_appSettings.Directories.EventsImages, request.HeaderImage);

            if (request.DetailsLogo != null)
                newEvent.DetailsLogo = await _fileUploaderService.UploadFile(_appSettings.Directories.EventsImages, request.DetailsLogo);

            if (request.TableLogo != null)
                newEvent.TableLogo = await _fileUploaderService.UploadFile(_appSettings.Directories.EventsImages, request.TableLogo);

            foreach (var categoryId in request.CategoriesIds)
            {
                var existingCategory = await _categoryRepo.GetById(categoryId);
                if (existingCategory == null || existingCategory.TypeFor != TypeFor.Event)
                    throw new OutOutException(ErrorCodes.CategoryNotFound);
                newEvent.Categories.Add(existingCategory);
            }

            if (!string.IsNullOrEmpty(request.VenueId))
            {
                var existingVenue = await _venueRepository.GetById(request.VenueId);
                if (existingVenue == null)
                    throw new OutOutException(ErrorCodes.VenueNotFound);
                newEvent.Venue = _mapper.Map<VenueSummary>(existingVenue);
            }

            if (!string.IsNullOrEmpty(request.HostedBy) && request.HostImage != null)
            {
                var hostImage = await _fileUploaderService.UploadFile(_appSettings.Directories.VenuesLogos, request.HostImage);
                newEvent.Venue = new VenueSummary { Name = request.HostedBy, Logo = hostImage };
            }

            newEvent.Occurrences = _mapper.Map<List<EventOccurrence>>(request.Occurrences);

            var packages = _mapper.Map<List<EventPackage>>(request.Packages);
            packages.ForEach(a =>
            {
                a.RemainingTickets = a.TicketsNumber;
                a.Id = ObjectId.GenerateNewId().ToString();
                a.Title = a.Title.Trim();
            });
            newEvent.Occurrences.ForEach(a => a.Packages.AddRange(packages));

            newEvent.Name = request.Name.Trim();
            newEvent.CreatedBy = _userDetailsProvider.UserId;
            newEvent.Id = ObjectId.GenerateNewId().ToString();

            if (!request.IsActive) newEvent.Status = Availability.Inactive;

            await _eventRequestRepository.UpsertEventRequest(newEvent, null, RequestType.AddEvent, newEvent.Id);
            var eventRequest = await _eventRequestRepository.GetEventRequestByEventId(newEvent.Id, RequestType.AddEvent);

            await _userDetailsProvider.ReInitialize();
            if (!_userDetailsProvider.IsSuperAdmin)
                await _notificationComposerService.SendSignalRNotification(NotificationAction.EventRequestDetails,
                                                                          $"New Event Added, by “{_userDetailsProvider.User.FullName}”",
                                                                          eventRequest.Id,
                                                                          Roles.SuperAdmin);

            if (_userDetailsProvider.IsSuperAdmin)
            {
                var isApproved = await ApproveEvent(eventRequest?.Id);
                if (!isApproved)
                    throw new OutOutException(ErrorCodes.CouldNotCreateEvent);
            }

            return _mapper.Map<FullEventResponse>(newEvent);
        }

        public async Task<FullEventResponse> UpdateEvent(string eventId, UpsertEventRequest request)
        {
            var existingEvent = await CheckAccessibleEvent(eventId);

            await _locationService.IsLocationInAllowedCountriesAsync(request.Location);

            var city = await _cityRepository.GetByArea(request.Location.CityId, request.Location.Area);
            if (city == null)
                throw new OutOutException(ErrorCodes.CityNotFound);
            var citySummary = _mapper.Map<CitySummary>(city);
            existingEvent.Location = new Location(request.Location.Longitude, request.Location.Latitude, citySummary, request.Location.Area, request.Location.Description);

            if (request.Image != null)
                existingEvent.Image = await _fileUploaderService.UploadFile(_appSettings.Directories.EventsImages, request.Image);

            if (request.HeaderImage != null)
                existingEvent.HeaderImage = await _fileUploaderService.UploadFile(_appSettings.Directories.EventsImages, request.HeaderImage);

            if (request.DetailsLogo != null)
                existingEvent.DetailsLogo = await _fileUploaderService.UploadFile(_appSettings.Directories.EventsImages, request.DetailsLogo);

            if (request.TableLogo != null)
                existingEvent.TableLogo = await _fileUploaderService.UploadFile(_appSettings.Directories.EventsImages, request.TableLogo);

            var newCategories = new List<Category>();
            foreach (var categoryId in request.CategoriesIds)
            {
                var existingCategory = await _categoryRepo.GetById(categoryId);
                if (existingCategory == null || existingCategory.TypeFor != TypeFor.Event)
                    throw new OutOutException(ErrorCodes.CategoryNotFound);
                newCategories.Add(existingCategory);
            }
            existingEvent.Categories = newCategories;

            string currentVenueId = existingEvent?.Venue?.Id;
            VenueSummary venue = null;
            if (!string.IsNullOrEmpty(request.VenueId))
            {
                var existingVenue = await _venueRepository.GetById(request.VenueId);
                if (existingVenue == null)
                    throw new OutOutException(ErrorCodes.VenueNotFound);
                venue = _mapper.Map<VenueSummary>(existingVenue);

                if (existingVenue.Status != Availability.Active && request.IsActive)
                    throw new OutOutException(ErrorCodes.CantActivateEventAssignedToInactiveVenue);
            }
            else if (!string.IsNullOrEmpty(request.HostedBy))
            {
                venue = new VenueSummary() { Name = request.HostedBy, Logo = existingEvent?.Venue?.Logo };
                if (request.HostImage != null)
                    venue.Logo = await _fileUploaderService.UploadFile(_appSettings.Directories.VenuesLogos, request.HostImage);
            }

            if (request.IsActive && (!city.IsActive || city.Areas.Where(a => a == request.Location.Area).FirstOrDefault() == null))
                throw new OutOutException(ErrorCodes.CantActivateEventInAnInactiveCityOrArea);

            var result = new Event();
            try
            {
                await LockProvider.WaitAsync(eventId);

                var currentEvent = await _eventRepository.GetById(eventId);

                request.Packages.ForEach(p => p.Title = p.Title.Trim());

                var newPackages = _mapper.Map<List<EventPackage>>(request.Packages.Where(p => p.Id == null));
                newPackages.ForEach(a =>
                {
                    a.RemainingTickets = a.TicketsNumber;
                    a.Id = ObjectId.GenerateNewId().ToString();
                });
                foreach (var occurrence in currentEvent.Occurrences)
                {
                    var updatedPackages = _mapper.Map<List<EventPackage>>(request.Packages);
                    occurrence.Packages = HandleRemainingTicketsUpdate(occurrence.Packages, updatedPackages);
                    occurrence.Packages.AddRange(newPackages);
                }

                if (request.VenueId == null && currentEvent.Venue != null)
                {
                    var oldVenue = await _venueRepository.GetById(currentEvent.Venue.Id);
                    if (oldVenue != null)
                    {
                        oldVenue.Events.Remove(currentEvent.Id);
                        await _venueRepository.Update(oldVenue);
                    }
                }
                currentEvent.Venue = venue;
                currentEvent.Location = existingEvent.Location;
                currentEvent.Image = existingEvent.Image;
                currentEvent.HeaderImage = existingEvent.HeaderImage;
                currentEvent.DetailsLogo = existingEvent.DetailsLogo;
                currentEvent.TableLogo = existingEvent.TableLogo;
                currentEvent.Categories = existingEvent.Categories;

                if (!request.IsActive && currentEvent.Status == Availability.Active)
                    currentEvent.Status = Availability.Inactive;
                else if (request.IsActive)
                    currentEvent.Status = Availability.Active;

                var oldEvent = await _eventRepository.GetById(eventId);

                var updatedEvent = _mapper.Map(request, currentEvent);

                updatedEvent.Name = request.Name.Trim();

                await _eventRequestRepository.UpsertEventRequest(updatedEvent, oldEvent, RequestType.UpdateEvent, eventId);

                var packagesForNewOccurrences = currentEvent.Occurrences.SelectMany(a => a.Packages).GroupBy(a => a.Id).Select(a => a.FirstOrDefault()).ToList();
                packagesForNewOccurrences.ForEach(p => p.RemainingTickets = p.TicketsNumber);

                await UpdateEventOccurrence(currentEvent, request.Occurrences, packagesForNewOccurrences);

                var eventRequest = await _eventRequestRepository.GetEventRequestByEventId(eventId, RequestType.UpdateEvent);
                result = eventRequest.Event;

                if (oldEvent.ToBsonDocument().Equals(eventRequest.Event.ToBsonDocument()))
                {
                    await _eventRequestRepository.DeleteEventRequest(eventRequest.Id);
                    throw new OutOutException(ErrorCodes.NoChangesHaveBeenMade);
                }

                await _userDetailsProvider.ReInitialize();
                if (!_userDetailsProvider.IsSuperAdmin)
                    await _notificationComposerService.SendSignalRNotification(NotificationAction.EventRequestDetails,
                                                                          $"“{eventRequest.OldEvent.Name}” has been edited, Waiting for approval",
                                                                          eventRequest.Id,
                                                                          Roles.SuperAdmin);

                if (_userDetailsProvider.IsSuperAdmin)
                {
                    var isApproved = await _eventRequestRepository.ApproveEvent(eventRequest.Id, eventRequest.Event);
                    if (!isApproved)
                        throw new OutOutException(ErrorCodes.CouldNotUpdateEvent);
                    if (currentEvent.Venue?.Id != null)
                        await HandleUpdateVenueInEvent(eventRequest.Event, currentVenueId, request.VenueId);
                    if (updatedEvent.Status != Availability.Active)
                        await HandleDeactivateDeleteEvent(new List<string> { updatedEvent.Id });

                    await _notificationComposerService.SendSignalRNotification(NotificationAction.EventDetails,
                                                                        $"“{existingEvent.Name}” has been updated by Super Admin",
                                                                        eventId,
                                                                        new List<string> { Roles.EventAdmin, Roles.VenueAdmin },
                                                                        accessibleEvent: eventId);
                }
            }
            finally
            {
                LockProvider.Release(eventId);
                LockProvider.Delete(eventId);
            }

            return _mapper.Map<FullEventResponse>(result);
        }

        public async Task<bool> DeleteEvent(string id)
        {
            var existingEvent = await CheckAccessibleEvent(id);

            await _eventRequestRepository.UpsertEventRequest(existingEvent, existingEvent, RequestType.DeleteEvent, id);

            await _userDetailsProvider.ReInitialize();

            if (_userDetailsProvider.IsSuperAdmin)
            {
                var eventRequest = await _eventRequestRepository.GetEventRequestByEventId(id, RequestType.DeleteEvent);
                await _notificationComposerService.SendSignalRNotification(NotificationAction.DeleteEvent,
                                                                         $"“{existingEvent.Name}” has been deleted by Super Admin",
                                                                         id,
                                                                         new List<string> { Roles.EventAdmin, Roles.VenueAdmin },
                                                                         accessibleEvent: id);
                await ApproveEvent(eventRequest?.Id);
            }
            return true;
        }

        private async Task HandleUpdateVenueInEvent(Event existingEvent, string existingVenueId, string newVenueId)
        {
            if (string.IsNullOrEmpty(newVenueId) && existingVenueId != null)
            {
                await _venueRepository.RemoveEventFromVenue(existingVenueId, existingEvent.Id);
            }

            if (!string.IsNullOrEmpty(newVenueId) && existingVenueId != newVenueId)
            {
                await _venueRepository.RemoveEventFromVenue(existingVenueId, existingEvent.Id);
                await _venueRepository.AddEventToVenue(newVenueId, existingEvent.Id);
            }
        }

        private List<EventPackage> HandleRemainingTicketsUpdate(List<EventPackage> existingPackages, List<EventPackage> packagesRequest)
        {
            foreach (var package in packagesRequest.ToList())
            {
                var existingPackage = existingPackages.Where(a => a.Id == package.Id).FirstOrDefault();
                var updatedPackage = packagesRequest.Where(a => a.Id == package.Id).FirstOrDefault();

                if (existingPackage == null)
                {
                    packagesRequest.Remove(updatedPackage);
                    continue;
                }

                if (updatedPackage.TicketsNumber < existingPackage.TicketsNumber && existingPackage.RemainingTickets == 0)
                    throw new OutOutException(ErrorCodes.PackageTicketNumberHasZeroRemaining);

                else if (updatedPackage.TicketsNumber != existingPackage.TicketsNumber)
                {
                    long remaining = existingPackage.RemainingTickets + (updatedPackage.TicketsNumber - existingPackage.TicketsNumber);
                    updatedPackage.RemainingTickets = remaining > 0 ? remaining :
                        throw new OutOutException(ErrorCodes.InvalidPackageTicketNumber);
                    continue;
                }
                else if (updatedPackage.TicketsNumber == existingPackage.TicketsNumber)
                    updatedPackage.RemainingTickets = existingPackage.RemainingTickets;
            }

            return packagesRequest;
        }

        private async Task UpdateEventOccurrence(Event currentEvent, List<EventOccurrenceRequest> occurrencesRequest, List<EventPackage> packagesForNewOccurrences)
        {
            #region Occurrence Ids Check
            foreach (var occurrenceRequest in occurrencesRequest.ToList())
            {
                if (string.IsNullOrEmpty(occurrenceRequest.Id))
                    continue;

                var SingleEventOccurrence = _eventRepository.GetSingleEventOccurrenceById(occurrenceRequest.Id);
                if (SingleEventOccurrence == null)
                    throw new OutOutException(ErrorCodes.EventOccurrenceNotFound);
            }
            #endregion

            #region Updating Occurrence

            var newOccurrences = _mapper.Map<List<EventOccurrence>>(occurrencesRequest.Where(e => e.Id == null));
            newOccurrences.ForEach(async (eventOccurrence) =>
            {
                eventOccurrence.Packages = packagesForNewOccurrences;
                await _eventRequestRepository.AddOccurrenceToEvent(currentEvent.Id, eventOccurrence);
            });

            var deletedOccurrencesIds = currentEvent.Occurrences.Select(a => a.Id).Except(occurrencesRequest.Select(a => a.Id)).ToList();
            await _eventRequestRepository.DeleteOccurrenceFromEvent(currentEvent.Id, deletedOccurrencesIds);
            var updatedEventRequest = await _eventRequestRepository.GetEventRequestByEventId(currentEvent.Id, RequestType.UpdateEvent);
            var updatedOccurrences = _mapper.Map(occurrencesRequest, updatedEventRequest?.Event?.Occurrences);
            foreach (var occurrence in updatedOccurrences)
                await _eventRequestRepository.UpdateOccurrenceDateTime(occurrence.Id, occurrence);

            #endregion
        }
        public async Task<bool> ApproveEvent(string id)
        {
            var eventRequest = await _eventRequestRepository.GetEventRequestById(id);
            if (eventRequest == null)
                throw new OutOutException(ErrorCodes.EventRequestNotFound);

            bool result = false;
            var eventCreator = await _userRepository.GetUserById(eventRequest.LastModificationRequest.CreatedBy);
            if (eventRequest.LastModificationRequest.Type == RequestType.AddEvent)
            {
                result = await _eventRequestRepository.ApproveEvent(id, eventRequest.Event);
                if (result)
                {
                    if (eventRequest.Event.Status == Availability.Active)
                        await _notificationService.SendNewEventNearYouNotifications(eventRequest.Event);

                    if (eventRequest.Event.Venue?.Id != null)
                        await _venueRepository.AddEventToVenue(eventRequest.Event.Venue?.Id, eventRequest.Event.Id);

                    if (await _userManager.IsInRoleAsync(eventCreator, Roles.VenueAdmin) || await _userManager.IsInRoleAsync(eventCreator, Roles.EventAdmin))
                        await _userRepository.AddEventIdToAccessibleEvents(eventCreator.Id, eventRequest.Event.Id);
                }
            }

            var existingEvent = await _eventRepository.GetById(eventRequest.Event.Id);
            if (existingEvent == null)
                throw new OutOutException(ErrorCodes.EventNotFound);

            if (eventRequest.LastModificationRequest.Type == RequestType.UpdateEvent)
            {
                result = await ApproveUpdateEvent(eventRequest.Event.Id);
                if (result && eventRequest.Event.Status != Availability.Active)
                    await HandleDeactivateDeleteEvent(new List<string> { eventRequest.Event.Id });
            }

            else if (eventRequest.LastModificationRequest.Type == RequestType.DeleteEvent)
            {
                result = await _eventRepository.Delete(eventRequest.Event.Id);
                if (result)
                {
                    await _venueRepository.RemoveEventFromVenue(eventRequest.Event.Venue?.Id, eventRequest.Event.Id);
                    await _eventBookingRepository.DeleteBookingsForDeletedEvent(eventRequest.Event.Id);
                    await _userRepository.DeleteEventIdFromAccessibleEvents(eventRequest.Event.Id);
                    await HandleDeactivateDeleteEvent(new List<string> { eventRequest.Event.Id });
                    await _eventRequestRepository.DeleteEventRequest(eventRequest.Id);

                    if (!await _userManager.IsInRoleAsync(eventCreator, Roles.SuperAdmin))
                        await _notificationComposerService.SendSignalRNotificationToUser(NotificationAction.DeleteEvent,
                                                          $"Delete request for “{eventRequest.Event.Name}” has been approved by Super Admin",
                                                          eventRequest.Event.Id,
                                                          eventRequest.LastModificationRequest.CreatedBy);
                    return result;
                }
            }

            if (!await _userManager.IsInRoleAsync(eventCreator, Roles.SuperAdmin))
            {
                await _notificationComposerService.SendSignalRNotificationToUser(NotificationAction.EventDetails,
                                                                          $"“{eventRequest.Event.Name}” has been approved by Super Admin",
                                                                          eventRequest.Event.Id,
                                                                          eventRequest.LastModificationRequest.CreatedBy);

                await _notificationComposerService.SendSignalRNotification(NotificationAction.EventDetails,
                                      $"“{eventRequest.Event.Name}” has been updated by Super Admin",
                                      eventRequest.Event.Id,
                                       new List<string> { Roles.EventAdmin, Roles.VenueAdmin },
                                      exceptedUserId: eventRequest.LastModificationRequest.CreatedBy,
                                      accessibleVenue: null,
                                      accessibleEvent: eventRequest.Event.Id);
            }
            return result;
        }

        public async Task<bool> RejectEvent(string id)
        {
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var eventRequest = await _eventRequestRepository.GetEventRequestById(id);
            if (eventRequest == null)
                throw new OutOutException(ErrorCodes.EventRequestNotFound);

            return await _eventRequestRepository.DeleteEventRequest(id);
        }

        private async Task<bool> ApproveUpdateEvent(string eventId)
        {
            bool result = false;
            try
            {
                await LockProvider.WaitAsync(eventId);

                var currentEvent = await _eventRepository.GetById(eventId);
                var eventRequest = await _eventRequestRepository.GetEventRequestByEventId(eventId, RequestType.UpdateEvent);

                var packagesRequest = eventRequest.Event.Occurrences.SelectMany(a => a.Packages).GroupBy(a => a.Id).Select(a => a.FirstOrDefault()).ToList();
                var existingPackages = currentEvent.Occurrences.SelectMany(a => a.Packages).GroupBy(a => a.Id).Select(a => a.FirstOrDefault()).ToList();

                HandleRemainingTicketsUpdate(existingPackages, packagesRequest);

                result = await _eventRequestRepository.ApproveEvent(eventRequest.Id, eventRequest.Event);

                await HandleUpdateVenueInEvent(currentEvent, currentEvent.Venue?.Id, eventRequest.Event?.Venue?.Id);
            }
            finally
            {
                LockProvider.Release(eventId);
                LockProvider.Delete(eventId);
            }
            return result;
        }

        public async Task<EventMiniSummaryResponse> GetEventById(string eventId)
        {
            var existingEvent = await CheckAccessibleEvent(eventId);
            return _mapper.Map<EventMiniSummaryResponse>(existingEvent);
        }

        public async Task<Page<EventSummaryResponse>> GetEvents(PaginationRequest paginationRequest, EventFilterationRequest filterRequest)
        {
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var events = await _eventRepository.GetEvents(filterRequest, user.Location);

            if (filterRequest?.EventFilter != EventFilter.Today)
                events = events.Where(a => a.Occurrence.GetStartDateTime() >= UAEDateTime.Now).GroupBy(a => a.Id).Select(a => a.FirstOrDefault()).ToList();

            var result = events.GetPaged(paginationRequest);
            return _mapper.Map<Page<EventSummaryResponse>>(result);
        }

        public async Task HandleDeactivateDeleteEvent(List<string> eventsIds)
        {
            await _eventBookingRepository.DeleteBookingRemindersForDeactivatedEvent(eventsIds);
            await _notificationRepository.DeleteRemindersWithDeactivatedEvent(eventsIds);
        }

        public async Task<Page<EventSummaryWithBookingResponse>> GetEventsPage(PaginationRequest paginationRequest, FilterationRequest filterRequest)
        {
            await _userDetailsProvider.ReInitialize();

            var eventsOcccurrences = await _eventRepository.GetAllEventsOccurrences(paginationRequest, filterRequest, _userDetailsProvider.GetAccessibleEvents(), _userDetailsProvider.IsSuperAdmin);
            return _mapper.Map<Page<EventSummaryWithBookingResponse>>(eventsOcccurrences);
        }

        public async Task<Page<EventSummaryWithBookingResponse>> GetFeaturedEventsPage(PaginationRequest paginationRequest, FilterationRequest filterRequest)
        {
            await _userDetailsProvider.ReInitialize();

            var eventsOcccurrences = await _eventRepository.GetFeaturedEventsPage(paginationRequest, filterRequest);
            return _mapper.Map<Page<EventSummaryWithBookingResponse>>(eventsOcccurrences);
        }

        public async Task<FullEventResponse> GetEventDetailsForAdmin(string id)
        {
            var eventResult = await CheckAccessibleEvent(id);
            return _mapper.Map<FullEventResponse>(eventResult);
        }

        public async Task<bool> UpdateIsFeatured(string id, bool isFeatured)
        {
            var existingEvent = await CheckAccessibleEvent(id);
            var isUpdated = await _eventRepository.UpdateIsFeatured(id, isFeatured);

            if (isUpdated & isFeatured)
                await _notificationComposerService.SendSignalRNotification(NotificationAction.EventDetails,
                                                                         $"“{existingEvent.Name}” is now set as featured",
                                                                         id,
                                                                         new List<string> { Roles.EventAdmin, Roles.VenueAdmin },
                                                                         accessibleEvent: id);

            if (isUpdated & !isFeatured)
                await _notificationComposerService.SendSignalRNotification(NotificationAction.EventDetails,
                                                                         $"“{existingEvent.Name}” is unset as featured",
                                                                         id,
                                                                         new List<string> { Roles.EventAdmin, Roles.VenueAdmin },
                                                                         accessibleEvent: id);
            return isUpdated;
        }

        public async Task<bool> UnsetAllFeaturedEvents()
        {
            return await _eventRepository.UnsetAllFeaturedEvents();
        }

        public async Task<bool> UpdateEventCode(string id, string code)
        {
            await CheckAccessibleEvent(id);
            return await _eventRepository.UpdateEventCode(id, code);
        }

        public async Task<List<EventSummaryResponse>> GetAllEvents(SearchFilterationRequest searchFilterationRequest)
        {
            await _userDetailsProvider.ReInitialize();

            var events = await _eventRepository.GetAllEvents(searchFilterationRequest, _userDetailsProvider.GetAccessibleEvents(), _userDetailsProvider.IsSuperAdmin);
            return _mapper.Map<List<EventSummaryResponse>>(events);
        }
        public async Task<List<EventSummaryResponse>> GetAllEventsNotAssociatedToVenue(string venueId, SearchFilterationRequest searchFilterationRequest)
        {
            await _userDetailsProvider.ReInitialize();

            var events = await _eventRepository.GetAllEventsNotAssociatedToVenue(venueId, searchFilterationRequest, _userDetailsProvider.GetAccessibleEvents(), _userDetailsProvider.IsSuperAdmin);
            return _mapper.Map<List<EventSummaryResponse>>(events);
        }
        public async Task<SingleEventOccurrenceResponse> GetEventDetails(string eventOccurrenceId, EventBooking eventBooking = null)
        {
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var eventResult = await _eventRepository.GetEventByOccurrenceId(eventOccurrenceId);
            if (eventResult == null)
                throw new OutOutException(ErrorCodes.EventNotFound);

            if (eventResult.Status != Availability.Active)
                throw new OutOutException(ErrorCodes.UnavailableEvent);

            var eventOccurrence = _mapper.Map<SingleEventOccurrenceResponse>(eventResult);

            eventOccurrence.IsFavorite = user.FavoriteEvents.Contains(eventOccurrenceId);
            eventOccurrence.Occurrence = eventOccurrence.Occurrences.Where(a => a.Id == eventOccurrenceId).FirstOrDefault();
            eventOccurrence.Occurrences = eventOccurrence.Occurrences.Where(a => a.StartDate > UAEDateTime.Now.Date || (a.StartDate == UAEDateTime.Now.Date && TimeSpan.Parse(a.StartTime) >= UAEDateTime.Now.TimeOfDay))
                                                                     .OrderBy(o => o.StartDate)
                                                                     .ThenBy(o => o.StartTime)
                                                                     .ToList();
            if (eventBooking != null)
            {
                eventOccurrence.Booking = _mapper.Map<EventBookingMiniSummaryResponse>(eventBooking);
                if (eventOccurrence.Booking.Tickets != null)
                {
                    eventOccurrence.Booking.Tickets = eventOccurrence.Booking.Tickets.OrderBy(t => t.Id).ToList();
                }
            }

            return eventOccurrence;
        }

        public async Task<List<EventSummaryWithBookingResponse>> GetUpcomingEvents()
        {
            await _userDetailsProvider.ReInitialize();

            var events = await _eventRepository.GetUpcomingEvents(_userDetailsProvider.GetAccessibleEvents(), _userDetailsProvider.IsSuperAdmin);
            events = events.Where(a => a.Occurrence.GetStartDateTime() >= UAEDateTime.Now).GroupBy(a => a.Id).Select(a => a.FirstOrDefault()).ToList();

            var result = events.Take(3);
            return _mapper.Map<List<EventSummaryWithBookingResponse>>(result);
        }

        public async Task<List<EventSummaryWithBookingResponse>> GetOngoingEvents()
        {
            await _userDetailsProvider.ReInitialize();

            var events = await _eventRepository.GetOngoingEvents(_userDetailsProvider.GetAccessibleEvents(), _userDetailsProvider.IsSuperAdmin);
            foreach (var eventOccurrence in events.ToList())
            {
                var currentDateTime = UAEDateTime.Now;
                if (eventOccurrence.Occurrence.StartTime == eventOccurrence.Occurrence.EndTime)
                    //continue;
                    events.Remove(eventOccurrence);

                if (eventOccurrence.Occurrence.StartTime <= eventOccurrence.Occurrence.EndTime)
                {
                    if (currentDateTime.TimeOfDay >= eventOccurrence.Occurrence.StartTime && currentDateTime.TimeOfDay <= eventOccurrence.Occurrence.EndTime
                         && currentDateTime.Date >= eventOccurrence.Occurrence.StartDate && currentDateTime.Date <= eventOccurrence.Occurrence.EndDate)
                        continue;
                    else
                        events.Remove(eventOccurrence);
                }
                else
                {
                    if (currentDateTime.TimeOfDay >= eventOccurrence.Occurrence.StartTime)
                    {
                        var occurrenceStartTimeTicks = currentDateTime.Date.Add(eventOccurrence.Occurrence.StartTime).Ticks;
                        var occurrenceEndTimeTicks = currentDateTime.AddDays(1).Date.Add(eventOccurrence.Occurrence.EndTime).Ticks;

                        if (currentDateTime.Ticks >= occurrenceStartTimeTicks && currentDateTime.Ticks <= occurrenceEndTimeTicks)
                            continue;

                        else
                        {
                            events.Remove(eventOccurrence);
                            continue;
                        }
                    }
                    else
                    {
                        var occurrenceStartTimeTicks = currentDateTime.Date.AddDays(-1).Add(eventOccurrence.Occurrence.StartTime).Ticks;
                        var occurrenceEndTimeTicks = currentDateTime.Date.Add(eventOccurrence.Occurrence.EndTime).Ticks;

                        if (currentDateTime.Ticks >= occurrenceStartTimeTicks && currentDateTime.Ticks <= occurrenceEndTimeTicks)
                            continue;

                        else
                        {
                            events.Remove(eventOccurrence);
                            continue;
                        }
                    }
                }
            }

            return _mapper.Map<List<EventSummaryWithBookingResponse>>(events);
        }

        public async Task<bool> FavoriteEventOccurrence(string eventOccurrenceId)
        {
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var eventResult = _eventRepository.GetSingleEventOccurrenceById(eventOccurrenceId);
            if (eventResult == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            user.FavoriteEvents.Add(eventOccurrenceId);
            user.FavoriteEvents = user.FavoriteEvents.Distinct().ToList();

            var identityResult = await _userManager.UpdateAsync(user);
            if (!identityResult.Succeeded)
                throw new OutOutException(identityResult);

            return true;
        }

        public async Task<bool> UnfavoriteEventOccurrence(string eventOccurrenceId)
        {
            var user = await _userManager.FindByIdAsync(_userDetailsProvider.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var eventResult = _eventRepository.GetSingleEventOccurrenceById(eventOccurrenceId);
            if (eventResult == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            user.FavoriteEvents.Remove(eventOccurrenceId);
            user.FavoriteEvents = user.FavoriteEvents.Distinct().ToList();

            var identityResult = await _userManager.UpdateAsync(user);
            if (!identityResult.Succeeded)
                throw new OutOutException(identityResult);

            return true;
        }

        public async Task<EventReportResponse> GetEventReport(string id)
        {
            var existingEvent = await CheckAccessibleEvent(id);
            return _mapper.Map<EventReportResponse>(existingEvent);
        }

        public async Task<Page<PackageOverviewReportResponse>> GetPackagesOverviewReport(string eventId, PaginationRequest paginationRequest, EventBookingReportFilterRequest filterRequest, List<string> packagesIds = null)
        {
            var existingEvent = await CheckAccessibleEvent(eventId);
            var packages = packagesIds == null ? existingEvent.Occurrences.SelectMany(a => a.Packages).GroupBy(a => a.Id).Select(a => a.FirstOrDefault()).ToList() :
                                                 existingEvent.Occurrences.SelectMany(a => a.Packages).GroupBy(a => a.Id).Select(a => a.FirstOrDefault()).ToList().Where(a => packagesIds.Contains(a.Id));
            var packagesResponse = _mapper.Map<List<PackageOverviewReportResponse>>(packages);
            packagesResponse.ForEach(package =>
            {
                package.TotalTicketsBooked = _eventBookingRepository.GetBookedTicketsCountPerPackage(eventId, package.Id, filterRequest);
                package.TotalTicketsCancelled = _eventBookingRepository.GetRejectedTicketsCountPerPackage(eventId, package.Id, filterRequest);
                package.TotalTicketsRemaining = existingEvent.Occurrences.SelectMany(a => a.Packages).Where(a => a.Id == package.Id).Sum(a => a.RemainingTickets);
                package.TotalSales = _eventBookingRepository.GetTotalSalesPerPackage(eventId, package.Id, filterRequest);
            });
            return packagesResponse.OrderByDescending(p => p.TotalSales).GetPaged(paginationRequest);
        }

        public async Task<FileResponse> ExportAllPackagesOverviewReportToExcel(string eventId) =>
            await ExportPackagesOverviewReportToExcel(eventId);

        public async Task<FileResponse> ExportSelectedPackagesOverviewReportToExcel(string eventId, string packageId) =>
            await ExportPackagesOverviewReportToExcel(eventId, new List<string> { packageId });

        private async Task<FileResponse> ExportPackagesOverviewReportToExcel(string eventId, List<string> packagesIds = null)
        {
            var existingEvent = await CheckAccessibleEvent(eventId);

            var packagesPage = await GetPackagesOverviewReport(eventId, PaginationRequest.Max, null, packagesIds);
            var data = packagesPage.Records.Select(p => new { p.PackageName, p.TotalTicketsBooked, p.TotalTicketsCancelled, p.TotalTicketsRemaining, p.TotalSales, p.NetPrice }).ToList();

            var file = ExcelUtils.ExportToExcel(data, $"{existingEvent.Name} - Booking Overview Report").ToArray();
            return new FileResponse(file, $"{existingEvent.Name} - Booking Overview Report.xlsx");
        }

        public async Task<Page<EventBookingDetailedReportResponse>> GetEventBookingDetailsReport(string id, PaginationRequest paginationRequest, EventBookingReportFilterRequest filterRequest)
        {
            var existingEvent = await CheckAccessibleEvent(id);

            var bookings = await _eventBookingRepository.GetEventBookingDetailedReport(id, filterRequest);
            var responseList = _mapper.Map<List<EventBookingDetailedReportResponse>>(bookings);

            if (filterRequest.Sort != null)
            {
                responseList = filterRequest?.Sort switch
                {
                    EventBookingReportSort.Newest => responseList.OrderByDescending(a => a.CreatedDate).ThenBy(a => a.User.FullName).ToList(),
                    EventBookingReportSort.Alphabetical => responseList.OrderBy(a => a.User.FullName).ThenByDescending(a => a.CreatedDate).ToList(),
                    EventBookingReportSort.Attended => responseList.OrderByDescending(a => a.Attendees).ThenByDescending(a => a.CreatedDate).ThenBy(a => a.User.FullName).ToList(),
                    EventBookingReportSort.Absent => responseList.OrderByDescending(a => a.Absentees).ThenByDescending(a => a.CreatedDate).ThenBy(a => a.User.FullName).ToList(),
                    (_) => responseList.OrderByDescending(a => a.Attendees).ThenByDescending(a => a.CreatedDate).ThenBy(a => a.User.FullName).ToList(),
                };
            }

            return _mapper.Map<Page<EventBookingDetailedReportResponse>>(responseList.GetPaged(paginationRequest));
        }

        public async Task<FileResponse> ExportAllEventBookingsDetailsReportToExcel(string eventId) =>
            await ExportEventBookingsDetailsReportToExcel(eventId);

        public async Task<FileResponse> ExportSelectedEventBookingsDetailsReportToExcel(string eventId, string bookingId) =>
           await ExportEventBookingsDetailsReportToExcel(eventId, new List<string> { bookingId });

        private async Task<FileResponse> ExportEventBookingsDetailsReportToExcel(string eventId, List<string> bookingsIds = null)
        {
            var existingEvent = await CheckAccessibleEvent(eventId);

            var bookings = await _eventBookingRepository.GetEventBookingDetailedReport(eventId, null, bookingsIds);
            var data = _mapper.Map<List<EventBookingDetailedReportDTO>>(bookings);

            var file = ExcelUtils.ExportToExcel(data, $"{existingEvent.Name} - Booking Details Report").ToArray();
            return new FileResponse(file, $"{existingEvent.Name} - Booking Details Report.xlsx");
        }

        private async Task<Event> CheckAccessibleEvent(string id)
        {
            var existingEvent = await _eventRepository.GetById(id);
            if (existingEvent == null)
                throw new OutOutException(ErrorCodes.EventNotFound);

            await _userDetailsProvider.ReInitialize();
            if (!_userDetailsProvider.HasAccessToEvent(id) && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisEvent, HttpStatusCode.Forbidden);

            return existingEvent;
        }
    }
}
