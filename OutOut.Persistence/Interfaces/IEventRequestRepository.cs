using OutOut.Constants.Enums;
using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Interfaces
{
    public interface IEventRequestRepository : IGenericNonSqlRepository<EventRequest>
    {
        Task<bool> UpsertEventRequest(Event eventObject, Event oldEvent, RequestType type, string modifiedId);
        Task<bool> ApproveEvent(string requestId, Event eventRequest);
        Task<bool> DeleteEventRequest(string id);
        Task<Page<EventRequest>> GetAllEventsRequests(PaginationRequest paginationRequest, FilterationRequest filterRequest, string createdBy = null);
        Task<EventRequest> GetEventRequestById(string id);
        Task<EventRequest> GetEventRequestByEventId(string eventId, RequestType type);
        Task<bool> AddOccurrenceToEvent(string id, EventOccurrence eventOccurrence);
        Task<bool> UpdateOccurrenceDateTime(string eventOccurrenceId, EventOccurrence occurrence);
        Task<bool> DeleteOccurrenceFromEvent(string id, string occurrenceId);
        Task<bool> DeleteOccurrenceFromEvent(string id, List<string> occurrenceId);
    }
}
