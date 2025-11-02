using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOut.Constants;
using OutOut.Core.Services;
using OutOut.Models.Wrappers;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Requests.Offers;
using OutOut.ViewModels.Requests.OfferTypes;
using OutOut.ViewModels.Responses.Offers;
using OutOut.ViewModels.Responses.OfferTypes;
using OutOut.ViewModels.Validators;
using OutOut.ViewModels.Wrappers;
using System.ComponentModel.DataAnnotations;

namespace OutOut.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class OfferController : ControllerBase
    {
        private readonly OfferService _offerService;

        public OfferController(OfferService offerService)
        {
            _offerService = offerService;
        }

        [Produces(typeof(OperationResult<List<OfferTypeSummaryResponse>>))]
        [HttpGet]
        public async Task<IActionResult> GetOfferTypes()
        {
            var result = await _offerService.GetOfferTypes();
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<OfferTypeResponse>>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> GetOfferTypesPage([FromQuery] PaginationRequest paginationRequest, FilterationRequest filterationRequest)
        {
            var result = await _offerService.GetOfferTypesPage(paginationRequest, filterationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<OfferTypeResponse>))]
        [HttpGet]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> GetOfferType([MongoId] string id)
        {
            var result = await _offerService.GetOfferType(id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<OfferTypeResponse>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> AddOfferType(OfferTypeRequest request)
        {
            var result = await _offerService.AddOfferType(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<OfferTypeResponse>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> UpdateOfferType([MongoId] string id, OfferTypeRequest request)
        {
            var result = await _offerService.UpdateOfferType(id, request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> DeleteOfferType([MongoId] string id)
        {
            var result = await _offerService.DeleteOfferType(id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<OfferWithVenueResponse>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetAssignedOffersPage([FromQuery] PaginationRequest paginationRequest, FilterationRequest filterationRequest)
        {
            var result = await _offerService.GetAssignedOffersPage(paginationRequest, filterationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }


        [Produces(typeof(OperationResult<Page<OfferWithUsageResponse>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetAssignedUpcomingOffersPage([FromQuery] PaginationRequest paginationRequest, FilterationRequest filterationRequest)
        {
            var result = await _offerService.GetAssignedUpcomingOffersPage(paginationRequest, filterationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<OfferWithUsageResponse>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetAssignedOffer([MongoId][Required] string offerId)
        {
            var result = await _offerService.GetAssignedOffer(offerId);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<OfferWithVenueResponse>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> AssignOfferToVenue([FromForm] AssignedOfferRequest request)
        {
            var result = await _offerService.AssignOfferToVenue(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> UpdateAssignedOffer([MongoId][Required] string offerId, [FromForm] AssignedOfferRequest request)
        {
            var result = await _offerService.UpdateAssignedOffer(offerId, request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> UnAssignOfferFromVenue([FromQuery][Required][MongoId] string venueId, [FromQuery][Required][MongoId] string offerId)
        {
            var result = await _offerService.UnAssignOfferFromVenue(venueId, offerId);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> UnAssignAllOffers()
        {
            var result = await _offerService.UnAssignAllOffers();
            return Ok(SuccessHelper.Wrap(result));
        }

        // mobile
        [Produces(typeof(OperationResult<Page<OfferWithVenueResponse>>))]
        [HttpPost]
        public async Task<IActionResult> GetActiveNonExpiredOffers([FromQuery] PaginationRequest paginationRequest, OfferFilterationRequest filterationRequest)
        {
            var result = await _offerService.GetActiveNonExpiredOffers(paginationRequest, filterationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        // mobile
        [Produces(typeof(OperationResult<Page<OfferWithVenueResponse>>))]
        [HttpPost]
        public async Task<IActionResult> GetActiveNonExpiredUpcomingOffers([FromQuery] PaginationRequest paginationRequest, OfferFilterationRequest filterationRequest)
        {
            var result = await _offerService.GetActiveNonExpiredUpcomingOffers(paginationRequest, filterationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpGet]
        public async Task<IActionResult> IsOfferExpired([FromQuery][MongoId] string offerId)
        {
            var result = await _offerService.IsOfferExpiredOrInActive(offerId);
            return Ok(SuccessHelper.Wrap(result));
        }


        [Produces(typeof(OperationResult<List<OfferWithUsageResponse>>))]
        [HttpGet]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> GetNewestOffers()
        {
            var result = await _offerService.GetNewestOffers();
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<bool>>))]
        [HttpPost]
        public async Task<IActionResult> RedeemOffer([FromQuery][MongoId] string offerId, [FromQuery] string pinCode)
        {
            var result = await _offerService.RedeemOffer(offerId, pinCode);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<OfferWithUsageResponse>>))]
        [HttpPost]
        public async Task<IActionResult> GetMyOffers([FromQuery] PaginationRequest paginationRequest, FilterationRequest filterationRequest)
        {
            var result = await _offerService.GetMyOffers(paginationRequest, filterationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }
    }
}
