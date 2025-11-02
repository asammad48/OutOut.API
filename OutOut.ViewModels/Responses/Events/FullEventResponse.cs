using OutOut.ViewModels.Responses.Categories;
using OutOut.ViewModels.Responses.Venues;
using System.Collections.Generic;

namespace OutOut.ViewModels.Responses.Events
{
    public class FullEventResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string HeaderImage { get; set; }
        public string TableLogo { get; set; }
        public string DetailsLogo { get; set; }
        public string Description { get; set; }
        public List<CategoryResponse> Categories { get; set; }
        public LocationResponse Location { get; set; }
        public string FacebookLink { get; set; }
        public string InstagramLink { get; set; }
        public string YoutubeLink { get; set; }
        public string WebpageLink { get; set; }
        public List<EventOccurrenceResponse> Occurrences { get; set; }
        public List<EventPackageResponse> Packages { get; set; }
        public bool IsFeatured { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Code { get; set; }
        public long BookedTicketsCount { get; set; }
        public long RemainingTicketsCount { get; set; }
        public long PendingTicketsCount { get; set; }
        public double Revenue { get; set; }
        public string Status { get; set; }
        public VenueMiniSummaryResponse Venue { get; set; }
    }
}
