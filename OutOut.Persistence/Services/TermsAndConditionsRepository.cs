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
    public class TermsAndConditionsRepository : GenericNonSqlRepository<TermsAndConditions>, ITermsAndConditionsRepository
    {
        public TermsAndConditionsRepository(ApplicationNonSqlDbContext dbContext, IEnumerable<ISyncRepository<TermsAndConditions>> syncRepositories) : base(dbContext, syncRepositories)
        {
        }

        public async Task<Page<TermsAndConditions>> GetTermsAndConditionsPage(PaginationRequest paginationRequest, FilterationRequest filterRequest)
        {
            var searchFilter = Builders<TermsAndConditions>.Filter.Empty;
            if (filterRequest != null && !string.IsNullOrEmpty(filterRequest.SearchQuery))
                searchFilter = Builders<TermsAndConditions>.Filter.SearchContains(a => a.TermCondition, filterRequest.SearchQuery);

            var collation = new Collation(locale: "en", strength: CollationStrength.Secondary);

            var sort = filterRequest.SortBy switch
            {
                Sort.Newest => Builders<TermsAndConditions>.Sort.Descending(a => a.CreationDate).Ascending(a => a.TermCondition),
                Sort.Alphabetical => Builders<TermsAndConditions>.Sort.Ascending(a => a.TermCondition),
                (_) => Builders<TermsAndConditions>.Sort.Ascending(a => a.TermCondition),
            };

            var records = await _collection.FindAsync(searchFilter, new FindOptions<TermsAndConditions, TermsAndConditions> { Sort = sort, Collation = collation });
            return records.ToList().GetPaged(paginationRequest);
        }

        public async Task<List<TermsAndConditions>> GetVenueTermsAndConditions(List<string> termsAndConditionsIds)
        {
            var termsAndConditionsFilter = Builders<TermsAndConditions>.Filter.In(c => c.Id, termsAndConditionsIds) &
                                           Builders<TermsAndConditions>.Filter.Eq(c => c.IsActive, true);
            return await Find(termsAndConditionsFilter);
        }

        public async Task<List<TermsAndConditions>> GetActiveTermsAndConditions()
        {
            var filter = Builders<TermsAndConditions>.Filter.Eq(c => c.IsActive, true);
            var result = await Find(filter);
            return result.OrderBy(a => a.TermCondition).ToList();
        }

    }
}
