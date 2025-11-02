using OutOut.Models.Domain;
using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.ViewModels.Requests.HomePage;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Requests.Offers;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Interfaces
{
    public interface IOfferRepository
    {
        Task<Page<VenueOneOfferWithDistance>> GetActiveNonExpiredOffers(PaginationRequest paginationRequest, UserLocation userLocation, OfferFilterationRequest filterRequest, string userId, bool getUpcoming = false);
        Task<List<VenueOneOfferWithDistance>> HomeFilter(UserLocation userLocation, HomePageFilterationRequest filterRequest);
        Task<Page<VenueOneOffer>> DashboardFilter(PaginationRequest paginationRequest, HomePageWebFilterationRequest filterRequest, List<string> accessibleVenues, bool isSuperAdmin);
        Task<List<VenueOneOffer>> GetNewestOffers(List<string> accessibleVenues, bool isSuperAdmin);
        Task<VenueOneOffer> GetOfferById(string offerId);
        long GetAssignedOffersCount(string offerTypeId);
        Task<Page<VenueOneOffer>> GetAssignedOffersPage(PaginationRequest paginationRequest, FilterationRequest filterRequest, List<string> accessibleVenues, bool isSuperAdmin, bool getUpcoming=false);
        Task<VenueOneOffer> AssignOffer(string id, Offer offer);
        Task<bool> UnAssignOffer(string id, string offerId);
        Task<bool> UpdateAssignedOffer(string id, Offer offer);
        Task<List<VenueOneOffer>> GetOfferByVenueId(string venueId);
        Task<bool> UnAssignOffersFromVenue(string venueId);
        Task<Page<VenueOneOffer>> GetAssignedUpcomingOffersPage(PaginationRequest paginationRequest, FilterationRequest filterRequest, List<string> accessibleVenues, bool isSuperAdmin);
    }
}
