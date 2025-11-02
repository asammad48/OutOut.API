using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOut.Constants;
using OutOut.Core.Services;
using OutOut.Models.Wrappers;
using OutOut.ViewModels.Requests.Areas;
using OutOut.ViewModels.Requests.Cities;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Responses.Cities;
using OutOut.ViewModels.Responses.Countries;
using OutOut.ViewModels.Validators;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class CityController : ControllerBase
    {
        private readonly CityService _cityService;
        public CityController(CityService cityService)
        {
            _cityService = cityService;
        }

        [Produces(typeof(OperationResult<Page<CityResponse>>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> GetCitiesPage([FromQuery] PaginationRequest paginationRequest, FilterationRequest filterRequest)
        {
            var result = await _cityService.GetCitiesPage(paginationRequest, filterRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<List<CityResponse>>))]
        [HttpGet]
        public async Task<IActionResult> GetActiveCities()
        {
            var result = await _cityService.GetActiveCities();
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<CityResponse>))]
        [HttpGet]
        public async Task<IActionResult> GetCity([MongoId] string id)
        {
            var result = await _cityService.GetCity(id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<CityResponse>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> CreateCity(CreateCityRequest createCityRequest)
        {
            var result = await _cityService.CreateCity(createCityRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<CityResponse>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> UpdateCity([MongoId] string id, UpdateCityRequest createCityRequest)
        {
            var result = await _cityService.UpdateCity(id, createCityRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> DeleteCity([MongoId] string id)
        {
            var result = await _cityService.DeleteCity(id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<string>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> CreateArea([FromQuery][MongoId] string id, [FromBody] AreaRequest request)
        {
            var result = await _cityService.CreateArea(id, request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> UpdateArea([FromQuery][MongoId] string id, UpdateAreaRequest request)
        {
            var result = await _cityService.UpdateArea(id, request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> DeleteArea([FromQuery][MongoId] string id, [FromBody] AreaRequest request)
        {
            var result = await _cityService.DeleteArea(id, request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<List<CountryResponse>>))]
        [HttpGet]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> GetAllCountries()
        {
            var result = await _cityService.GetAllCountries();
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<CountryResponse>))]
        [HttpGet]
        public async Task<IActionResult> GetCountry([MongoId] string id)
        {
            var result = await _cityService.GetCountry(id);
            return Ok(SuccessHelper.Wrap(result));
        }
    }
}
