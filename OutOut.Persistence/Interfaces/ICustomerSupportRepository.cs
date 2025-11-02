using OutOut.Constants.Enums;
using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Interfaces
{
    public interface ICustomerSupportRepository : IGenericNonSqlRepository<CustomerSupportMessage>
    {
        Task<Page<CustomerSupportMessage>> GetAllCustomerServices(PaginationRequest paginationRequest, FilterationRequest filterRequest);
        Task<bool> UpdateStatus(string id, CustomerSupportStatus status);
    }
}
