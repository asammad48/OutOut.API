using AutoMapper;
using OutOut.Constants;
using OutOut.Constants.Enums;
using OutOut.Constants.Errors;
using OutOut.Core.Utils;
using OutOut.Infrastructure.Services;
using OutOut.Models.Exceptions;
using OutOut.Models.Models;
using OutOut.Models.Models.Embedded;
using OutOut.Models.Utils;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Providers;
using OutOut.ViewModels.Requests.Reminders;
using OutOut.ViewModels.Requests.VenueBooking;
using OutOut.ViewModels.Responses.VenueBooking;
using OutOut.ViewModels.Responses.Venues;
using System.Net;

namespace OutOut.Core.Services
{
    public class VenueBookingService
    {
        private readonly IVenueRepository _venueRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IUserDetailsProvider _userDetailsProvider;
        private readonly IVenueBookingRepository _venueBookingRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly NotificationComposerService _notificationComposerService;
        private readonly VenueService _venueService;
        private readonly EmailComposerService _emailComposerService;
        private const string KEY_VenueBookingNumber = "Venue Booking Number";

        public VenueBookingService(IVenueRepository venueRepository,
                                   IMapper mapper,
                                   IUserDetailsProvider userDetailsProvider,
                                   IVenueBookingRepository venueBookingRepository,
                                   INotificationRepository notificationRepository,
                                   NotificationComposerService notificationComposerService,
                                   VenueService venueService,
                                   IUserRepository userRepository,
                                   EmailComposerService emailComposerService)
        {
            _venueRepository = venueRepository;
            _mapper = mapper;
            _userDetailsProvider = userDetailsProvider;
            _venueBookingRepository = venueBookingRepository;
            _notificationRepository = notificationRepository;
            _notificationComposerService = notificationComposerService;
            _venueService = venueService;
            _userRepository = userRepository;
            _emailComposerService = emailComposerService;
        }

        public async Task<VenueResponse> GetBookingDetails(string bookingId)
        {
            var booking = await _venueBookingRepository.GetById(bookingId);
            if (booking == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            return await _venueService.GetVenueDetails(booking.Venue.Id, booking);
        }

        public async Task<VenueBookingResponse> MakeABooking(VenueBookingRequest request)
        {
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var venue = await _venueRepository.GetById(request.VenueId);
            if (venue == null)
                throw new OutOutException(ErrorCodes.VenueNotFound);

            if (venue.Status != Availability.Active)
                throw new OutOutException(ErrorCodes.UnavailableVenue);

            if (!venue.OpenTimes.IsInRangeOf(request.Date, afterThisDate: UAEDateTime.Now))
                throw new OutOutException(ErrorCodes.InvalidBookingDate);

            var booking = _mapper.Map<VenueBooking>(request);
            booking.BookingNumber = await _venueBookingRepository.GenerateLastIncrementalNumber(KEY_VenueBookingNumber);

            booking.Venue = _mapper.Map<VenueSummary>(venue);
            booking.User = _mapper.Map<ApplicationUserSummary>(user);
            booking.CreatedDate = UAEDateTime.Now;

            var bookingResult = await _venueBookingRepository.Create(booking);

            await _notificationComposerService.SendSignalRNotification(NotificationAction.NewVenueBooking,
                                                                         $"Booking has been made for “{venue.Name}”",
                                                                         bookingResult.Id,
                                                                         Roles.VenueAdmin,
                                                                         accessibleVenue: venue.Id);

            return _mapper.Map<VenueBookingResponse>(bookingResult);
        }

        public async Task<bool> CancelABooking(string bookingId)
        {
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var booking = await _venueBookingRepository.GetById(bookingId);
            if (booking == null)
                throw new OutOutException(ErrorCodes.BookingNotFound);

            if (booking.User.Id != _userDetailsProvider.UserId)
                throw new OutOutException(ErrorCodes.InvalidUser);

            if (UAEDateTime.Now > booking.Date)
                throw new OutOutException(ErrorCodes.CantCancelAfterBookingDate);

            var deleteRemindersAttempt = await SetBookingReminder(new BookingReminderRequest
            {
                BookingId = bookingId,
                ReminderTypes = new List<ReminderType>()
            });

            if (!deleteRemindersAttempt)
                throw new OutOutException(ErrorCodes.CantDeleteRemindersForThisBooking);

            booking.Status = VenueBookingStatus.Cancelled;
            await _venueBookingRepository.UpdateVenueBooking(bookingId, booking);


            var x = _userDetailsProvider.GetVenueAdminsByVenueId(booking.Venue.Id);
            var notificationTasks = new List<Task>();
            Parallel.ForEach(_userDetailsProvider.GetVenueAdminsByVenueId(booking.Venue.Id), venueAdmin =>
            {
                var notification = new Notification(NotificationType.Notification,
                                                     NotificationAction.VenueBookingCancellation,
                                                     venueAdmin.Id,
                                                     "Booking Cancellation",
                                                     $"Booking for “{booking.Venue.Name}“ has been cancelled by “{user.FullName}“ ",
                                                     "venue.png",
                                                     booking.Venue.Id);

                _notificationRepository.Create(notification);


                string htmlEmailBody = $"<p>Hi {venueAdmin.FullName.Split(" ")[0]},</p> " +
                "<p><br></p>" +
                $"<p>Kindly note that booking for user \"{user.FullName}\" in venue \"{booking.Venue.Name}\" has been cancelled</p>";

                _emailComposerService.SendCustomMail(venueAdmin.Email, venueAdmin.FullName, "Booking cancellation", htmlEmailBody);



            });
            await _notificationComposerService.SendSignalRNotification(NotificationAction.NewVenueBooking,
                                                                            $"Booking for “{booking.Venue.Name}“ has been cancelled by “{user.FullName}“ ",
                                                                             bookingId,
                                                                             Roles.VenueAdmin,
                                                                             accessibleVenue: booking.Venue.Id);

            return true;
        }

        public async Task<bool> SetBookingReminder(BookingReminderRequest request)
        {
            request.ReminderTypes = request.ReminderTypes.ToHashSet().ToList();

            var venueBooking = await _venueBookingRepository.GetById(request.BookingId);
            if (venueBooking == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            if (venueBooking.Date < UAEDateTime.Now)
                throw new OutOutException(ErrorCodes.CantSetReminderForOldBooking);

            venueBooking.Reminders = new List<ReminderType>();
            venueBooking.Reminders.AddRange(request.ReminderTypes);
            await _venueBookingRepository.UpdateVenueBooking(venueBooking.Id, venueBooking);

            var existingReminder = await _notificationRepository.GetByVenueBookingId(venueBooking.Id);
            if (existingReminder.Any())
                await _notificationRepository.DeleteReminders(existingReminder);

            foreach (var type in request.ReminderTypes)
            {
                switch (type)
                {
                    case ReminderType.OneDayBefore:
                        var oneDayReminder = new Notification(NotificationType.Reminder,
                                                                NotificationAction.VenueBookingReminder,
                                                              _userDetailsProvider.UserId,
                                                              venueBooking.Venue.Name,
                                                              $"You have a booking in 24 hours at {venueBooking.Venue.Name}",
                                                              "venue.png",
                                                              venueBooking.Id
                                                              );
                        oneDayReminder.ToBeSentDate = venueBooking.Date.AddDays(-1);
                        await _notificationRepository.Create(oneDayReminder);
                        break;

                    case ReminderType.SixHoursBefore:
                        var sixHoursReminder = new Notification(NotificationType.Reminder,
                                                                NotificationAction.VenueBookingReminder,
                                                              _userDetailsProvider.UserId,
                                                              venueBooking.Venue.Name,
                                                                $"You have a booking in 6 hours at {venueBooking.Venue.Name}",
                                                                "venue.png",
                                                                venueBooking.Id);
                        sixHoursReminder.ToBeSentDate = venueBooking.Date.AddHours(-6);
                        await _notificationRepository.Create(sixHoursReminder);
                        break;
                }
            }

            return true;
        }

        public async Task<bool> ApproveBooking(string bookingId)
        {
            var booking = await _venueBookingRepository.GetById(bookingId);
            if (booking == null)
                throw new OutOutException(ErrorCodes.BookingNotFound);

            await _userDetailsProvider.ReInitialize();

            if (!_userDetailsProvider.HasAccessToVenue(booking.Venue.Id) && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisVenue, HttpStatusCode.Forbidden);

            var isUpdated = await _venueBookingRepository.ApproveBooking(bookingId);
            if (isUpdated)
            {
                var bookingOwner = await _userRepository.GetUserById(booking.User.Id);
                if (bookingOwner == null)
                    throw new OutOutException(ErrorCodes.UserNotFound);

                var venue = await _venueRepository.GetById(booking.Venue.Id);
                if (venue == null)
                    throw new OutOutException(ErrorCodes.VenueNotFound);

                var notification = new Notification(NotificationType.Notification,
                                                    NotificationAction.VenueBookingConfirmation, bookingOwner.Id,
                                                    "Booking Confirmation",
                                                    $"Your “{venue.Name}“ booking is now confirmed, waiting for you",
                                                    "venue.png",
                                                    venue.Id);
                await _notificationRepository.Create(notification);
                _ = Task.Run(() => _notificationComposerService.SendNotification(notification, bookingOwner, NotificationAction.VenueBookingConfirmation, bookingId));


                string htmlEmailBodyForUser = $"<p>Hi {bookingOwner.FullName.Split(" ")[0]},</p> " +
                   $"<p>Kindly note that your booking in venue \"{booking.Venue.Name}\" has been approved</p>";

                await _emailComposerService.SendCustomMail(bookingOwner.Email, bookingOwner.FullName, "Venue Booking Approval", htmlEmailBodyForUser);


                var bookingDetails = await _venueBookingRepository.GetById(booking.Id);

                var userTakeApproval = await _userRepository.GetUserById(bookingDetails.ModifiedBy);

                string htmlEmailBodyForAdmin = $"<p>Hi {userTakeApproval.FullName.Split(" ")[0]},</p> " +
                $"<p>Kindly note that you approved venue booking in venue \"{booking.Venue.Name}\" with order number {bookingDetails.BookingNumber}</p>";

                await _emailComposerService.SendCustomMail(userTakeApproval.Email, userTakeApproval.FullName, "Venue Booking Approval", htmlEmailBodyForAdmin);


            }

            return isUpdated;
        }

        public async Task<bool> RejectBooking(string bookingId)
        {
            var booking = await _venueBookingRepository.GetById(bookingId);
            if (booking == null)
                throw new OutOutException(ErrorCodes.BookingNotFound);

            await _userDetailsProvider.ReInitialize();

            if (!_userDetailsProvider.HasAccessToVenue(booking.Venue.Id) && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisVenue, HttpStatusCode.Forbidden);

            var isUpdated = await _venueBookingRepository.RejectBooking(bookingId);
            if (isUpdated)
            {
                var bookingOwner = await _userRepository.GetUserById(booking.User.Id);
                if (bookingOwner == null)
                    throw new OutOutException(ErrorCodes.UserNotFound);

                var venue = await _venueRepository.GetById(booking.Venue.Id);
                if (venue == null)
                    throw new OutOutException(ErrorCodes.VenueNotFound);

                var notification = new Notification(NotificationType.Notification, bookingOwner.Id,
                                                    "Booking Rejection",
                                                    $"Unfortunately “{venue.Name}“ booking is rejected, you can try booking again at another time",
                                                    "venue.png", NotificationAction.VenueBookingRejection);
                await _notificationRepository.Create(notification);
                _ = Task.Run(() => _notificationComposerService.SendNotification(notification, bookingOwner, NotificationAction.VenueBookingRejection, bookingId));

                await _notificationRepository.DeleteRemindersForRejectedBooking(bookingId, bookingOwner.Id);
            }

            return isUpdated;
        }
    }
}
