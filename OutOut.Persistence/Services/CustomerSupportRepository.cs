using MongoDB.Driver;
using OutOut.Constants.Enums;
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
    public class CustomerSupportRepository : GenericNonSqlRepository<CustomerSupportMessage>, ICustomerSupportRepository
    {
        public CustomerSupportRepository(ApplicationNonSqlDbContext dbContext, IEnumerable<ISyncRepository<CustomerSupportMessage>> syncRepositories) : base(dbContext, syncRepositories)
        {
        }

        public async Task<Page<CustomerSupportMessage>> GetAllCustomerServices(PaginationRequest paginationRequest, FilterationRequest filterRequest)
        {
            var searchFilter = Builders<CustomerSupportMessage>.Filter.Empty;
            if (filterRequest != null && !string.IsNullOrEmpty(filterRequest.SearchQuery))
                searchFilter = Builders<CustomerSupportMessage>.Filter.SearchContains(a => a.FullName, filterRequest.SearchQuery) |
                               Builders<CustomerSupportMessage>.Filter.SearchContains(a => a.Email, filterRequest.SearchQuery);
            
            var collation = new Collation(locale: "en", strength: CollationStrength.Secondary);

            var sort = filterRequest.SortBy switch
            {
                Sort.Newest => Builders<CustomerSupportMessage>.Sort.Descending(a => a.CreationDate).Ascending(a => a.FullName),
                Sort.Alphabetical => Builders<CustomerSupportMessage>.Sort.Ascending(a => a.FullName),
                (_) => Builders<CustomerSupportMessage>.Sort.Ascending(a => a.FullName),
            };

            var records = await _collection.FindAsync(searchFilter, new FindOptions<CustomerSupportMessage, CustomerSupportMessage> { Sort = sort, Collation = collation });
            return records.ToList().GetPaged(paginationRequest);
        }

        public async Task<bool> UpdateStatus(string id, CustomerSupportStatus status)
        {
            var filter = Builders<CustomerSupportMessage>.Filter.Eq(a => a.Id, id);
            var update = Builders<CustomerSupportMessage>.Update.Set(a => a.Status, status);
            var updateResult = await _collection.UpdateOneAsync(filter, update);
            return updateResult.IsAcknowledged;
        }
    }
}
