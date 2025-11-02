using MongoDB.Driver;
using OutOut.Models.Models;
using OutOut.Persistence.Data;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.Persistence.Services.Basic;
using OutOut.Constants.Enums;
using OutOut.Models.Wrappers;
using OutOut.ViewModels.Wrappers;
using OutOut.Persistence.Extensions;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Requests.TypesFor;

namespace OutOut.Persistence.Services
{
    public class CategoryRepository : GenericNonSqlRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationNonSqlDbContext dbContext, IEnumerable<ISyncRepository<Category>> syncRepositories) : base(dbContext, syncRepositories)
        {
        }

        public async Task<Page<Category>> GetAllCategories(PaginationRequest paginationRequest, FilterationRequest filterRequest)
        {
            var searchFilter = Builders<Category>.Filter.Empty;
            if (filterRequest != null && !string.IsNullOrEmpty(filterRequest.SearchQuery))
                searchFilter = Builders<Category>.Filter.SearchContains(a => a.Name, filterRequest.SearchQuery);

            var collation = new Collation(locale: "en", strength: CollationStrength.Secondary);

            var sort = filterRequest.SortBy switch
            {
                Sort.Newest => Builders<Category>.Sort.Descending(a => a.CreationDate).Ascending(a => a.Name),
                Sort.Alphabetical => Builders<Category>.Sort.Ascending(a => a.Name),
                (_) => Builders<Category>.Sort.Ascending(a => a.Name),
            };

            var records = await _collection.FindAsync(searchFilter, new FindOptions<Category, Category> { Sort = sort, Collation = collation });
            return records.ToList().GetPaged(paginationRequest);
        }
        
        public async Task<List<Category>> GetCategoriesByType(TypeFor type)
        {
            var typeFilter = Builders<Category>.Filter.Eq(c => c.TypeFor, type);

            var collation = new Collation(locale: "en", strength: CollationStrength.Secondary);

            var sort = Builders<Category>.Sort.Ascending(a => a.Order);


            var records = await _collection.FindAsync(typeFilter, new FindOptions<Category, Category> { Sort = sort, Collation = collation });
            return records.ToList();
        }

        public async Task<Category> GetByTypeAndName(TypeFor type, string categoryName)
        {
            var filters = Builders<Category>.Filter.Eq(a => a.TypeFor, type) &
                          Builders<Category>.Filter.Eq(a => a.Name, categoryName);
            return await FindFirst(filters);
        }

        public async Task<List<Category>> GetByType(TypeForRequest request)
        {
            var typeFilter = Builders<Category>.Filter.Eq(a => a.TypeFor, request.TypeFor);
            return await Find(typeFilter);
        }

        public async Task<List<Category>> GetActiveCategoriesByType(TypeForRequest request)
        {
            var typeFilter = Builders<Category>.Filter.Eq(a => a.TypeFor, request.TypeFor) &
                             Builders<Category>.Filter.Eq(a => a.IsActive, true);
            return await Find(typeFilter);
        }

        public async Task<int> GetCountOfCategoryByType(TypeFor type)
        {
            var typeFilter = Builders<Category>.Filter.Eq(a => a.TypeFor, type);
            return (int)await Count(typeFilter);

        }

        public async Task<bool> UpdateCatgoriesOrderByIds(Dictionary<string, int> categoriesWithOrders)
        {
            var x = await BulkUpdateDocuments(categoriesWithOrders, l => l.Id, new(i => i.Order));

            return x.IsAcknowledged;
        }
    }
}
