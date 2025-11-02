using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOut.Constants;
using OutOut.Core.Services;
using OutOut.Models.Wrappers;
using OutOut.ViewModels.Requests.CustomersSupport;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Responses.CustomersSupport;
using OutOut.ViewModels.Validators;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CustomerSupportController : ControllerBase
    {
        private readonly CustomerSupportService _customerSupportService;

        public CustomerSupportController(CustomerSupportService customerSupportService)
        {
            _customerSupportService = customerSupportService;
        }

        [Produces(typeof(OperationResult<CustomerSupportResponse>))]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostNewRequest(CustomerSupportRequest request)
        {
            var result = await _customerSupportService.PostNewRequest(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<CustomerSupportResponse>>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> GetCustomerServiceRequestsPage([FromQuery] PaginationRequest paginationRequest, FilterationRequest filterRequest)
        {
            var result = await _customerSupportService.GetCustomerServiceRequestsPage(paginationRequest, filterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<CustomerSupportResponse>))]
        [HttpGet]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> GetCustomerServiceRequest([MongoId] string id)
        {
            var result = await _customerSupportService.GetCustomerServiceRequest(id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> ResolveCustomerServiceRequest([MongoId] string id)
        {
            var result = await _customerSupportService.ResolveCustomerServiceRequest(id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> RejectCustomerServiceRequest([MongoId] string id)
        {
            var result = await _customerSupportService.RejectCustomerServiceRequest(id);
            return Ok(SuccessHelper.Wrap(result));
        }
    }
}
