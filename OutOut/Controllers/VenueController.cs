using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOut.Constants;
using OutOut.Core.Services;
using OutOut.Models.Wrappers;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Requests.VenueBooking;
using OutOut.ViewModels.Requests.Venues;
using OutOut.ViewModels.Responses.Events;
using OutOut.ViewModels.Responses.Offers;
using OutOut.ViewModels.Responses.TermsAndConditions;
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
    public class VenueController : ControllerBase
    {
        private readonly VenueService _venueService;

        public VenueController(VenueService venueService)
        {
            _venueService = venueService;
        }

        [Produces(typeof(OperationResult<Page<VenueSummaryResponse>>))]
        [HttpPost]
        public async Task<IActionResult> GetVenues([FromQuery] PaginationRequest paginationRequest, [FromBody] VenueFilterationRequest filterRequest)
        {
            var result = await _venueService.GetVenues(paginationRequest, filterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<List<VenueSummaryWithBookingResponse>>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetNewestVenues()
        {
            var result = await _venueService.GetNewestVenues();
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<List<VenueSummaryResponse>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetAllVenues(SearchFilterationRequest searchFilterationRequest)
        {
            var result = await _venueService.GetAllVenues(searchFilterationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<List<VenueSummaryResponse>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetActiveAccessibleVenues(SearchFilterationRequest searchFilterationRequest)
        {
            var result = await _venueService.GetActiveAccessibleVenues(searchFilterationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<List<VenueMiniSummaryResponse>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> GetActiveVenues(SearchFilterationRequest searchFilterationRequest)
        {
            var result = await _venueService.GetActiveVenues(searchFilterationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<VenueSummaryWithBookingResponse>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetVenuesPage([FromQuery] PaginationRequest paginationRequest, [FromBody] FilterationRequest filterRequest)
        {
            var result = await _venueService.GetVenuesPage(paginationRequest, filterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<FullVenueResponse>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetVenueDetailsForAdmin([MongoId] string id)
        {
            var result = await _venueService.GetVenueDetailsForAdmin(id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<OfferWithUsageResponse>>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetOffersPageInVenue([MongoId][FromQuery] string id, [FromQuery] PaginationRequest paginationRequest)
        {
            var result = await _venueService.GetOffersPageInVenue(id, paginationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<EventSummaryResponse>>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetUpcomingEventsPageInVenue([MongoId][FromQuery] string id, [FromQuery] PaginationRequest paginationRequest)
        {
            var result = await _venueService.GetUpcomingEventsPageInVenue(id, paginationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpGet]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> ApproveVenue([MongoId][Required] string requestId)
        {
            var result = await _venueService.ApproveVenue(requestId);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpGet]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> RejectVenue([MongoId][Required] string id)
        {
            var result = await _venueService.RejectVenue(id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> UpdateVenueCode([MongoId][Required] string id, [ValidCode] string code)
        {
            var result = await _venueService.UpdateVenueCode(id, code);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<VenueResponse>))]
        [HttpGet]
        public async Task<IActionResult> GetVenueDetails([MongoId][Required] string venueId)
        {
            var result = await _venueService.GetVenueDetails(venueId);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<List<TermsAndConditionsResponse>>))]
        [HttpPost]
        public async Task<IActionResult> UpdateVenueTermsAndConditions([MongoId][Required][FromQuery] string venueId, [FromBody] List<string> selectedTermsAndConditions)
        {
            var result = await _venueService.UpdateVenueTermsAndConditions(venueId, selectedTermsAndConditions);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<List<TermsAndConditionsResponse>>))]
        [HttpGet]
        public async Task<IActionResult> GetVenueTermsAndConditions([MongoId][Required] string venueId)
        {
            var result = await _venueService.GetVenueTermsAndConditions(venueId);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        public async Task<IActionResult> FavoriteVenue([MongoId][Required] string venueId)
        {
            var result = await _venueService.FavoriteVenue(venueId);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        public async Task<IActionResult> UnfavoriteVenue([MongoId][Required] string venueId)
        {
            var result = await _venueService.UnfavoriteVenue(venueId);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<FullVenueResponse>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> CreateVenue([FromForm] CreateVenueRequest request)
        {
            var result = await _venueService.CreateVenue(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<FullVenueResponse>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> UpdateVenue([MongoId][FromQuery][Required] string id, [FromForm] UpdateVenueRequest request)
        {
            var result = await _venueService.UpdateVenue(id, request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> DeleteGalleryImages([MongoId][Required][FromQuery] string id, [FromBody][Required] List<string> images)
        {
            var result = await _venueService.DeleteGalleryImages(id, images);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> DeleteVenue([MongoId][Required] string id)
        {
            var result = await _venueService.DeleteVenue(id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<FullVenueResponse>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetVenue([MongoId][Required] string id)
        {
            var result = await _venueService.GetVenue(id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<VenueMiniSummaryResponse>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetVenueById([MongoId][Required] string id)
        {
            var result = await _venueService.GetVenueById(id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<List<VenueMiniSummaryResponse>>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetActiveVenuesWithNoLoyalty()
        {
            var result = await _venueService.GetActiveVenuesWithNoLoyalty();
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<List<VenueMiniSummaryResponse>>))]
        [HttpGet]
        public async Task<IActionResult> GetActiveVenuesWithNoLoyaltyToAllAdmins()
        {
            var result = await _venueService.GetActiveVenuesWithNoLoyaltyToAllAdmins();
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<VenueReportResponse>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetVenueReport([MongoId][Required] string id)
        {
            var result = await _venueService.GetVenueReport(id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<OfferReportResponse>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetOffersOverviewForVenueReport([MongoId][Required] string id, [FromQuery] PaginationRequest paginationRequest)
        {
            var result = await _venueService.GetOffersOverviewForVenueReport(id, paginationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> ExportOffersOverviewForVenueReportToExcel([Required][FromQuery][MongoId] string id)
        {
            var result = await _venueService.ExportOffersOverviewForVenueReportToExcel(id);
            return File(result.File, "application/octet-stream", result.FileName);
        }

        [Produces(typeof(OperationResult<Page<VenueBookingDetailedReportResponse>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetVenueBookingDetailsReport([MongoId][Required] string id, [FromQuery] PaginationRequest paginationRequest, VenueBookingReportFilterRequest filterRequest)
        {
            var result = await _venueService.GetVenueBookingDetailsReport(id, paginationRequest, filterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> ExportAllVenueBookingsDetailsReportToExcel([Required][FromQuery][MongoId] string venueId)
        {
            var result = await _venueService.ExportAllVenueBookingsDetailsReportToExcel(venueId);
            return File(result.File, "application/octet-stream", result.FileName);
        }

        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> ExportSelectedVenueBookingDetailsReportToExcel([Required][FromQuery][MongoId] string venueId, [FromQuery] string bookingId)
        {
            var result = await _venueService.ExportSelectedVenueBookingsDetailsReportToExcel(venueId, bookingId);
            return File(result.File, "application/octet-stream", result.FileName);
        }
    }
}
