using OutOut.Constants.Enums;
using OutOut.Models.Domains;
using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Interfaces
{
    public interface IVenueRequestRepository : IGenericNonSqlRepository<VenueRequest>
    {
        Task<bool> UpsertVenueRequest(Venue updatedVenue, Venue oldVenue, RequestType type, string modifiedId);
        Task<Page<VenueRequest>> GetVenueRequests(PaginationRequest paginationRequest, FilterationRequest filterRequest, string createdBy = null);
        Task<VenueRequest> GetVenueRequestById(string id);
        Task<VenueRequest> GetVenueRequestByVenueId(string venueId, RequestType type, string modifiedFieldId = null);
        Task<VenueRequest> GetVenueRequest(string venueId, RequestType type, string requestCreator);
        Task<List<UnwindVenueRequestOffer>> GetOffersByRequestId(string requetsId);
        Task<bool> DeleteVenueRequest(string id);
        Task<bool> DeleteVenueRequest(string venueId, RequestType type);
        Task<bool> DeleteVenueRequest(string venueId, RequestType type, string modifiedFieldId);
        Task<bool> RequestUpdateAssignedOffer(string id, Offer offer);
        Task<bool> ApproveVenue(string requestId, Venue venue);
        Task<bool> DeleteVenueRequestsByType(RequestType type);
        Task<bool> DeleteGalleryImages(string requestId, List<string> images);
    }
}
