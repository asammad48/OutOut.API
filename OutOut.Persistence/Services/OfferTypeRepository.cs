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
    public class OfferTypeRepository : GenericNonSqlRepository<OfferType>, IOfferTypeRepository
    {
        public OfferTypeRepository(ApplicationNonSqlDbContext dbContext, IEnumerable<ISyncRepository<OfferType>> syncRepositories) : base(dbContext, syncRepositories)
        {
        }

        public async Task<List<OfferType>> GetAllOfferTypes()
        {
            var collation = new Collation(locale: "en", strength: CollationStrength.Secondary);
            var result = await _collection.FindAsync(offerType => true, new FindOptions<OfferType, OfferType> { Sort = Builders<OfferType>.Sort.Ascending(a => a.Name), Collation = collation });
            return result.ToList();
        }

        public async Task<Page<OfferType>> GetOfferTypesPage(PaginationRequest paginationRequest, FilterationRequest filterationRequest)
        {
            var searchFilter = Builders<OfferType>.Filter.Empty;
            if (filterationRequest != null && !string.IsNullOrEmpty(filterationRequest.SearchQuery))
                searchFilter = Builders<OfferType>.Filter.SearchContains(a => a.Name, filterationRequest.SearchQuery);
            
            var collation = new Collation(locale: "en", strength: CollationStrength.Secondary);

            var sort = filterationRequest.SortBy switch
            {
                Sort.Newest => Builders<OfferType>.Sort.Descending(a => a.CreationDate).Ascending(a => a.Name),
                Sort.Alphabetical => Builders<OfferType>.Sort.Ascending(a => a.Name),
                (_) => Builders<OfferType>.Sort.Ascending(a => a.Name),
            };

            var records = await _collection.FindAsync(searchFilter, new FindOptions<OfferType, OfferType> { Sort = sort, Collation = collation });
            return records.ToList().GetPaged(paginationRequest);
        }
    }
}
