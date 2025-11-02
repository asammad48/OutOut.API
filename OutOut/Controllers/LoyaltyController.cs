using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOut.Constants;
using OutOut.Core.Services;
using OutOut.Models.Wrappers;
using OutOut.ViewModels.Requests.Loyalties;
using OutOut.ViewModels.Requests.LoyaltyTypes;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Responses.Loyalties;
using OutOut.ViewModels.Responses.LoyaltyTypes;
using OutOut.ViewModels.Validators;
using OutOut.ViewModels.Wrappers;
using System.ComponentModel.DataAnnotations;

namespace OutOut.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class LoyaltyController : ControllerBase
    {
        private readonly LoyaltyService _loyaltyService;

        public LoyaltyController(LoyaltyService loyaltyService)
        {
            _loyaltyService = loyaltyService;
        }

        [Produces(typeof(OperationResult<LoyaltyResponse>))]
        [HttpPost]
        public async Task<IActionResult> ApplyLoyalty(UserLoyaltyRequest request)
        {
            var result = await _loyaltyService.ApplyLoyalty(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<LoyaltyTypeResponse>>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> GetLoyaltyTypesPage([FromQuery] PaginationRequest paginationRequest, FilterationRequest filterationRequest)
        {
            var result = await _loyaltyService.GetLoyaltyTypesPage(paginationRequest, filterationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<List<LoyaltyTypeSummaryResponse>>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetAllLoyaltyTypes()
        {
            var result = await _loyaltyService.GetAllLoyaltyTypes();
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<LoyaltyTypeResponse>))]
        [HttpGet]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> GetLoyaltyType([MongoId] string id)
        {
            var result = await _loyaltyService.GetLoyaltyType(id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<LoyaltyTypeResponse>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> AddLoyaltyType(CreateLoyaltyTypeRequest request)
        {
            var result = await _loyaltyService.AddLoyaltyType(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<LoyaltyTypeResponse>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> UpdateLoyaltyType([MongoId] string id, CreateLoyaltyTypeRequest request)
        {
            var result = await _loyaltyService.UpdateLoyaltyType(id, request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> DeleteLoyaltyType([MongoId] string id)
        {
            var result = await _loyaltyService.DeleteLoyaltyType(id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<AssignedLoyaltyResponse>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetAssignedLoyaltyPage([FromQuery] PaginationRequest paginationRequest, FilterationRequest filterationRequest)
        {
            var result = await _loyaltyService.GetAssignedLoyaltyPage(paginationRequest, filterationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<AssignedLoyaltyResponse>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetAssignedLoyalty([MongoId] string loyaltyId)
        {
            var result = await _loyaltyService.GetAssignedLoyalty(loyaltyId);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<AssignedLoyaltyResponse>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> AssignLoyaltyToVenue([FromForm] AssignedLoyaltyRequest request)
        {
            var result = await _loyaltyService.AssignLoyaltyToVenue(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> UpdateAssignedLoyalty([FromQuery][MongoId][Required] string loyaltyId, [FromForm] AssignedLoyaltyRequest request)
        {
            var result = await _loyaltyService.UpdateAssignedLoyalty(loyaltyId, request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> UnAssignLoyaltyFromVenue([MongoId][Required][FromQuery] string venueId, [MongoId][Required][FromQuery] string loyaltyId)
        {
            var result = await _loyaltyService.UnAssignLoyaltyFromVenue(venueId, loyaltyId);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> UnAssignAllLoyalty()
        {
            var result = await _loyaltyService.UnAssignAllLoyalty();
            return Ok(SuccessHelper.Wrap(result));
        }
    }
}
