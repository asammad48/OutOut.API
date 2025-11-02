using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOut.Constants;
using OutOut.Core.Services;
using OutOut.Models.Wrappers;
using OutOut.ViewModels.Requests.Categories;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Requests.TypesFor;
using OutOut.ViewModels.Responses.Categories;
using OutOut.ViewModels.Validators;
using OutOut.ViewModels.Wrappers;
using System.ComponentModel.DataAnnotations;

namespace OutOut.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _categoryService;
        public CategoryController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [Produces(typeof(OperationResult<List<CategoryResponse>>))]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetActiveCategories([FromQuery] TypeForRequest request)
        {
            var result = await _categoryService.GetActiveCategories(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<Page<CategoryResponse>>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> GetCategoriesPage([FromQuery] PaginationRequest paginationRequest, FilterationRequest filterationRequest)
        {
            var result = await _categoryService.GetCategoriesPage(paginationRequest, filterationRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<List<CategoryResponse>>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> GetCategoriesByType([FromBody][Required] TypeForRequest request)
        {
            var result = await _categoryService.GetCategoriesByType(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<CategoryResponse>))]
        [HttpGet]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> GetCategory([MongoId] string id)
        {
            var result = await _categoryService.GetCategory(id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<CategoryResponse>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> CreateCategory([FromForm] CreateCategoryRequest request)
        {
            var result = await _categoryService.CreateCategory(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<CategoryResponse>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> UpdateCategory([MongoId] string id, [FromForm] UpdateCategoryRequest request)
        {
            var result = await _categoryService.UpdateCategory(id, request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> DeleteCategory([MongoId] string id)
        {
            var result = await _categoryService.DeleteCategory(id);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> UpdateCatgoriesOrder(UpdateCategoriesOrders updatedCategories)
        {
            var result = await _categoryService.UpdateCategoriesOrder(updatedCategories);
            return Ok(SuccessHelper.Wrap(result));
        }
    }
}
