using AutoMapper;
using MongoDB.Driver;
using OutOut.Models.Models;
using OutOut.Infrastructure.Services;
using OutOut.Models;
using Microsoft.Extensions.Options;
using OutOut.Persistence.Interfaces;
using OutOut.ViewModels.Responses.Categories;
using OutOut.ViewModels.Requests.TypesFor;
using OutOut.Models.Exceptions;
using OutOut.Constants.Errors;
using OutOut.ViewModels.Requests.Categories;
using OutOut.Models.Wrappers;
using OutOut.ViewModels.Wrappers;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.Constants.Enums;
using OutOut.Constants;

namespace OutOut.Core.Services
{
    public class CategoryService
    {
        private readonly IMapper _mapper;
        private readonly FileUploaderService _fileUploaderService;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IVenueRepository _venueRepository;
        private readonly IEventRepository _eventRepository;
        private readonly AppSettings _appSettings;
        private readonly NotificationComposerService _notificationComposerService;

        public CategoryService(IMapper mapper,
                               FileUploaderService fileUploaderService,
                               IOptions<AppSettings> appSettings,
                               ICategoryRepository categoryRepo,
                               IVenueRepository venueRepository,
                               IEventRepository eventRepository,
                               NotificationComposerService notificationComposerService)
        {
            _mapper = mapper;
            _fileUploaderService = fileUploaderService;
            _appSettings = appSettings.Value;
            _categoryRepository = categoryRepo;
            _venueRepository = venueRepository;
            _eventRepository = eventRepository;
            _notificationComposerService = notificationComposerService;
        }

        public async Task<List<CategoryResponse>> GetActiveCategories(TypeForRequest request)
        {
            var categories = new List<Category>();

            if (request.TypeFor == null)
                categories = await _categoryRepository.GetAll();


            else
                categories = await _categoryRepository.GetActiveCategoriesByType(request);

            return _mapper.Map<List<CategoryResponse>>(categories.OrderBy(a => a.Order).ToList());
        }

        public async Task<Page<CategoryResponse>> GetCategoriesPage(PaginationRequest paginationRequest, FilterationRequest filterationRequest)
        {
            var categories = await _categoryRepository.GetAllCategories(paginationRequest, filterationRequest);
            return _mapper.Map<Page<CategoryResponse>>(categories);
        }

        public async Task<List<CategoryResponse>> GetCategoriesByType(TypeForRequest request)
        {
            var categories = await _categoryRepository.GetCategoriesByType((TypeFor)request.TypeFor);
            var result = _mapper.Map<List<CategoryResponse>>(categories);
            return result;
        }

        public async Task<CategoryResponse> GetCategory(string id)
        {
            var category = await _categoryRepository.GetById(id);
            if (category == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            return _mapper.Map<CategoryResponse>(category);
        }

        public async Task<CategoryResponse> CreateCategory(CreateCategoryRequest request)
        {
            var category = _mapper.Map<Category>(request);

            if (request.Icon != null)
            {
                var fileName = await _fileUploaderService.UploadFile(_appSettings.Directories.CategoryIcons, request.Icon);
                category.Icon = fileName;
            }

            category.Order = await _categoryRepository.GetCountOfCategoryByType(request.TypeFor) + 1;
            var result = await _categoryRepository.Create(category);

            if (category.TypeFor == TypeFor.Venue && request.IsActive)
                await _notificationComposerService.SendSignalRNotification(NotificationAction.NewCategory,
                                                                          $"New Category “{result.Name}” has been added by Super Admin",
                                                                          result.Id,
                                                                          Roles.VenueAdmin);
            else if (category.TypeFor == TypeFor.Event && request.IsActive)
                await _notificationComposerService.SendSignalRNotification(NotificationAction.NewCategory,
                                                                          $"New Category “{result.Name}” has been added by Super Admin",
                                                                          result.Id,
                                                                          new List<string> { Roles.EventAdmin, Roles.VenueAdmin });
            return _mapper.Map<CategoryResponse>(result);
        }

        public async Task<CategoryResponse> UpdateCategory(string id, UpdateCategoryRequest request)
        {
            var existingCategory = await _categoryRepository.GetById(id);
            if (existingCategory == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            var category = _mapper.Map(request, existingCategory);

            if (request.Icon != null)
            {
                var oldCategoryIcon = existingCategory.Icon;
                category.Icon = await _fileUploaderService.UploadFile(_appSettings.Directories.CategoryIcons, request.Icon);
                _fileUploaderService.DeleteFile(_appSettings.Directories.CategoryIcons, oldCategoryIcon);
            }

            var result = await _categoryRepository.Update(category);
            return _mapper.Map<CategoryResponse>(result);
        }

        public async Task<bool> DeleteCategory(string id)
        {
            var category = await _categoryRepository.GetById(id);
            if (category == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            await _venueRepository.DeleteCategory(id);
            await _eventRepository.DeleteCategory(id);

            return await _categoryRepository.Delete(id);
        }

        public async Task<bool> UpdateCategoriesOrder(UpdateCategoriesOrders updateCategoriesOrders)
        {

            try
            {
                var toDictonary = updateCategoriesOrders.UpdateCategoryOrders.ToDictionary(l => l.Id, l => l.Order);
                return await _categoryRepository.UpdateCatgoriesOrderByIds(toDictonary);

            }
            catch (System.Exception ex)
            {

                throw;
            }

        }
    }
}
