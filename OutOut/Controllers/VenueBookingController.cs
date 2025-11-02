using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOut.Constants;
using OutOut.Core.Services;
using OutOut.ViewModels.Requests.Reminders;
using OutOut.ViewModels.Requests.VenueBooking;
using OutOut.ViewModels.Responses.VenueBooking;
using OutOut.ViewModels.Responses.Venues;
using OutOut.ViewModels.Validators;
using OutOut.ViewModels.Wrappers;
using System.ComponentModel.DataAnnotations;

namespace OutOut.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class VenueBookingController : ControllerBase
    {
        private readonly VenueBookingService _venueBookingService;

        public VenueBookingController(VenueBookingService venueBookingService)
        {
            _venueBookingService = venueBookingService;
        }

        [Produces(typeof(OperationResult<VenueResponse>))]
        [HttpGet]
        public async Task<IActionResult> GetBookingDetails([MongoId][Required] string bookingId)
        {
            var result = await _venueBookingService.GetBookingDetails(bookingId);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<VenueBookingResponse>))]
        [HttpPost]
        public async Task<IActionResult> MakeABooking(VenueBookingRequest request)
        {
            var result = await _venueBookingService.MakeABooking(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        public async Task<IActionResult> CancelABooking([MongoId][Required] string bookingId)
        {
            var result = await _venueBookingService.CancelABooking(bookingId);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        public async Task<IActionResult> SetBookingReminder(BookingReminderRequest request)
        {
            var result = await _venueBookingService.SetBookingReminder(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> ApproveBooking([MongoId][Required] string bookingId)
        {
            var result = await _venueBookingService.ApproveBooking(bookingId);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> RejectBooking([MongoId][Required] string bookingId)
        {
            var result = await _venueBookingService.RejectBooking(bookingId);
            return Ok(SuccessHelper.Wrap(result));
        }
    }
}
