using MongoDB.Driver;
using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Data;
using OutOut.Persistence.Extensions;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.Persistence.Services.Basic;
using OutOut.ViewModels.Requests.FAQs;
using OutOut.ViewModels.Requests.Venues;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Services
{
    public class FAQRepository : GenericNonSqlRepository<FAQ>, IFAQRepository
    {
        public FAQRepository(ApplicationNonSqlDbContext dbContext, IEnumerable<ISyncRepository<FAQ>> syncRepositories) : base(dbContext, syncRepositories)
        {
        }

        public async Task<Page<FAQ>> GetFAQPage(PaginationRequest paginationRequest, FAQFilterationRequest filterRequest)
        {
            var searchFilter = Builders<FAQ>.Filter.Empty;
            if (!string.IsNullOrEmpty(filterRequest.SearchQuery))
                searchFilter = Builders<FAQ>.Filter.SearchContains(c => c.Question, filterRequest.SearchQuery);

            var collation = new Collation(locale: "en", strength: CollationStrength.Secondary);

            var sort = filterRequest.SortBy switch
            {
                SortFAQ.QuestionNumber => Builders<FAQ>.Sort.Ascending(a => a.QuestionNumber),
                SortFAQ.Alphabetical => Builders<FAQ>.Sort.Ascending(a => a.Question).Ascending(a => a.QuestionNumber),
                (_) => Builders<FAQ>.Sort.Ascending(a => a.QuestionNumber),
            };

            var records = await _collection.FindAsync(searchFilter, new FindOptions<FAQ, FAQ> { Sort = sort, Collation = collation });
            return records.ToList().GetPaged(paginationRequest);
        }

        public async Task<Page<FAQ>> GetAllFAQ(PaginationRequest paginationRequest, SearchFilterationRequest searchFilterRequest)
        {
            var searchFilter = Builders<FAQ>.Filter.Empty;
            if (!string.IsNullOrEmpty(searchFilterRequest?.SearchQuery))
                searchFilter = Builders<FAQ>.Filter.SearchContains(c => c.Question, searchFilterRequest.SearchQuery);

            var records = await Find(searchFilter);
            return records.OrderBy(a => a.QuestionNumber).GetPaged(paginationRequest);
        }

        public async Task<bool> ResetQuestionNumbers(int deletedQuestionNumber)
        {
            var filter = Builders<FAQ>.Filter.Gt(a => a.QuestionNumber, deletedQuestionNumber);
            var update = Builders<FAQ>.Update.Inc(a => a.QuestionNumber, -1);
            var updateResult = await _collection.UpdateManyAsync(filter, update);
            return updateResult.IsAcknowledged;
        }
    }
}
