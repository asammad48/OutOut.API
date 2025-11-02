using OutOut.ViewModels.Responses.Venues;

namespace OutOut.ViewModels.Responses.Events
{
    public class EventSummaryResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string TableLogo { get; set; }
        public string DetailsLogo { get; set; }
        public string Description { get; set; }
        public LocationResponse Location { get; set; }
        public EventOccurrenceResponse Occurrence { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsFeatured { get; set; }
        public string EventVenueName { get; set; }
        public string VenueName { get; set; }
    }
}
