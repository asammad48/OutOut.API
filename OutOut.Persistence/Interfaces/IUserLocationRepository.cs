using OutOut.Models.Identity;
using OutOut.Models.Models;

namespace OutOut.Persistence.Interfaces
{
    public interface IUserLocationRepository
    {
        Task<ApplicationUser> UpdateUserLocation(string userId, UserLocation userLocation);
        Task<ApplicationUser> GetUserLocation(string userId);
    }
}
