using MongoDB.Driver;
using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Data;
using OutOut.Persistence.Extensions;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.Persistence.Services.Basic;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Services
{
    public class LoyaltyTypeRepository : GenericNonSqlRepository<LoyaltyType>, ILoyaltyTypeRepository
    {
        public LoyaltyTypeRepository(ApplicationNonSqlDbContext dbContext, IEnumerable<ISyncRepository<LoyaltyType>> syncRepositories) : base(dbContext, syncRepositories)
        {
        }

        public async Task<List<LoyaltyType>> GetAllLoyaltyTypes()
        {
            var collation = new Collation(locale: "en", strength: CollationStrength.Secondary);
            var result = await _collection.FindAsync(offerType => true, new FindOptions<LoyaltyType, LoyaltyType> { Sort = Builders<LoyaltyType>.Sort.Ascending(a => a.Name), Collation = collation });
            return result.ToList();
        }

        public async Task<Page<LoyaltyType>> GetLoyaltyTypesPage(PaginationRequest paginationRequest, FilterationRequest filterationRequest)
        {
            var searchFilter = Builders<LoyaltyType>.Filter.Empty;
            if (filterationRequest != null && !string.IsNullOrEmpty(filterationRequest.SearchQuery))
                searchFilter = Builders<LoyaltyType>.Filter.SearchContains(a => a.Name, filterationRequest.SearchQuery);

            var collation = new Collation(locale: "en", strength: CollationStrength.Secondary);

            var sort = filterationRequest.SortBy switch
            {
                Sort.Newest => Builders<LoyaltyType>.Sort.Descending(a => a.CreationDate).Ascending(a => a.Name),
                Sort.Alphabetical => Builders<LoyaltyType>.Sort.Ascending(a => a.Name),
                (_) => Builders<LoyaltyType>.Sort.Ascending(a => a.Name),
            };

            var records = await _collection.FindAsync(searchFilter, new FindOptions<LoyaltyType, LoyaltyType> { Sort = sort, Collation = collation });
            return records.ToList().GetPaged(paginationRequest);
        }
    }
}
