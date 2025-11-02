using OutOut.Constants.Enums;
using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Requests.TypesFor;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Interfaces
{
    public interface ICategoryRepository : IGenericNonSqlRepository<Category>
    {
        Task<Page<Category>> GetAllCategories(PaginationRequest paginationRequest, FilterationRequest filterRequest);
        Task<Category> GetByTypeAndName(TypeFor type, string categoryName);
        Task<List<Category>> GetByType(TypeForRequest request);
        Task<List<Category>> GetActiveCategoriesByType(TypeForRequest request);
        Task<bool> UpdateCatgoriesOrderByIds(Dictionary<string,int> categoriesWithOrders);
        Task<int> GetCountOfCategoryByType(TypeFor type);
        Task<List<Category>> GetCategoriesByType(TypeFor type);

    }
}
