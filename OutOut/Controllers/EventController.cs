using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOut.Constants;
using OutOut.Core.Services;
using OutOut.Models.Wrappers;
using OutOut.ViewModels.Requests.EventBooking;
using OutOut.ViewModels.Requests.Events;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Requests.Venues;
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
    public class EventController : ControllerBase
    {
        private readonly EventService _eventService;

        public EventController(EventService eventService)
        {
            _eventService = eventService;
        }

        [Produces(typeof(OperationResult<Page<EventSummaryResponse>>))]
        [HttpPost]
        public async Task<IActionResult> GetEvents([FromQuery] PaginationRequest paginationRequest, [FromBody] EventFilterationRequest filterRequest)
        {
            var result = await _eventService.GetEvents(paginationRequest, filterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<EventSummaryWithBookingResponse>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> GetEventsPage([FromQuery] PaginationRequest paginationRequest, [FromBody] FilterationRequest filterRequest)
        {
            var result = await _eventService.GetEventsPage(paginationRequest, filterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<EventSummaryWithBookingResponse>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin)]
        public async Task<IActionResult> GetFeaturedEventsPage([FromQuery] PaginationRequest paginationRequest, [FromBody] FilterationRequest filterRequest)
        {
            var result = await _eventService.GetFeaturedEventsPage(paginationRequest, filterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<FullEventResponse>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> GetEventDetailsForAdmin([Required][MongoId] string id)
        {
            var result = await _eventService.GetEventDetailsForAdmin(id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<FullEventResponse>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> CreateEvent([FromForm] UpsertEventRequest request)
        {
            var result = await _eventService.CreateEvent(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<FullEventResponse>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> UpdateEvent([MongoId][Required] string id, [FromForm] UpsertEventRequest request)
        {
            var result = await _eventService.UpdateEvent(id, request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> DeleteEvent([MongoId][Required] string id)
        {
            var result = await _eventService.DeleteEvent(id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<EventMiniSummaryResponse>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> GetEventById([MongoId][Required] string id)
        {
            var result = await _eventService.GetEventById(id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpGet]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> ApproveEvent([MongoId][Required] string id)
        {
            var result = await _eventService.ApproveEvent(id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpGet]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> RejectEvent([MongoId][Required] string id)
        {
            var result = await _eventService.RejectEvent(id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin)]
        public async Task<IActionResult> UpdateIsFeatured([Required][MongoId] string id, bool isFeatured)
        {
            var result = await _eventService.UpdateIsFeatured(id, isFeatured);
            return Ok(SuccessHelper.Wrap(result));
        } 
        
        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin)]
        public async Task<IActionResult> UnsetAllFeaturedEvents()
        {
            var result = await _eventService.UnsetAllFeaturedEvents();
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> UpdateEventCode([Required][MongoId] string id, [ValidCode] string code)
        {
            var result = await _eventService.UpdateEventCode(id, code);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<List<EventSummaryResponse>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> GetAllEvents(SearchFilterationRequest searchFilterationRequest)
        {
            var result = await _eventService.GetAllEvents(searchFilterationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }
        [Produces(typeof(OperationResult<List<EventSummaryResponse>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> GetAllEventsNotAssociatedToVenue([FromQuery] SearchFilterationRequest searchFilterationRequest, [FromQuery] string venueId)
        {
            var result = await _eventService.GetAllEventsNotAssociatedToVenue(venueId, searchFilterationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }
        [Produces(typeof(OperationResult<SingleEventOccurrenceResponse>))]
        [HttpGet]
        public async Task<IActionResult> GetEventDetails([FromQuery][MongoId] string eventOccurrenceId)
        {
            var result = await _eventService.GetEventDetails(eventOccurrenceId);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<List<EventSummaryWithBookingResponse>>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> GetUpcomingEvents()
        {
            var result = await _eventService.GetUpcomingEvents();
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<List<EventSummaryWithBookingResponse>>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> GetOngoingEvents()
        {
            var result = await _eventService.GetOngoingEvents();
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        public async Task<IActionResult> FavoriteEvent([MongoId] string eventOccurrenceId)
        {
            var result = await _eventService.FavoriteEventOccurrence(eventOccurrenceId);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        public async Task<IActionResult> UnfavoriteEvent([MongoId] string eventOccurrenceId)
        {
            var result = await _eventService.UnfavoriteEventOccurrence(eventOccurrenceId);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<EventReportResponse>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> GetEventReport([MongoId][Required] string eventId)
        {
            var result = await _eventService.GetEventReport(eventId);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<PackageOverviewReportResponse>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> GetPackagesOverviewReport([MongoId][Required][FromQuery] string eventId, [FromQuery] PaginationRequest paginationRequest, [FromBody] EventBookingReportFilterRequest filterRequest)
        {
            var result = await _eventService.GetPackagesOverviewReport(eventId, paginationRequest, filterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> ExportAllPackagesOverviewReportToExcel([Required][FromQuery][MongoId] string eventId)
        {
            var result = await _eventService.ExportAllPackagesOverviewReportToExcel(eventId);
            return File(result.File, "application/octet-stream", result.FileName);
        }

        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> ExportSelectedPackageOverviewReportToExcel([Required][FromQuery][MongoId] string eventId, [FromQuery] string packageId)
        {
            var result = await _eventService.ExportSelectedPackagesOverviewReportToExcel(eventId, packageId);
            return File(result.File, "application/octet-stream", result.FileName);
        }

        [Produces(typeof(OperationResult<Page<EventBookingDetailedReportResponse>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> GetEventBookingDetailsReport([MongoId][Required][FromQuery] string eventId, [FromQuery] PaginationRequest paginationRequest, EventBookingReportFilterRequest filterRequest)
        {
            var result = await _eventService.GetEventBookingDetailsReport(eventId, paginationRequest, filterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> ExportAllEventBookingsDetailsReportToExcel([Required][FromQuery][MongoId] string eventId)
        {
            var result = await _eventService.ExportAllEventBookingsDetailsReportToExcel(eventId);
            return File(result.File, "application/octet-stream", result.FileName);
        }

        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> ExportSelectedEventBookingDetailsReportToExcel([Required][FromQuery][MongoId] string eventId, [FromQuery] string bookingId)
        {
            var result = await _eventService.ExportSelectedEventBookingsDetailsReportToExcel(eventId, bookingId);
            return File(result.File, "application/octet-stream", result.FileName);
        }

    }
}
