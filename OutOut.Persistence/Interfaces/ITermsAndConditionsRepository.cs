using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Interfaces
{
    public interface ITermsAndConditionsRepository : IGenericNonSqlRepository<TermsAndConditions>
    {
        Task<Page<TermsAndConditions>> GetTermsAndConditionsPage(PaginationRequest paginationRequest, FilterationRequest filterRequest);
        Task<List<TermsAndConditions>> GetVenueTermsAndConditions(List<string> termsAndConditionsIds);
        Task<List<TermsAndConditions>> GetActiveTermsAndConditions();
    }
}
