using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.ViewModels.Requests.FAQs;
using OutOut.ViewModels.Requests.Venues;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Interfaces
{
    public interface IFAQRepository : IGenericNonSqlRepository<FAQ>
    {
        Task<Page<FAQ>> GetFAQPage(PaginationRequest paginationRequest, FAQFilterationRequest filterRequest);
        Task<bool> ResetQuestionNumbers(int deletedQuestionNumber);
        Task<Page<FAQ>> GetAllFAQ(PaginationRequest paginationRequest, SearchFilterationRequest searchFilterRequest);
    }
}
