using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOut.Constants;
using OutOut.Core.Services;
using OutOut.Models.Wrappers;
using OutOut.ViewModels.Requests.HomePage;
using OutOut.ViewModels.Responses.Events;
using OutOut.ViewModels.Responses.HomePage;
using OutOut.ViewModels.Responses.Offers;
using OutOut.ViewModels.Responses.Venues;
using OutOut.ViewModels.Validators;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class HomeScreenController : ControllerBase
    {
        private readonly HomeScreenService _homeScreenService;

        public HomeScreenController(HomeScreenService homeScreenService)
        {
            _homeScreenService = homeScreenService;
        }

        [Produces(typeof(OperationResult<HomePageResponse>))]
        [HttpPost]
        public async Task<IActionResult> HomeSearchFilter([FromBody] HomePageFilterationRequest filterRequest)
        {
            var result = await _homeScreenService.HomeSearchFilter(filterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<VenueSummaryResponse>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> DashboardVenuesSearchFilter([FromBody] HomePageWebFilterationRequest filterRequest, [FromQuery] PaginationRequest paginationRequest)
        {
            var result = await _homeScreenService.DashboardVenuesSearchFilter(filterRequest, paginationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<EventSummaryResponse>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin)]
        public async Task<IActionResult> DashboardEventsSearchFilter([FromBody] HomePageWebFilterationRequest filterRequest, [FromQuery] PaginationRequest paginationRequest)
        {
            var result = await _homeScreenService.DashboardEventsSearchFilter(filterRequest, paginationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<OfferWithVenueResponse>>))]
        [HttpPost]
        [Roles(Roles.SuperAdmin, Roles.VenueAdmin)]
        public async Task<IActionResult> DashboardOffersSearchFilter([FromBody] HomePageWebFilterationRequest filterRequest, [FromQuery] PaginationRequest paginationRequest)
        {
            var result = await _homeScreenService.DashboardOffersSearchFilter(filterRequest, paginationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }
    }
}
