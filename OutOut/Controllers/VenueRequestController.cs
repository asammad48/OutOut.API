using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOut.Constants;
using OutOut.Core.Services;
using OutOut.Models.Wrappers;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Responses.Events;
using OutOut.ViewModels.Responses.Offers;
using OutOut.ViewModels.Responses.TermsAndConditions;
using OutOut.ViewModels.Responses.VenueRequest;
using OutOut.ViewModels.Validators;
using OutOut.ViewModels.Wrappers;
using System.ComponentModel.DataAnnotations;

namespace OutOut.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class VenueRquestController : ControllerBase
    {
        private readonly VenueRequestService _venueRequestService;

        public VenueRquestController(VenueRequestService venueRequestService)
        {
            _venueRequestService = venueRequestService;
        }

        [Produces(typeof(OperationResult<Page<VenueRequestSummaryDTO>>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> GetVenuesAwaitingApproval([FromQuery] PaginationRequest paginationRequest, [FromBody] FilterationRequest filterRequest)
        {
            var result = await _venueRequestService.GetVenuesAwaitingApproval(paginationRequest, filterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }  
        
        [Produces(typeof(OperationResult<Page<VenueRequestSummaryDTO>>))]
        [HttpPost]
        [Authorize(Roles = Roles.VenueAdmin)]
        public async Task<IActionResult> GetMyVenuesAwaitingApproval([FromQuery] PaginationRequest paginationRequest, [FromBody] FilterationRequest filterRequest)
        {
            var result = await _venueRequestService.GetMyVenuesAwaitingApproval(paginationRequest, filterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<VenueRequestDTO>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetAwaitingVenueDetailsForAdmin([MongoId][Required] string requestId)
        {
            var result = await _venueRequestService.GetAwaitingVenueDetailsForAdmin(requestId);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<OfferWithUsageResponse>>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetOffersPageInAwaitingVenue([MongoId][Required][FromQuery] string requestId, [FromQuery] PaginationRequest paginationRequest)
        {
            var result = await _venueRequestService.GetOffersPageInAwaitingVenue(requestId, paginationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<EventSummaryResponse>>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetUpcomingEventsPageInAwaitingVenue([MongoId][Required][FromQuery] string requestId, [FromQuery] PaginationRequest paginationRequest)
        {
            var result = await _venueRequestService.GetUpcomingEventsPageInAwaitingVenue(requestId, paginationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<List<TermsAndConditionsResponse>>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetAwaitingVenueTermsAndConditions([MongoId][Required] string requestId)
        {
            var result = await _venueRequestService.GetAwaitingVenueTermsAndConditions(requestId);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Roles(Roles.VenueAdmin)]
        public async Task<IActionResult> DeleteVenueRequest([Required][MongoId] string requestId)
        {
            var result = await _venueRequestService.DeleteVenueRequest(requestId);
            return Ok(SuccessHelper.Wrap(result));
        }
    }
}
