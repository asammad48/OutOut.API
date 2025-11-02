using OutOut.ViewModels.Enums;
using System.Collections.Generic;

namespace OutOut.ViewModels.Requests.Venues
{
    public class VenueFilterationRequest
    {
        public string SearchQuery { get; set; }
        public VenueTimeFilter? TimeFilter { get; set; }
        public List<string> CategoriesIds { get; set; }
    }
}
