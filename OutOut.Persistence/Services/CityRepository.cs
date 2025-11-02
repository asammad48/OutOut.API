using MongoDB.Driver;
using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Data;
using OutOut.Persistence.Extensions;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.Persistence.Services.Basic;
using OutOut.ViewModels.Requests.Areas;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Services
{
    public class CityRepository : GenericNonSqlRepository<City>, ICityRepository
    {
        public CityRepository(ApplicationNonSqlDbContext dbContext, IEnumerable<ISyncRepository<City>> syncRepositories) : base(dbContext, syncRepositories)
        {
        }
        public async Task<List<City>> GetActiveCities()
        {
            var activeFilter = Builders<City>.Filter.Eq(a => a.IsActive, true);
            var records = await Find(activeFilter);
            return records.OrderBy(a => a.Name).ToList();
        }

        public async Task<Page<City>> GetCitiesPage(PaginationRequest paginationRequest, FilterationRequest filterRequest)
        {
            var searchFilter = Builders<City>.Filter.Empty;
            if (filterRequest != null && !string.IsNullOrEmpty(filterRequest.SearchQuery))
                searchFilter = Builders<City>.Filter.SearchContains(a => a.Name, filterRequest.SearchQuery);

            var collation = new Collation(locale: "en", strength: CollationStrength.Secondary);

            var sort = filterRequest.SortBy switch
            {
                Sort.Newest => Builders<City>.Sort.Descending(a => a.CreationDate).Ascending(a => a.Name),
                Sort.Alphabetical => Builders<City>.Sort.Ascending(a => a.Name),
                (_) => Builders<City>.Sort.Ascending(a => a.Name),
            };

            var records = await _collection.FindAsync(searchFilter, new FindOptions<City, City> { Sort = sort, Collation = collation });
            return records.ToList().GetPaged(paginationRequest);
        }

        public async Task<bool> DeleteArea(string id, AreaRequest request)
        {
            var filter = Builders<City>.Filter.Eq(city => city.Id, id) &
                         Builders<City>.Filter.Where(city => city.Areas.Any(a => a.ToLower() == request.Area.ToLower()));
            var update = Builders<City>.Update.Pull(a => a.Areas, request.Area);
            var updateResult = await _collection.UpdateManyAsync(filter, update);
            return updateResult.IsAcknowledged;
        }

        public async Task<bool> UpdateArea(string id, UpdateAreaRequest request)
        {
            var filter = Builders<City>.Filter.Eq(city => city.Id, id) &
                         Builders<City>.Filter.Where(city => city.Areas.Any(a => a == request.OldArea));
            var update = Builders<City>.Update.Set(a => a.Areas[-1], request.NewArea);
            var updateResult = await _collection.UpdateManyAsync(filter, update);
            return updateResult.IsAcknowledged;
        }

        public Task<City> GetByArea(string cityId, string area)
        {
            var filter = Builders<City>.Filter.Eq(city => city.Id, cityId) &
                         Builders<City>.Filter.Where(city => city.Areas.Any(a => a == area));
            return FindFirst(filter);
        }

        public async Task<bool> CityExists(string cityName)
        {
            var searchFilter = Builders<City>.Filter.SearchContains(a => a.Name, cityName);
            var existingCity = await _collection.FindAsync(searchFilter);
            var result = existingCity.ToList();
            foreach (var city in result)
            {
                if (string.Compare(city?.Name?.ToLower(), cityName.ToLower()) == 0)
                    return true;
            }
            return false;
        }

        public async Task<City> UpdateCity(City city)
        {
            City oldCity = null;
            if (_syncRepositories.Any())
                oldCity = await GetById(city.Id);

            var filter = Builders<City>.Filter.Eq(city => city.Id, city.Id);
            var update = Builders<City>.Update.Set(a => a.Name, city.Name).Set(a => a.IsActive, city.IsActive).Set(a => a.Country, city.Country);
            var updateResult = await _collection.UpdateOneAsync(filter, update);

            if (_syncRepositories.Any())
                await Sync(oldCity, city);

            return city;
        }
    }
}
