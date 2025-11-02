using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOut.Constants;
using OutOut.Core.Services;
using OutOut.Models.Wrappers;
using OutOut.ViewModels.Requests.Customers;
using OutOut.ViewModels.Requests.Loyalties;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Requests.Users;
using OutOut.ViewModels.Requests.Venues;
using OutOut.ViewModels.Responses.Customers;
using OutOut.ViewModels.Responses.EventBookings;
using OutOut.ViewModels.Responses.Events;
using OutOut.ViewModels.Responses.Loyalties;
using OutOut.ViewModels.Responses.Notifications;
using OutOut.ViewModels.Responses.Users;
using OutOut.ViewModels.Responses.VenueBooking;
using OutOut.ViewModels.Responses.Venues;
using OutOut.ViewModels.Validators;
using OutOut.ViewModels.Wrappers;
using System.ComponentModel.DataAnnotations;

namespace OutOut.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly CustomerService _customerService;
        private readonly LocationService _userLocationService;
        private readonly NotificationService _notificationService;

        public CustomersController(CustomerService customerService, LocationService userLocationService, NotificationService notificationService)
        {
            _customerService = customerService;
            _userLocationService = userLocationService;
            _notificationService = notificationService;
        }


        [Produces(typeof(OperationResult<ApplicationUserResponse>))]
        [HttpGet]
        [Authorize]
        public IActionResult GetAccountInfo()
        {
            var result = _customerService.GetAccountInfo();
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<ApplicationUserSummaryResponse>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> GetCustomersPage([FromQuery] PaginationRequest paginationRequest, FilterationRequest filterRequest)
        {
            var result = await _customerService.GetCustomersPage(paginationRequest, filterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> ExportAllCustomersInfoToExcel()
        {
            var result = await _customerService.ExportAllCustomersInfoToExcel();
            return File(result.File, "application/octet-stream", result.FileName);
        }

        [Produces(typeof(OperationResult<ApplicationUserResponse>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> GetCustomer([Required] string id)
        {
            var result = await _customerService.GetCustomer(id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> ExportCustomerInfoToExcel([Required] string id)
        {
            var result = await _customerService.ExportCustomerInfoToExcel(id);
            return File(result.File, "application/octet-stream", result.FileName);
        }

        [Produces(typeof(OperationResult<Page<CustomerEventBookingResponse>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> GetCustomersAttendedEventsPage([FromQuery][Required] string id, [FromQuery] PaginationRequest paginationRequest, SearchFilterationRequest filterRequest)
        {
            var result = await _customerService.GetCustomersAttendedEventsPage(id, paginationRequest, filterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<CustomerLoyaltyResponse>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetCustomersAvailedLoyaltyPage([FromQuery][Required] string id, [FromQuery] PaginationRequest paginationRequest, SearchFilterationRequest filterRequest)
        {
            var result = await _customerService.GetCustomersAvailedLoyaltyPage(id, paginationRequest, filterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<CustomerOfferResponse>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetCustomersAvailedOffersPage([FromQuery][Required] string id, [FromQuery] PaginationRequest paginationRequest, SearchFilterationRequest searchFilterationRequest)
        {
            var result = await _customerService.GetCustomersAvailedOffersPage(id, paginationRequest, searchFilterationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<ApplicationUserResponse>))]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateAccountInfo([FromForm] CustomerUpdateAccountRequest request)
        {
            var result = await _customerService.UpdateAccountInfoAsync(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<ApplicationUserResponse>))]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateUserLocation(UserLocationRequest userLocationRequest)
        {
            var result = await _userLocationService.UpdateUserLocation(userLocationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<ApplicationUserResponse>))]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateNotificationsAllowed([FromBody] UpdateNotificationsAllowedRequest request)
        {
            var result = await _customerService.UpdateNotificationsAllowed(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<ApplicationUserResponse>))]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateRemindersAllowed([FromBody] UpdateRemindersAllowedRequest request)
        {
            var result = await _customerService.UpdateRemindersAllowed(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<VenueSummaryResponse>>))]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetFavoriteVenues([FromQuery] PaginationRequest request, [FromBody] SearchFilterationRequest filterRequest)
        {
            var result = await _customerService.GetFavoriteVenues(request, filterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<EventSummaryResponse>>))]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetFavoriteEvents([FromQuery] PaginationRequest request, [FromBody] SearchFilterationRequest filterRequest)
        {
            var result = await _customerService.GetFavoriteEventsOccurrences(request, filterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<LoyaltyResponse>>))]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetMyLoyalty([FromQuery] PaginationRequest paginationRequest, [FromBody] LoyaltyFilterationRequest filterRequest)
        {
            var result = await _customerService.GetMyLoyalty(paginationRequest, filterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<VenueBookingResponse>>))]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetMyVenueBookings([FromQuery] PaginationRequest paginationRequest, [FromBody] MyBookingFilterationRequest filterRequest)
        {
            var result = await _customerService.GetMyVenueBookings(paginationRequest, filterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<EventBookingSummaryResponse>>))]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetMyEventBookings([FromQuery] PaginationRequest paginationRequest, [FromBody] MyBookingFilterationRequest filterRequest)
        {
            var result = await _customerService.GetMyEventBookings(paginationRequest, filterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<SingleEventBookingTicketSummaryResponse>>))]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetMyEventSharedTickets([FromQuery] PaginationRequest paginationRequest, [FromBody] MyBookingFilterationRequest filterRequest)
        {
            var result = await _customerService.GetMyEventSharedTickets(paginationRequest, filterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<CustomNotificationPage<NotificationResponse>>))]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetMyNotifications([FromQuery] PaginationRequest paginationRequest)
        {
            var result = await _notificationService.GetMyNotifications(paginationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }


        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> MarkNotificationsAsRead([FromQuery] List<string> notificationIds)
        {
            var result = await _notificationService.MarkNotificationsAsRead(notificationIds);
            return Ok(SuccessHelper.Wrap(result));
        }


        [Produces(typeof(OperationResult<long>))]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetUnReadNotificationNumber()
        {
            var result = await _notificationService.GetUserUnReadNotificationsCount();
            return Ok(SuccessHelper.Wrap(result));
        }
    }
}
