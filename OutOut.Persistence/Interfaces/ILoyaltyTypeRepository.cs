using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Interfaces
{
    public interface ILoyaltyTypeRepository : IGenericNonSqlRepository<LoyaltyType>
    {
        Task<List<LoyaltyType>> GetAllLoyaltyTypes();
        Task<Page<LoyaltyType>> GetLoyaltyTypesPage(PaginationRequest paginationRequest, FilterationRequest filterationRequest);
    }
}
