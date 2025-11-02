using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOut.Constants;
using OutOut.Core.Services;
using OutOut.Models.Wrappers;
using OutOut.ViewModels.Requests.EventBooking;
using OutOut.ViewModels.Requests.EventBookings;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Requests.Reminders;
using OutOut.ViewModels.Requests.Ticket;
using OutOut.ViewModels.Responses.EventBookings;
using OutOut.ViewModels.Responses.Events;
using OutOut.ViewModels.Validators;
using OutOut.ViewModels.Wrappers;
using System.ComponentModel.DataAnnotations;

namespace OutOut.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class EventBookingController : ControllerBase
    {
        private readonly EventBookingService _eventBookingService;

        public EventBookingController(EventBookingService eventBookingService)
        {
            _eventBookingService = eventBookingService;
        }

        [Produces(typeof(OperationResult<SingleEventOccurrenceResponse>))]
        [HttpGet]
        public async Task<IActionResult> GetBookingDetails([FromQuery][MongoId] string eventBookingId)
        {
            var result = await _eventBookingService.GetBookingDetails(eventBookingId);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<SingleEventOccurrenceResponse>))]
        [HttpGet]
        public async Task<IActionResult> GetSharedTicketDetails([FromQuery][MongoId] string ticketId)
        {
            var result = await _eventBookingService.GetSharedTicketDetails(ticketId);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<EventBookingSummaryResponse>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> GetCustomersOrdersForEvent([FromQuery][MongoId][Required] string id, [FromQuery] PaginationRequest paginationRequest, [FromBody] FilterationRequest filterRequest)
        {
            var result = await _eventBookingService.GetCustomersOrdersForEvent(id, paginationRequest, filterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<TelrBookingResponse>))]
        [HttpPost]
        public async Task<IActionResult> MakeATelrBooking(EventBookingRequest eventBookingRequest)
        {
            var result = await _eventBookingService.MakeATelrBooking(eventBookingRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<EventBookingSummaryResponse>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> RejectBooking([MongoId][Required] string eventBookingId)
        {
            var result = await _eventBookingService.RejectBooking(eventBookingId);
            return Ok(SuccessHelper.Wrap(result));
        }

        [HttpPost]
        public async Task<IActionResult> AbortPayment([MongoId] string eventBookingId)
        {
            await _eventBookingService.AbortPayment(eventBookingId);
            return Ok();
        }

        [Produces(typeof(OperationResult<EventBookingSummaryResponse>))]
        [HttpGet]
        public async Task<IActionResult> BookingConfirmation(string eventBookingId)
        {
            var result = await _eventBookingService.BookingConfirmation(eventBookingId);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        public async Task<IActionResult> SetBookingReminder(BookingReminderRequest request)
        {
            var result = await _eventBookingService.SetBookingReminder(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        public async Task<IActionResult> SetSharedBookingReminder(BookingReminderRequest request)
        {
            var result = await _eventBookingService.SetSharedBookingReminder(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        public async Task<IActionResult> RedeemTicket(TicketRedemptionRequest request)
        {
            var result = await _eventBookingService.RedeemTicket(request);
            return Ok(SuccessHelper.Wrap(result));
        }
        
        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.TicketAdmin)]
        public async Task<IActionResult> RejectTicket(TicketRejectionRequest request)
        {
            var result = await _eventBookingService.RejectTicket(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.TicketAdmin)]
        public async Task<IActionResult> QrRedeemTicket(QrTicketRedemptionRequest request)
        {
            var result = await _eventBookingService.QrRedeemTicket(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<TicketDetails>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.TicketAdmin)]
        public async Task<IActionResult> GetTicketDetails(TicketStatusRequest request)
        {
            var result = await _eventBookingService.GetTicketDetails(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<TicketDetails>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.TicketAdmin)]
        public async Task<IActionResult> GetTicketsRedeemedByMe([FromQuery] PaginationRequest pageRequest, [FromBody] TicketFilterationRequest request)
        {
            var result = await _eventBookingService.GetTicketsRedeemedByMe(pageRequest, request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        public async Task<IActionResult> IsTicketShareable(ShareTicketRequest request)
        {
            var result = await _eventBookingService.IsTicketShareable(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        public async Task<IActionResult> AddToSharedTickets(ShareTicketRequest request)
        {
            var result = await _eventBookingService.AddToSharedTickets(request);
            return Ok(SuccessHelper.Wrap(result));
        }
    }
}
