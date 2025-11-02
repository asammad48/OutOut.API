using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Interfaces
{
    public interface IOfferTypeRepository : IGenericNonSqlRepository<OfferType>
    {
        Task<List<OfferType>> GetAllOfferTypes();
        Task<Page<OfferType>> GetOfferTypesPage(PaginationRequest paginationRequest, FilterationRequest filterationRequest);
    }
}
