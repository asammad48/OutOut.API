using OutOut.ViewModels.Responses.Venues;

namespace OutOut.ViewModels.Responses.Events
{
    public class EventMiniSummaryResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public LocationResponse Location { get; set; }
    }
}
