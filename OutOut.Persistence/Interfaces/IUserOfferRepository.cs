using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Requests.Venues;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Interfaces
{
    public interface IUserOfferRepository : IGenericNonSqlRepository<UserOffer>
    {
        Task<UserOffer> GetUserRedeems(string userId, string offerId, DateTime inDate);
        Task<List<UserOffer>> GetUserRedeemsThisYear(string userId, string offerId);
        long GetUserOffersCountById(string id);
        Task DeleteUserOfferByType(string offerTypeId);
        long GetUserOffersCount(string offerTypeId);
        Task DeleteUserOffersByAssignedOffer(string assignedOfferId);
        Task<Page<UserOffer>> GetCustomersAvailedOffers(string userId, PaginationRequest paginationRequest, SearchFilterationRequest searchFilterationRequest, List<string> accessibleVenues, bool isSuperAdmin);
        long GetRedemptionsCount(string offerTypeId, string userId, string venueId);
        long GetOfferUsageCountByVenueId(string venueId, string typeId);
        Task<bool> DeleteAllUserOffers();
        Task SyncVenueData(Venue oldVenue, Venue updatedVenue);
        Task<List<UserOffer>> GetUserOffersByAssignedOffer(string assignedOfferId);
        Task<List<UserOffer>> GetUserOffersByVenueId(string venueId);
        Task<Page<UserOffer>> GetUserOffersByUserId(string userId, PaginationRequest paginationRequest, FilterationRequest filterationRequest);
        long GetUserOffersCountByUserIdAndOfferId(string offerId, string userId);
    }
}
