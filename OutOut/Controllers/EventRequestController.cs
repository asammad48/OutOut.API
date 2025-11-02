using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOut.Constants;
using OutOut.Core.Services;
using OutOut.Models.Wrappers;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Responses.EventRequest;
using OutOut.ViewModels.Validators;
using OutOut.ViewModels.Wrappers;
using System.ComponentModel.DataAnnotations;

namespace OutOut.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class EventRequestController : ControllerBase
    {
        private readonly EventRequestService _eventRequestService;

        public EventRequestController(EventRequestService eventRequestService)
        {
            _eventRequestService = eventRequestService;
        }

        [Produces(typeof(OperationResult<Page<EventRequestSummaryDTO>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin)]
        public async Task<IActionResult> GetEventsAwaitingApproval([FromQuery] PaginationRequest paginationRequest, [FromBody] FilterationRequest filterRequest)
        {
            var result = await _eventRequestService.GetEventsAwaitingApproval(paginationRequest, filterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<EventRequestSummaryDTO>>))]
        [HttpPost]
        [Roles(Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> GetMyEventsAwaitingApproval([FromQuery] PaginationRequest paginationRequest, [FromBody] FilterationRequest filterRequest)
        {
            var result = await _eventRequestService.GetMyEventsAwaitingApproval(paginationRequest, filterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<EventRequestDTO>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> GetEventAwaitingApprovalDetailsForAdmin([Required][MongoId] string requestId)
        {
            var result = await _eventRequestService.GetEventAwaitingApprovalDetailsForAdmin(requestId);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Roles(Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> DeleteEventRequest([Required][MongoId] string requestId)
        {
            var result = await _eventRequestService.DeleteEventRequest(requestId);
            return Ok(SuccessHelper.Wrap(result));
        }
    }
}
