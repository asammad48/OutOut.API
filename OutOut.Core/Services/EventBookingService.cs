using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using OutOut.Constants;
using OutOut.Constants.Enums;
using OutOut.Constants.Errors;
using OutOut.Core.Utils;
using OutOut.Infrastructure.Services;
using OutOut.Models.Domains;
using OutOut.Models.Exceptions;
using OutOut.Models.Identity;
using OutOut.Models.Models;
using OutOut.Models.Models.Embedded;
using OutOut.Models.Utils;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Extensions;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Providers;
using OutOut.ViewModels.Requests.EventBooking;
using OutOut.ViewModels.Requests.EventBookings;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Requests.Reminders;
using OutOut.ViewModels.Requests.Ticket;
using OutOut.ViewModels.Responses.EventBookings;
using OutOut.ViewModels.Responses.Events;
using OutOut.ViewModels.Wrappers;
using System.Net;

namespace OutOut.Core.Services
{
    public class EventBookingService
    {
        private readonly IEventBookingRepository _eventBookingRepository;
        private readonly PaymentService _paymentService;
        private readonly IEventRepository _eventRepository;
        private readonly EventService _eventService;
        private readonly IVenueRepository _venueRepository;
        private readonly IUserDetailsProvider _userDetailsProvider;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly NotificationComposerService _notificationComposerService;
        private readonly StringLockProvider LockProvider;
        private readonly IMapper _mapper;
        private readonly INotificationRepository _notificationRepository;
        private const string KEY_EventOrderNumber = "Event Order Number";
        private readonly ILogger<EventBookingService> _logger;

        public EventBookingService(IEventBookingRepository eventBookingRepository, PaymentService paymentService, IEventRepository eventRepository, IVenueRepository venueRepository, IUserDetailsProvider userDetailsProvider, IUserRepository userRepository, UserManager<ApplicationUser> userManager, NotificationComposerService notificationComposerService, StringLockProvider lockProvider, IMapper mapper, INotificationRepository notificationRepository, ILogger<EventBookingService> logger, EventService eventService)
        {
            _eventBookingRepository = eventBookingRepository;
            _paymentService = paymentService;
            _eventRepository = eventRepository;
            _venueRepository = venueRepository;
            _userDetailsProvider = userDetailsProvider;
            _userRepository = userRepository;
            _userManager = userManager;
            _notificationComposerService = notificationComposerService;
            LockProvider = lockProvider;
            _mapper = mapper;
            _notificationRepository = notificationRepository;
            _logger = logger;
            _eventService = eventService;
        }

        public async Task<SingleEventOccurrenceResponse> GetBookingDetails(string eventBookingId)
        {
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var booking = await _eventBookingRepository.GetById(eventBookingId);
            if (booking == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            var isTicketShared = _userDetailsProvider.User.SharedTickets.Where(b => b.BookingId == eventBookingId).FirstOrDefault() != null;
            if (booking.User.Id != _userDetailsProvider.UserId
                && !isTicketShared)
                throw new OutOutException(ErrorCodes.Unauthorized);

            return await _eventService.GetEventDetails(booking.Event.Occurrence.Id, booking);
        }

        public async Task<SingleEventOccurrenceResponse> GetSharedTicketDetails(string ticketId)
        {
            await _userDetailsProvider.ReInitialize();
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var booking = await _eventBookingRepository.GetEventBookingByTicketId(ticketId);
            if (booking == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            var sharedTicketIds = user.SharedTickets.Select(a => a.TicketId).ToList();
            if (!sharedTicketIds.Contains(ticketId) && booking.User.Id != user.Id)
            {
                _logger.LogError($"Booking of id {booking.Id} and Ticket of id {ticketId} is not added to user's shared tickets and they're not the onwer");
                throw new OutOutException(ErrorCodes.RequestNotFound);
            }

            if (booking.User.Id != user.Id)
            {
                booking.Tickets.RemoveAll(t => !sharedTicketIds.Contains(t.Id));
                booking.Quantity = booking.Tickets.Count;
                booking.Reminders = user.SharedTickets.Where(a => a.BookingId == booking.Id).FirstOrDefault().Reminders;
            }

            return await _eventService.GetEventDetails(booking.Event.Occurrence.Id, booking);
        }

        public async Task<Page<EventBookingSummaryResponse>> GetCustomersOrdersForEvent(string id, PaginationRequest paginationRequest, FilterationRequest filterRequest)
        {
            var eventResult = await _eventRepository.GetById(id);
            if (eventResult == null)
                throw new OutOutException(ErrorCodes.EventNotFound);

            await _userDetailsProvider.ReInitialize();
            if (!_userDetailsProvider.HasAccessToEvent(id) && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisEvent, HttpStatusCode.Forbidden);

            var eventBookings = await _eventBookingRepository.GetBookingsByEventId(id, filterRequest);
            return _mapper.Map<Page<EventBookingSummaryResponse>>(eventBookings.GetPaged(paginationRequest));
        }

        public async Task UpdatePackageRemainingTickets(EventOccurrence eventOccurrence, string eventPackageId, int ticketsQuantity)
        {
            try
            {
                await LockProvider.WaitAsync(eventOccurrence.Id);
                await _eventRepository.UpdatePackageRemainingTickets(eventOccurrence.Id, eventPackageId, ticketsQuantity);
            }
            finally
            {
                LockProvider.Release(eventOccurrence.Id);
                LockProvider.Delete(eventOccurrence.Id);
            }
        }

        public async Task HandlePaidBooking(EventBooking eventBooking)
        {
            if (eventBooking.Tickets.Any())
                return;

            var eventResult = _eventRepository.GetSingleEventOccurrenceById(eventBooking.Event.Occurrence.Id);

            var eventPackage = eventResult.Occurrence.Packages.Where(a => a.Id == eventBooking.Package.Id).FirstOrDefault();

            var packageSummary = _mapper.Map<EventPackageSummary>(eventPackage);
            for (var i = 0; i < eventBooking.Quantity; i++)
            {
                eventBooking.Tickets.Add(new Ticket { Package = packageSummary, Secret = CreateSecret() });
            }

            eventBooking.Status = PaymentStatus.Paid;
            await _eventBookingRepository.UpdateEventBooking(eventBooking);

            await _notificationComposerService.SendSignalRNotification(NotificationAction.NewEventBooking,
                                                                        $"Order has been placed for “{eventResult.Name}”",
                                                                        eventBooking.Id,
                                                                        new List<string> { Roles.EventAdmin, Roles.VenueAdmin },
                                                                        accessibleEvent: eventResult.Id);
        }

        public async Task HandleAbortedBooking(EventBooking eventBooking)
        {
            await UpdatePackageRemainingTickets(eventBooking.Event.Occurrence, eventBooking.Package.Id, eventBooking.Quantity);

            eventBooking.Status = PaymentStatus.Aborted;
            await _eventBookingRepository.Update(eventBooking);
        }

        public async Task HandleExpiredBooking(EventBooking eventBooking)
        {
            await UpdatePackageRemainingTickets(eventBooking.Event.Occurrence, eventBooking.Package.Id, eventBooking.Quantity);

            eventBooking.Status = PaymentStatus.Expired;
            await _eventBookingRepository.Update(eventBooking);
        }

        public async Task HandleCancelledBooking(EventBooking eventBooking)
        {
            var _eventBooking = _eventBookingRepository.GetById(eventBooking.Id);
            if (_eventBooking.Status != TaskStatus.Canceled)
            {
                await UpdatePackageRemainingTickets(eventBooking.Event.Occurrence, eventBooking.Package.Id, eventBooking.Quantity);

                eventBooking.Status = PaymentStatus.Cancelled;
                await _eventBookingRepository.Update(eventBooking);
            }
        }

        public async Task HandleDeclinedBooking(EventBooking eventBooking)
        {
            await UpdatePackageRemainingTickets(eventBooking.Event.Occurrence, eventBooking.Package.Id, eventBooking.Quantity);

            eventBooking.Status = PaymentStatus.Declined;
            await _eventBookingRepository.Update(eventBooking);
        }

        public async Task<EventBookingSummaryResponse> RejectBooking(string eventBookingId)
        {
            var eventBooking = await _eventBookingRepository.GetById(eventBookingId);
            if (eventBooking == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            if (eventBooking.Status != PaymentStatus.Paid)
                throw new OutOutException(ErrorCodes.UnableToCancelEventBooking);

            eventBooking.Status = PaymentStatus.Rejected;
            eventBooking.Tickets.Clear();

            var updatedBooking = await _eventBookingRepository.Update(eventBooking);

            await UpdatePackageRemainingTickets(eventBooking.Event.Occurrence, eventBooking.Package.Id, eventBooking.Quantity);

            var bookingOwner = await _userRepository.GetUserById(eventBooking.User.Id);
            var notification = new Notification(NotificationType.Notification,
                                                eventBooking.User.Id,
                                                $"Booking Cancellation",
                                                $"Your booking for {eventBooking.Event.Name} has been cancelled",
                                                "event.png",
                                                NotificationAction.EventBookingRejection,
                                                eventBooking: _mapper.Map<EventBookingSummary>(eventBooking));
            await _notificationRepository.Create(notification);
            await _notificationComposerService.SendNotification(notification, bookingOwner, NotificationAction.EventBookingRejection, eventBooking.Event.Id);

            return _mapper.Map<EventBookingSummaryResponse>(updatedBooking);
        }

        public async Task HandleOnHoldBooking(EventBooking eventBooking)
        {
            eventBooking.Status = PaymentStatus.OnHold;
            await _eventBookingRepository.Update(eventBooking);
        }

        public async Task<TelrBookingResponse> MakeATelrBooking(EventBookingRequest request)
        {
            var user = await _userManager.FindByIdAsync(_userDetailsProvider.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var eventOccurrenceResult = _eventRepository.GetSingleEventOccurrenceById(request.EventOccurrenceId);
            if (eventOccurrenceResult == null)
                throw new OutOutException(ErrorCodes.EventOccurrenceNotFound);

            if (eventOccurrenceResult.Status != Availability.Active)
                throw new OutOutException(ErrorCodes.UnavailableEvent);

            if (eventOccurrenceResult.Occurrence.GetStartDateTime() <= UAEDateTime.Now)
                throw new OutOutException(ErrorCodes.InvalidBookingDate);

            var eventPackage = eventOccurrenceResult.Occurrence.Packages.Where(a => a.Id == request.PackageId).FirstOrDefault();
            if (eventPackage == null)
                throw new OutOutException(ErrorCodes.PackageNotFound);

            if (eventPackage.RemainingTickets == 0)
                throw new OutOutException(ErrorCodes.TicketsSoldOut);
            if (eventPackage.RemainingTickets < request.Quantity)
                throw new OutOutException(ErrorCodes.ExceededQuantityOfRemainingTickets, eventPackage.RemainingTickets == 1 ?
                    $"The package selected has only 1 ticket left." :
                    $"The package selected has only {eventPackage.RemainingTickets} tickets left.");

            var actualAmount = request.Quantity * eventPackage.Price;
            request.TotalAmount = request.TotalAmount == actualAmount ? actualAmount : throw new OutOutException(ErrorCodes.TotalAmountIsNotCorrect);

            await UpdatePackageRemainingTickets(eventOccurrenceResult.Occurrence, eventPackage.Id, -request.Quantity);

            var packageSummary = _mapper.Map<EventPackageSummary>(eventPackage);
            var venue = await _venueRepository.GetByEventId(eventOccurrenceResult.Id);
            var eventBooking = new EventBooking
            {
                PaymentGateway = request.TotalAmount == 0 ? PaymentGateway.None : PaymentGateway.Telr,
                User = _mapper.Map<ApplicationUserSummary>(user),
                Venue = _mapper.Map<VenueSummary>(venue),
                Event = eventOccurrenceResult,
                TotalAmount = request.TotalAmount,
                OrderNumber = await _eventBookingRepository.GenerateLastIncrementalNumber(KEY_EventOrderNumber),
                Description = $"Booking {eventOccurrenceResult.Name}",
                Status = request.TotalAmount == 0 ? PaymentStatus.Paid : PaymentStatus.Pending,
                Quantity = request.Quantity,
                Package = packageSummary,
                CreatedDate = UAEDateTime.Now,
                LastModifiedDate = UAEDateTime.Now
            };


            // In case will be paid amount =0 then skip make telr payment step
            TelrCreateResponse paymentResult = null;
            if (eventBooking.PaymentGateway == PaymentGateway.Telr)
            {
                paymentResult = await _paymentService.CreateTelrTransaction(eventBooking);

                if (paymentResult?.Order?.Url == null)
                {
                    eventBooking.Status = PaymentStatus.Failed;
                    await _eventBookingRepository.Create(eventBooking);

                    throw new OutOutException(ErrorCodes.Telr_TransactionError);
                }
                eventBooking.OrderReference = paymentResult?.Order?.Ref;
            }

            await _eventBookingRepository.Create(eventBooking);

            //if payment gatway is set to none that's means that the package price is 0 so after creating a booking 
            // we can reserve the tickets for users
            if (eventBooking.PaymentGateway == PaymentGateway.None)
                await HandlePaidBooking(eventBooking);


            return new TelrBookingResponse(paymentResult?.Order?.Url, eventBooking.Id);
        }

        public async Task<string> HandleTelrBooking(string eventBookingId)
        {
            var eventBooking = await _eventBookingRepository.GetById(eventBookingId);
            if (eventBooking == null)
                throw new OutOutException(ErrorCodes.Telr_InvalidEventBookingId);

            if (eventBooking.Status != PaymentStatus.Pending)
                return eventBooking.Status.ToString();

            var response = await _paymentService.CheckTelrTransaction(eventBooking.OrderReference);

            if (response?.Order?.Status?.Text == null)
                throw new OutOutException(ErrorCodes.Telr_TransactionError);

            while (response?.Order?.Status?.Text == PaymentStatus.Pending.ToString())
            {
                await Task.Delay(5000);
                response = await _paymentService.CheckTelrTransaction(eventBooking.OrderReference);
            }

            if (response?.Order?.Status?.Text == PaymentStatus.Paid.ToString())
            {
                try
                {
                    await HandlePaidBooking(eventBooking);
                }
                catch (Exception ex)
                {
                    Serilog.Log.Fatal(ex, $"An error occurred while creating tickets for booking ID : {eventBooking.Id}");
                    // TODO: Send email to the user 
                }
            }
            else if (response?.Order?.Status?.Text == PaymentStatus.Cancelled.ToString())
                await HandleCancelledBooking(eventBooking);
            else if (response?.Order?.Status?.Text == PaymentStatus.Declined.ToString())
                await HandleDeclinedBooking(eventBooking);
            else if (response?.Order?.Status?.Text == PaymentStatus.Expired.ToString())
                await HandleExpiredBooking(eventBooking);
            else if (response?.Order?.Status?.Text.FromTelrStatus() == PaymentStatus.OnHold)
                await HandleOnHoldBooking(eventBooking);

            return response?.Order?.Status?.Text;
        }

        public async Task AbortPayment(string eventBookingId)
        {
            var eventBooking = await _eventBookingRepository.GetById(eventBookingId);
            if (eventBooking == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            var user = await _userManager.FindByIdAsync(_userDetailsProvider.UserId);
            if (user == null || eventBooking.User.Id != _userDetailsProvider.UserId)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            await HandleAbortedBooking(eventBooking);
        }

        private static string CreateSecret()
        {
            const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$_-";
            return new string(Enumerable.Repeat(allowedChars, 200).Select(s => s[new Random().Next(s.Length)]).ToArray());
        }

        public async Task<EventBookingSummaryResponse> BookingConfirmation(string eventBookingId)
        {
            var user = await _userManager.FindByIdAsync(_userDetailsProvider.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var booking = await _eventBookingRepository.GetEventBooking(_userDetailsProvider.UserId, eventBookingId);
            if (booking == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            return _mapper.Map<EventBookingSummaryResponse>(booking);
        }

        public async Task<bool> SetSharedBookingReminder(BookingReminderRequest request)
        {
            var user = await _userManager.FindByIdAsync(_userDetailsProvider.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var eventBooking = await _eventBookingRepository.GetById(request.BookingId);
            if (eventBooking == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            if (eventBooking.Event.Occurrence.StartDate < UAEDateTime.Now.Date)
                throw new OutOutException(ErrorCodes.CantSetReminderForOldBooking);

            var sharedTickets = user.SharedTickets.Where(a => a.BookingId == request.BookingId).ToList();
            foreach (var ticket in sharedTickets)
            {
                ticket.Reminders = new List<ReminderType>();
                ticket.Reminders.AddRange(request.ReminderTypes);
                await _userManager.UpdateAsync(user);
            }

            var existingReminder = await _notificationRepository.GetByEventBookingId(eventBooking.Id, _userDetailsProvider.UserId);
            if (existingReminder.Any())
                await _notificationRepository.DeleteReminders(existingReminder);

            await SetReminder(eventBooking, request.ReminderTypes);

            return true;
        }

        public async Task<bool> SetBookingReminder(BookingReminderRequest request)
        {
            var user = await _userManager.FindByIdAsync(_userDetailsProvider.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var eventBooking = await _eventBookingRepository.GetById(request.BookingId);
            if (eventBooking == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            if (eventBooking.Event.Occurrence.StartDate < UAEDateTime.Now.Date)
                throw new OutOutException(ErrorCodes.CantSetReminderForOldBooking);

            if (eventBooking.User.Id != _userDetailsProvider.UserId)
                throw new OutOutException(ErrorCodes.CantSetReminder);

            eventBooking.Reminders = new List<ReminderType>();
            eventBooking.Reminders.AddRange(request.ReminderTypes);
            await _eventBookingRepository.UpdateEventBooking(eventBooking);

            var existingReminder = await _notificationRepository.GetByEventBookingId(eventBooking.Id, _userDetailsProvider.UserId);
            if (existingReminder.Any())
                await _notificationRepository.DeleteReminders(existingReminder);

            await SetReminder(eventBooking, request.ReminderTypes);

            return true;
        }

        public async Task SetReminder(EventBooking eventBooking, List<ReminderType> reminderTypes)
        {
            foreach (var type in reminderTypes)
            {
                switch (type)
                {
                    case ReminderType.OneDayBefore:
                        var oneDayReminder = new Notification(NotificationType.Reminder,
                                                              _userDetailsProvider.UserId,
                                                              eventBooking.Event.Name,
                                                              $"You have a booking in 24 hours at {eventBooking.Event.Name}", "event.png",
                                                              NotificationAction.EventBookingReminder,
                                                              eventBooking: _mapper.Map<EventBookingSummary>(eventBooking),
                                                              affectedId: eventBooking.Id);
                        oneDayReminder.ToBeSentDate = eventBooking.Event.Occurrence.GetStartDateTime().AddHours(-24);
                        await _notificationRepository.Create(oneDayReminder);
                        break;

                    case ReminderType.SixHoursBefore:
                        var sixHoursReminder = new Notification(NotificationType.Reminder,
                                                                _userDetailsProvider.UserId,
                                                                eventBooking.Event.Name,
                                                                $"You have a booking in 6 hours at {eventBooking.Event.Name}", "event.png",
                                                                NotificationAction.EventBookingReminder,
                                                                eventBooking: _mapper.Map<EventBookingSummary>(eventBooking),
                                                                affectedId: eventBooking.Id);


                        sixHoursReminder.ToBeSentDate = eventBooking.Event.Occurrence.GetStartDateTime().AddHours(-6);
                        await _notificationRepository.Create(sixHoursReminder);
                        break;
                }
            }
        }

        public async Task<Page<TicketDetails>> GetTicketsRedeemedByMe(PaginationRequest pageRequest, TicketFilterationRequest request)
        {
            await _userDetailsProvider.ReInitialize();

            var result = await _eventBookingRepository.GetTicketsRedeemedByUser(pageRequest, request, _userDetailsProvider.UserId);
            var mappedList = new List<TicketDetails>();
            foreach (var r in result.Records)
            {
                var ticketOwner = r.User;
                var redeemedUser = await _userRepository.GetUserById(r.Ticket.RedeemedBy ?? r.Ticket.TicketHolder);
                var tempTicket = _mapper.Map<TicketDetails>(r);

                if (redeemedUser != null)
                {
                    tempTicket.Email = redeemedUser.Email;
                    tempTicket.UserName = redeemedUser.FullName;
                    tempTicket.PhoneNumber = redeemedUser.PhoneNumber;
                    tempTicket.Gender = redeemedUser.Gender;
                }
                tempTicket.IsTicketShared = redeemedUser?.Id != ticketOwner.Id;

                if (tempTicket.IsTicketShared)
                {
                    tempTicket.TicketOwnerEmail = ticketOwner.Email;
                    tempTicket.TicketOwnerUserName = ticketOwner.FullName;
                    tempTicket.TicketOwnerPhoneNumber = ticketOwner.PhoneNumber;
                    tempTicket.TicketOwnerGender = ticketOwner.Gender;
                }


                mappedList.Add(tempTicket);
            }
            return new Page<TicketDetails>(mappedList, result.PageNumber, result.PageSize, result.RecordsTotalCount);
        }

        public async Task<TicketDetails> GetTicketDetails(TicketStatusRequest request)
        {
            await _userDetailsProvider.ReInitialize();

            if (!ObjectId.TryParse(request.TicketId, out _))
                throw new OutOutException(ErrorCodes.InvalidTicket);

            var booking = await _eventBookingRepository.GetEventBookingByTicketId(request.TicketId);
            if (booking == null)
                throw new OutOutException(ErrorCodes.TicketDoesNotExist);

            if (!_userDetailsProvider.IsSuperAdmin
                && !_userDetailsProvider.HasAccessToEvent(booking.Event?.Id)
                && !_userDetailsProvider.HasAccessToVenue(booking.Venue?.Id))
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisEvent);

            var user = await _userRepository.GetUserById(request.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var ticket = await _eventBookingRepository.GetTicketDetails(request.TicketId);
            if (ticket == null)
                throw new OutOutException(ErrorCodes.TicketNotFound);

            var eventOccurrence = _eventRepository.GetSingleEventOccurrenceById(booking.Event.Occurrence.Id);
            if (eventOccurrence == null)
                throw new OutOutException(ErrorCodes.EventOccurrenceNotFound);

            if (request.TicketSecret != ticket.Ticket.Secret)
                throw new OutOutException(ErrorCodes.InvalidTicket);

            var result = _mapper.Map<TicketDetails>(ticket);

            result.Email = user.Email;
            result.UserName = user.FullName;
            result.Gender = user.Gender;
            result.PhoneNumber = user.PhoneNumber;
            if (user.Id != booking.User.Id)
            {
                result.TicketOwnerEmail = booking.User.Email;
                result.TicketOwnerUserName = booking.User.FullName;
                result.TicketOwnerGender = booking.User.Gender;
                result.TicketOwnerPhoneNumber = booking.User.PhoneNumber;

                result.IsTicketShared = true;
            }

            result.TotalTicketsCount = booking.Tickets.Count;
            result.RedeemedTicketsCount = booking.Tickets.Where(t => t.Status != TicketStatus.Rejected && t.Status != TicketStatus.Ready && t.RedeemedBy != null).Count();
            return result;
        }

        public async Task<bool> QrRedeemTicket(QrTicketRedemptionRequest request)
        {
            await _userDetailsProvider.ReInitialize();

            if (!ObjectId.TryParse(request.TicketId, out _))
                throw new OutOutException(ErrorCodes.InvalidTicket);

            var booking = await _eventBookingRepository.GetEventBookingByTicketId(request.TicketId);
            if (booking == null)
                throw new OutOutException(ErrorCodes.TicketDoesNotExist);

            if (!_userDetailsProvider.IsSuperAdmin
                && !_userDetailsProvider.HasAccessToEvent(booking.Event?.Id)
                && !_userDetailsProvider.HasAccessToVenue(booking.Venue?.Id))
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisEvent);

            var user = await _userRepository.GetUserById(request.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var eventOccurrence = _eventRepository.GetSingleEventOccurrenceById(booking.Event.Occurrence.Id);
            if (eventOccurrence == null)
                throw new OutOutException(ErrorCodes.EventOccurrenceNotFound);

            var ticket = booking.Tickets.Select(a => a).Where(a => a.Id == request.TicketId).FirstOrDefault();
            if (!string.IsNullOrEmpty(ticket.RedeemedBy) || ticket.RedemptionDate != null)
                throw new OutOutException(ErrorCodes.TicketUsedBefore);

            if (request.TicketSecret != ticket.Secret)
                throw new OutOutException(ErrorCodes.InvalidTicket);

            if (ticket.Status == TicketStatus.Rejected)
                throw new OutOutException(ErrorCodes.RejectedTicket, $"Ticket has been rejected before for the following reason: {ticket.RejectionReason}");

            booking.LastModifiedDate = UAEDateTime.Now;
            ticket.RedemptionDate = UAEDateTime.Now;
            ticket.RedeemedBy = request.UserId;
            ticket.QrRedeemedBy = _userDetailsProvider.UserId;
            ticket.Status = TicketStatus.Approved;
            ticket.TicketHolder = request.UserId;

            await _eventBookingRepository.Update(booking);

            return true;
        }

        public async Task<bool> RejectTicket(TicketRejectionRequest request)
        {
            await _userDetailsProvider.ReInitialize();

            if (!ObjectId.TryParse(request.TicketId, out _))
                throw new OutOutException(ErrorCodes.InvalidTicket);

            var booking = await _eventBookingRepository.GetEventBookingByTicketId(request.TicketId);
            if (booking == null)
                throw new OutOutException(ErrorCodes.BookingNotFound);

            if (!_userDetailsProvider.IsSuperAdmin
                && !_userDetailsProvider.HasAccessToEvent(booking.Event?.Id)
                && !_userDetailsProvider.HasAccessToVenue(booking.Venue?.Id))
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisEvent);

            var user = await _userRepository.GetUserById(request.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.UserNotFound);

            var eventOccurrence = _eventRepository.GetSingleEventOccurrenceById(booking.Event.Occurrence.Id);
            if (eventOccurrence == null)
                throw new OutOutException(ErrorCodes.EventOccurrenceNotFound);

            var ticket = booking.Tickets.Select(a => a).Where(a => a.Id == request.TicketId).FirstOrDefault();
            if (!string.IsNullOrEmpty(ticket.RedeemedBy) || ticket.RedemptionDate != null)
                throw new OutOutException(ErrorCodes.TicketUsedBefore);

            if (request.TicketSecret != ticket.Secret)
                throw new OutOutException(ErrorCodes.InvalidTicket);

            if (ticket.Status == TicketStatus.Rejected)
                throw new OutOutException(ErrorCodes.RejectedTicket, $"Ticket has been rejected before for the following reason: {ticket.RejectionReason}");

            booking.LastModifiedDate = UAEDateTime.Now;
            ticket.RedemptionDate = UAEDateTime.Now;
            ticket.RedeemedBy = null;
            ticket.QrRedeemedBy = null;
            ticket.Status = TicketStatus.Rejected;
            ticket.RejectionReason = request.RejectionReason;
            ticket.RejectedBy = _userDetailsProvider.UserId;
            ticket.TicketHolder = request.UserId;

            await _eventBookingRepository.Update(booking);

            return true;
        }

        public async Task<bool> RedeemTicket(TicketRedemptionRequest request)
        {
            var booking = await _eventBookingRepository.GetEventBookingByTicketId(request.TicketId);
            if (booking == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            var eventOccurrence = _eventRepository.GetSingleEventOccurrenceById(booking.Event.Occurrence.Id);
            if (eventOccurrence == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            var ticket = booking.Tickets.Select(a => a).Where(a => a.Id == request.TicketId).FirstOrDefault();
            if (!string.IsNullOrEmpty(ticket.RedeemedBy) || ticket.RedemptionDate != null)
                throw new OutOutException(ErrorCodes.TicketUsedBefore);

            if (request.TicketSecret != ticket.Secret)
                throw new OutOutException(ErrorCodes.InvalidSecret);

            if (request.EventCode != eventOccurrence.Code)
                throw new OutOutException(ErrorCodes.InvalidCode);

            if (ticket.Status == TicketStatus.Rejected)
                throw new OutOutException(ErrorCodes.RejectedTicket, $"Ticket has been rejected before for the following reason: {ticket.RejectionReason}");

            booking.LastModifiedDate = UAEDateTime.Now;
            ticket.RedemptionDate = UAEDateTime.Now;
            ticket.RedeemedBy = _userDetailsProvider.UserId;
            ticket.Status = TicketStatus.Approved;
            ticket.TicketHolder = _userDetailsProvider.UserId;

            await _eventBookingRepository.Update(booking);

            return true;
        }

        public async Task<bool> IsTicketShareable(ShareTicketRequest request)
        {
            var user = await _userManager.FindByIdAsync(_userDetailsProvider.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var eventBooking = await _eventBookingRepository.GetEventBookingByTicket(request.TicketId, request.TicketSecret);
            if (eventBooking == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            var ticket = eventBooking.Tickets.Where(a => a.Id == request.TicketId && a.Secret == request.TicketSecret).FirstOrDefault();
            if (ticket.RedemptionDate != null || !string.IsNullOrEmpty(ticket.RedeemedBy))
                throw new OutOutException(ErrorCodes.CantShareRedeemedTicket);

            return true;
        }

        public async Task<bool> AddToSharedTickets(ShareTicketRequest request)
        {
            await _userDetailsProvider.ReInitialize();
            var user = await _userManager.FindByIdAsync(_userDetailsProvider.UserId);
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var eventBooking = await _eventBookingRepository.GetEventBookingByTicket(request.TicketId, request.TicketSecret);
            if (eventBooking == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            if (eventBooking.User.Id == user.Id)
                return true;

            var existingSharedTicket = user.SharedTickets.Where(a => a.TicketSecret == request.TicketSecret && a.TicketId == request.TicketId).FirstOrDefault();
            if (existingSharedTicket != null)
                return true;

            var receivedTicket = new SharedTicket(eventBooking.User.Id, eventBooking.Id, request.TicketId, request.TicketSecret);
            var updateResult = await _userRepository.AddToSharedTickets(user.Id, receivedTicket);
            if (!updateResult)
            {
                _logger.LogError($"Failed to add shared ticket {receivedTicket.TicketId} to user {user.Id} shared tickets");
                throw new OutOutException(ErrorCodes.CouldNotReceiveSharedTicket);
            }

            var bookingOwner = await _userRepository.GetUserById(eventBooking.User.Id);
            var notification = new Notification(NotificationType.Notification,
                                                eventBooking.User.Id,
                                                "Ticket transferred",
                                                $"You have transferred a ticket to {user.FullName}",
                                                "event.png",
                                                NotificationAction.TicketTransfer,
                                                eventBooking: _mapper.Map<EventBookingSummary>(eventBooking));
            await _notificationRepository.Create(notification);
            await _notificationComposerService.SendNotification(notification, bookingOwner, NotificationAction.TicketTransfer, request.TicketId);

            return true;
        }
    }
}
