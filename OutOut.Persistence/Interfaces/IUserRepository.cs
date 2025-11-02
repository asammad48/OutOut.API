using MongoDB.Driver;
using OutOut.Models.Identity;
using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Interfaces
{
    public interface IUserRepository
    {
        Task AssignFirebaseMessagingTokenToUser(string userId, string fcmToken);
        Task UnassignFirebaseMessagingTokenFromUser(string userId, string fcmToken);
        Task<List<string>> GetAllFirebaseMessagingTokens();
        Task<List<string>> GetFirebaseMessagingTokens(string userId);
        Task<List<ApplicationUser>> GetInactiveUsers();
        Task<bool> AddToSharedTickets(string id, SharedTicket sharedTicket);
        Task<bool> UpdateLastUsageDate(string id);
        Task<Page<ApplicationUser>> GetUsers(PaginationRequest paginationRequest, FilterationRequest filterRequest, string userId);
        Task<Page<ApplicationUser>> GetCustomersPage(PaginationRequest paginationRequest, FilterationRequest filterRequest);
        Task<Page<ApplicationUser>> GetCustomersByIds(PaginationRequest paginationRequest, FilterationRequest sortRequest, List<string> ids);
        Task<ApplicationUser> GetUserById(string id);
        FilterDefinition<ApplicationUser> GetDistance(Location location);
        Task<bool> AddVenueIdToAccessibleVenues(string userId, string venueId);
        Task<bool> AddEventIdToAccessibleEvents(string userId, string eventId);
        Task<bool> DeleteEventIdFromAccessibleEvents(string eventId);
        Task<bool> DeleteVenueIdFromAccessibleVenues(string venueId);
        bool EmailExistsForOtherUsers(string userId, string email);
    }
}
