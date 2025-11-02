using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOut.Constants;
using OutOut.Core.Services;
using OutOut.Models.Wrappers;
using OutOut.ViewModels.Requests.AdminProfile;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Responses.Events;
using OutOut.ViewModels.Responses.Notifications;
using OutOut.ViewModels.Responses.Users;
using OutOut.ViewModels.Responses.Venues;
using OutOut.ViewModels.Validators;
using OutOut.ViewModels.Wrappers;
using System.ComponentModel.DataAnnotations;

namespace OutOut.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AdminProfileController : ControllerBase
    {
        private readonly AdminProfileService _adminProfileService;
        private readonly NotificationService _notificationService;

        public AdminProfileController(AdminProfileService adminProfileService, NotificationService notificationService)
        {
            _adminProfileService = adminProfileService;
            _notificationService = notificationService;
        }

        [Produces(typeof(OperationResult<ApplicationUserAdminResponse>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public IActionResult GetMyAccountInfo()
        {
            var result = _adminProfileService.GetMyAccountInfo();
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<VenueSummaryWithBookingResponse>>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetVenuesCreatedByMe([FromQuery] PaginationRequest paginationRequest)
        {
            var result = await _adminProfileService.GetVenuesCreatedByMe(paginationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<EventSummaryWithBookingResponse>>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> GetEventsCreatedByMe([FromQuery] PaginationRequest paginationRequest)
        {
            var result = await _adminProfileService.GetEventsCreatedByMe(paginationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<ApplicationUserAdminResponse>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> UpdateMyAccountInfo([FromForm] UpdateAdminAccountRequest request)
        {
            var result = await _adminProfileService.UpdateMyAccountInfo(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<ApplicationUserAdminResponse>>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> GetUsersPage([FromQuery] PaginationRequest paginationRequest, FilterationRequest filterationRequest)
        {
            var result = await _adminProfileService.GetUsersPage(paginationRequest, filterationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<ApplicationUserAdminResponse>))]
        [HttpGet]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> GetUserAccountInfo([Required] string id)
        {
            var result = await _adminProfileService.GetUserAccountInfo(id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<VenueSummaryWithBookingResponse>>))]
        [HttpGet]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> GetUserVenues([FromQuery][Required] string id, [FromQuery] PaginationRequest paginationRequest)
        {
            var result = await _adminProfileService.GetUserVenues(paginationRequest, id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<EventSummaryWithBookingResponse>>))]
        [HttpGet]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> GetUserEvents([FromQuery][Required] string id, [FromQuery] PaginationRequest paginationRequest)
        {
            var result = await _adminProfileService.GetUserEvents(paginationRequest, id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<VenueSummaryWithBookingResponse>>))]
        [HttpGet]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> GetVenuesUserAdminOn([FromQuery][Required] string id, [FromQuery] PaginationRequest paginationRequest)
        {
            var result = await _adminProfileService.GetVenuesUserAdminOn(paginationRequest, id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<ApplicationUserAdminResponse>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> CreateUser([FromForm] AdminProfileRequest request)
        {
            var result = await _adminProfileService.CreateUser(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<ApplicationUserAdminResponse>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> UpdateUser([FromQuery][Required] string id, [FromForm] UpdateAdminProfileRequest request)
        {
            var result = await _adminProfileService.UpdateUser(id, request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> DeleteUser([Required] string id)
        {
            var result = await _adminProfileService.DeleteUser(id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<List<string>>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public IActionResult GetSystemRoles()
        {
            var result = _adminProfileService.GetSystemRoles();
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<List<string>>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetMyAccessibleVenues()
        {
            var result = await _adminProfileService.GetMyAccessibleVenues();
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<List<string>>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> GetMyAccessibleEvents()
        {
            var result = await _adminProfileService.GetMyAccessibleEvents();
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<CustomNotificationPage<NotificationAdminResponse>>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin, Roles.TicketAdmin)]
        public async Task<IActionResult> GetMyNotificationsForAdmin([FromQuery] PaginationRequest paginationRequest)
        {
            var result = await _notificationService.GetMyNotificationsForAdmin(paginationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin, Roles.TicketAdmin)]
        public async Task<IActionResult> MarkNotificationsAsRead([FromBody] MarkNotificationsAsReadRequest notificationIds)
        {
            var result = await _notificationService.MarkNotificationsAsRead(notificationIds.NotificationIds);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin, Roles.TicketAdmin)]
        public async Task<IActionResult> MarkAllNotificationsAsRead()
        {
            var result = await _notificationService.MarkAllNotificationsAsRead();
            return Ok(SuccessHelper.Wrap(result));
        }
    }
}
