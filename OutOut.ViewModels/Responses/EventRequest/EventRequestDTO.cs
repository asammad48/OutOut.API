using OutOut.ViewModels.Responses.Events;
using OutOut.ViewModels.Responses.Venues;

namespace OutOut.ViewModels.Responses.EventRequest
{
    public class EventRequestDTO
    {
        public string Id { get; set; }
        public LastModificationRequestDTO lastModificationRequest { get; set; }
        public FullEventResponse Event { get; set; }
    }
}
