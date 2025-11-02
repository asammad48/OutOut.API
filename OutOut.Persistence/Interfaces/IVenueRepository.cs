using OutOut.Constants.Enums;
using OutOut.Models.Domain;
using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.ViewModels.Requests.Areas;
using OutOut.ViewModels.Requests.HomePage;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Requests.Venues;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Interfaces
{
    public interface IVenueRepository : IGenericNonSqlRepository<Venue>
    {
        Page<VenueWithDistance> GetVenues(PaginationRequest paginationRequest, UserLocation userLocation, VenueFilterationRequest filterRequest);
        Task<List<Venue>> GetUsersFavoriteVenues(List<string> venuesIds, SearchFilterationRequest filterRequest);
        Task<Venue> GetByEventId(string eventId);
        Task<Venue> GetByLoyaltyId(string loyaltyId);
        Task<Venue> GetByOfferId(string offerId);
        Task<VenueOneOffer> GetVenueByOfferId(string offerId);
        Task<List<Venue>> HomeFilter(UserLocation userLocation, HomePageFilterationRequest filterRequest);
        Task<Page<Venue>> DashboardFilter(PaginationRequest paginationRequest, HomePageWebFilterationRequest filterRequest, List<string> accessibleVenues, bool isSuperAdmin);
        Task<Page<Venue>> GetVenuesByUserId(PaginationRequest paginationRequest, string userId);
        Task<List<Venue>> GetNewestVenues(List<string> accessibleVenues, bool isSuperAdmin);
        Task<bool> DeleteLocationFromVenue(string cityId, string area = null);
        Task<bool> UpdateVenuesArea(string cityId, UpdateAreaRequest request);
        Task<List<Venue>> GetVenuesByCityId(string cityId, string area = null);
        Task DeleteCategory(string categoryId);
        Task DeleteLoyalty(string loyaltyId);
        Task DeleteOffer(string offerId);
        Task<bool> UpdateTermsAndConditions(string id, List<string> selectedTermsAndConditions);
        Task DeleteTermsAndConditions(string termsAndConditionsId);
        long GetAssignedLoyaltyCount(string loyaltyTypeId);
        Task<Page<Venue>> GetVenuesPage(PaginationRequest paginationRequest, FilterationRequest filterRequest, List<string> accessibleVenues, bool isSuperAdmin);
        Task<bool> UpdateVenueCode(string id, string code);
        Task<List<Venue>> GetAllVenues(SearchFilterationRequest searchFilterationRequest, List<string> accessibleVenues, bool isSuperAdmin);
        Task<List<Venue>> GetActiveAccessibleVenues(SearchFilterationRequest searchFilterationRequest, List<string> accessibleVenues, bool isSuperAdmin);
        Task<List<Venue>> GetActiveVenues(SearchFilterationRequest searchFilterationRequest);
        Task<bool> UpdateVenueStatus(string venueId, Availability status);
        Task<Page<Venue>> GetVenuesWithAssignedLoyalty(PaginationRequest paginationRequest, FilterationRequest filterRequest, List<string> accessibleVenues, bool isSuperAdmin);
        Task<Venue> AssignLoyalty(string id, Loyalty loyalty);
        Task<bool> UnAssignLoyalty(string id);
        Task<bool> UpdateAssignedLoyalty(string id, Loyalty loyalty);
        Task<List<Venue>> GetActiveVenuesWithNoLoyalty(List<string> accessibleVenues, bool isSuperAdmin);
        Task<List<Venue>> GetActiveVenuesWithNoLoyaltyToAllAdmins();
        Task<bool> AddEventToVenue(string venueId, string eventId);
        Task<bool> RemoveEventFromVenue(string venueId, string eventId);
        Task<bool> UpdateAssignedLoyaltyStatus(string id, bool isActive);
        Task<bool> UpdateAssignedOffersStatus(string id, bool isActive);
        Task<bool> RemoveEventFromOldAssignedVenues(string newAssignedVenueId, string eventId);
        Task<Page<Venue>> GetVenuesUserAdminOn(PaginationRequest paginationRequest, List<string> venuesIds);
        Task<bool> DeleteGalleryImages(string id, List<string> images);
        Venue GetVenueById(string id);
        List<Venue> GetVenuesByIds(List<string> ids);
        Task<List<VenueOneOffer>> GetOffersByVenueId(string id);
        Task<bool> UnAssignAllLoyalty();
        Task<bool> UnAssignAllOffers();
    }
}
