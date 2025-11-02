using OutOut.Models.Models;
using OutOut.Persistence.Data;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.Persistence.Services.Basic;

namespace OutOut.Persistence.Services
{
    public class CountryRepository : GenericNonSqlRepository<Country>, ICountryRepository
    {
        public CountryRepository(ApplicationNonSqlDbContext dbContext, IEnumerable<ISyncRepository<Country>> syncRepositories) : base(dbContext, syncRepositories)
        {
        }
    }
}
