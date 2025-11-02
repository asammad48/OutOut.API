using OutOut.Models.Models;
using OutOut.Persistence.Interfaces.Basic;

namespace OutOut.Persistence.Interfaces
{
    public interface ICountryRepository : IGenericNonSqlRepository<Country>
    {
    }
}
