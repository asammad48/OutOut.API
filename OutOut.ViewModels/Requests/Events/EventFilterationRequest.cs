using OutOut.ViewModels.Enums;
using System.Collections.Generic;

namespace OutOut.ViewModels.Requests.Events
{
    public class EventFilterationRequest
    {
        public string SearchQuery { get; set; }
        public EventFilter? EventFilter { get; set; }
        public List<string> CategoriesIds { get; set; }
    }
}
