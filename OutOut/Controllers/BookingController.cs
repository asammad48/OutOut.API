using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOut.Constants;
using OutOut.Core.Services;
using OutOut.Models.Wrappers;
using OutOut.ViewModels.Requests.Bookings;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Responses.Bookings;
using OutOut.ViewModels.Responses.EventBookings;
using OutOut.ViewModels.Responses.Events;
using OutOut.ViewModels.Responses.VenueBooking;
using OutOut.ViewModels.Validators;
using OutOut.ViewModels.Wrappers;
using System.ComponentModel.DataAnnotations;

namespace OutOut.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly BookingService _bookingService;

        public BookingController(BookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [Produces(typeof(OperationResult<Page<BookingResponse>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> GetAllBookingsPage([FromQuery] PaginationRequest paginationRequest, [FromBody] BookingFilterationRequest filterationRequest)
        {
            var result = await _bookingService.GetAllBookingsPage(paginationRequest, filterationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<VenueBookingResponse>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetVenueBookingsPage([FromQuery][Required][MongoId] string venueId, [FromQuery] PaginationRequest paginationRequest, [FromBody] FilterationRequest filterationRequest)
        {
            var result = await _bookingService.GetVenueBookingsPage(venueId, paginationRequest, filterationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> ExportAllVenueBookingsToExcel([Required][FromQuery][MongoId] string venueId)
        {
            var result = await _bookingService.ExportAllVenueBookingsToExcel(venueId);
            return File(result.File, "application/octet-stream", result.FileName);
        }

        [Produces(typeof(OperationResult<VenueBookingResponse>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetVenueBooking([FromQuery][Required][MongoId] string bookingId)
        {
            var result = await _bookingService.GetVenueBooking(bookingId);
            return Ok(SuccessHelper.Wrap(result));
        }

        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> ExportVenueBookingToExcel([Required][FromQuery][MongoId] string venueBookingId)
        {
            var result = await _bookingService.ExportVenueBookingToExcel(venueBookingId);
            return File(result.File, "application/octet-stream", result.FileName);
        }

        [Produces(typeof(OperationResult<Page<EventBookingSummaryResponse>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> GetEventBookingsPage([FromQuery][Required][MongoId] string eventId, [FromQuery] PaginationRequest paginationRequest, [FromBody] FilterationRequest filterationRequest)
        {
            var result = await _bookingService.GetEventBookingsPage(eventId, paginationRequest, filterationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> ExportAllEventBookingsToExcel([Required][FromQuery][MongoId] string eventId)
        {
            var result = await _bookingService.ExportAllEventBookingsToExcel(eventId);
            return File(result.File, "application/octet-stream", result.FileName);
        }

        [Produces(typeof(OperationResult<EventBookingSummaryResponse>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> GetEventBooking([FromQuery][Required][MongoId] string bookingId)
        {
            var result = await _bookingService.GetEventBooking(bookingId);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<TicketResponse>>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> GetTicketsByBookingId([FromQuery][Required][MongoId] string bookingId, [FromQuery] PaginationRequest paginationRequest)
        {
            var result = await _bookingService.GetTicketsByBookingId(bookingId, paginationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> ExportEventBookingToExcel([Required][FromQuery][MongoId] string eventBookingId)
        {
            var result = await _bookingService.ExportEventBookingToExcel(eventBookingId);
            return File(result.File, "application/octet-stream", result.FileName);
        }
    }
}
