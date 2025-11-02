using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.ViewModels.Requests.Loyalties;
using OutOut.ViewModels.Requests.Venues;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Interfaces
{
    public interface IUserLoyaltyRepository : IGenericNonSqlRepository<UserLoyalty>
    {
        Task<List<UserLoyalty>> GetAllLoyaltyRedeems(string userId, string loyaltyId, bool? isConsumed = null);
        Task<UserLoyalty> GetLatestUserLoyalty(string userId, string loyaltyId);
        Task<List<UserLoyalty>> GetUserLoyalty(string userId);
        Task<List<UserLoyalty>> GetUserLoyalty(LoyaltyFilterationRequest filterRequest, string userId);
        Task DeleteUserLoyaltyByType(string loyaltyTypeId);
        long GetConsumedUserLoyaltyCount(string loyaltyTypeId);
        Task DeleteUserLoyaltyByAssignedLoyalty(string assignedLoyaltyId, string venueId);
        Task DeleteUserLoyaltyByVenue(string venueId);
        Page<UserLoyalty> GetCustomersAvailedLoyalty(string userId, PaginationRequest paginationRequest, SearchFilterationRequest searchFilterationRequest, List<string> accessibleVenues, bool isSuperAdmin);
        long GetRedemptionsCount(string userId, string venueId);
        long GetLoyaltyUsageCount(string venueId);
        Task<bool> DeleteAllUserLoyalty();
        Task SyncVenueData(Venue oldVenue, Venue updatedVenue);
        Task<List<UserLoyalty>> GetUserLoyaltyByAssignedLoyalty(string assignedLoyaltyId, string venueId);
        Task<List<UserLoyalty>> GetUserLoyaltiesByLoyaltyId(string loyaltyId);
    }
}
