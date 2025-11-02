using OutOut.Constants.Enums;
using OutOut.Models.Domains;
using OutOut.Models.Models;
using OutOut.Models.Models.Embedded;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.ViewModels.Requests.Areas;
using OutOut.ViewModels.Requests.Events;
using OutOut.ViewModels.Requests.HomePage;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Requests.Venues;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Interfaces
{
    public interface IEventRepository : IGenericNonSqlRepository<Event>
    {
        Event GetEventById(string id);
        Task<Event> CreateEventWithOccurrences(Event newEvent);
        SingleEventOccurrence GetSingleEventOccurrenceById(string eventOccurrenceId);
        Task<Event> GetEventByOccurrenceId(string eventOccurrenceId);
        Task<List<SingleEventOccurrence>> GetEvents(EventFilterationRequest filterRequest, UserLocation userLocation);
        Task<List<SingleEventOccurrence>> GetUpcomingEvents(List<string> eventIds);
        Task<List<SingleEventOccurrence>> GetUsersFavoriteEvents(List<string> eventIds, SearchFilterationRequest filterRequest);
        Task<List<SingleEventOccurrence>> HomeFilter(HomePageFilterationRequest filterRequest);
        Task<List<SingleEventOccurrence>> DashboardFilter(HomePageWebFilterationRequest filterRequest, List<string> accessibleEvents, bool isSuperAdmin);
        Task<Event> UpdatePackageRemainingTickets(string eventOccurrenceId, string packageId, int ticketsQuantity);
        Task<Page<SingleEventOccurrence>> GetEventsByUserId(PaginationRequest paginationRequest, string userId);
        Task<List<SingleEventOccurrence>> GetUpcomingEvents(List<string> accessibleEvents, bool isSuperAdmin);
        Task<List<SingleEventOccurrence>> GetOngoingEvents(List<string> accessibleEvents, bool isSuperAdmin);
        Task<bool> DeleteLocationFromEvent(string cityId, string area = null);
        Task<bool> UpdateEventsArea(string cityId, UpdateAreaRequest request);
        Task DeleteCategory(string categoryId);
        Task<List<SingleEventOccurrence>> GetUpcomingEvents(List<string> eventIds, List<string> accessibleEvents, bool isSuperAdmin);
        Task<List<Event>> GetAllEvents(SearchFilterationRequest searchFilterationRequest, List<string> accessibleEvents, bool isSuperAdmin);
        Task<List<Event>> GetAllEventsNotAssociatedToVenue(string venueId, SearchFilterationRequest searchFilterationRequest, List<string> accessibleEvents, bool isSuperAdmin);
        Task<Page<SingleEventOccurrence>> GetAllEventsOccurrences(PaginationRequest paginationRequest, FilterationRequest filterRequest, List<string> accessibleEvents, bool isSuperAdmin);
        Task<Page<SingleEventOccurrence>> GetFeaturedEventsPage(PaginationRequest paginationRequest, FilterationRequest filterRequest);
        Task<bool> UpdateIsFeatured(string id, bool isFeatured);
        Task<bool> UnsetAllFeaturedEvents();
        Task<bool> UpdateEventCode(string id, string code);
        Task<bool> UpdateEventsStatus(List<string> ids, Availability status);
        Task<List<Event>> GetEventsByCityId(string cityId, string area = null);
        Task<bool> UpdateAssignedVenue(string id, VenueSummary venue);
    }
}
