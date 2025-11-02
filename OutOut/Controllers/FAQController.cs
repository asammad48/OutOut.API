using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOut.Constants;
using OutOut.Core.Services;
using OutOut.Models.Wrappers;
using OutOut.ViewModels.Requests.FAQs;
using OutOut.ViewModels.Requests.Venues;
using OutOut.ViewModels.Responses.FAQs;
using OutOut.ViewModels.Validators;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FAQController : ControllerBase
    {
        private readonly FAQService _faqService;
        public FAQController(FAQService faqService)
        {
            _faqService = faqService;
        }

        [Produces(typeof(OperationResult<Page<FAQResponse>>))]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetFAQPage([FromQuery] PaginationRequest paginationRequest, [FromBody] FAQFilterationRequest filterRequest)
        {
            var result = await _faqService.GetFAQPage(paginationRequest, filterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<FAQResponse>>))]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetAllFAQ([FromQuery] PaginationRequest paginationRequest, [FromBody] SearchFilterationRequest searchFilterRequest)
        {
            var result = await _faqService.GetAllFAQ(paginationRequest, searchFilterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<FAQResponse>))]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetFAQ([MongoId] string id)
        {
            var result = await _faqService.GetFAQ(id);
            return Ok(SuccessHelper.Wrap(result));
        } 
        
        [Produces(typeof(OperationResult<int>))]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetNextQuestionNumber()
        {
            var result = await _faqService.GetNextQuestionNumber();
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<FAQResponse>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> CreateFAQ(FAQRequest request)
        {
            var result = await _faqService.CreateFAQ(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<FAQResponse>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> UpdateFAQ([MongoId] string id, FAQRequest request)
        {
            var result = await _faqService.UpdateFAQ(id, request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<FAQResponse>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> DeleteFAQ([MongoId] string id)
        {
            var result = await _faqService.DeleteFAQ(id);
            return Ok(SuccessHelper.Wrap(result));
        }
    }
}
