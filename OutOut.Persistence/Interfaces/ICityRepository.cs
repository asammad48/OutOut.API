using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.ViewModels.Requests.Areas;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Interfaces
{
    public interface ICityRepository : IGenericNonSqlRepository<City>
    {
        Task<List<City>> GetActiveCities();
        Task<Page<City>> GetCitiesPage(PaginationRequest paginationRequest, FilterationRequest filterRequest);
        Task<bool> DeleteArea(string id, AreaRequest request);
        Task<bool> UpdateArea(string id, UpdateAreaRequest request);
        Task<City> GetByArea(string cityId, string area);
        Task<bool> CityExists(string cityName);
        Task<City> UpdateCity(City city);
    }
}
