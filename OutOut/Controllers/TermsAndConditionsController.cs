using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOut.Constants;
using OutOut.Core.Services;
using OutOut.Models.Wrappers;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Requests.TermsAndConditions;
using OutOut.ViewModels.Responses.TermsAndConditions;
using OutOut.ViewModels.Validators;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TermsAndConditionsController : ControllerBase
    {
        private readonly TermsAndConditionsService _termsAndConfiditionsService;
        public TermsAndConditionsController(TermsAndConditionsService termsAndConfiditionsService)
        {
            _termsAndConfiditionsService = termsAndConfiditionsService;
        }

        [Produces(typeof(OperationResult<Page<TermsAndConditionsResponse>>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> GetTermsAndConditionsPage([FromQuery] PaginationRequest paginationRequest, FilterationRequest filterationRequest)
        {
            var result = await _termsAndConfiditionsService.GetTermsAndConditionsPage(paginationRequest, filterationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<List<TermsAndConditionsResponse>>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> GetActiveTermsAndConditions()
        {
            var result = await _termsAndConfiditionsService.GetActiveTermsAndConditions();
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<TermsAndConditionsResponse>))]
        [HttpGet]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> GetTermAndCondition([MongoId] string id)
        {
            var result = await _termsAndConfiditionsService.GetTermAndCondition(id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<TermsAndConditionsResponse>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> CreateTermsAndConditions(TermsAndConditionsRequest request)
        {
            var result = await _termsAndConfiditionsService.CreateTermsAndConditions(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<TermsAndConditionsResponse>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> UpdateTermsAndConditions([MongoId] string id, TermsAndConditionsRequest request)
        {
            var result = await _termsAndConfiditionsService.UpdateTermsAndConditions(id, request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> DeleteTermsAndConditions([MongoId] string id)
        {
            var result = await _termsAndConfiditionsService.DeleteTermsAndConditions(id);
            return Ok(SuccessHelper.Wrap(result));
        }
    }
}
