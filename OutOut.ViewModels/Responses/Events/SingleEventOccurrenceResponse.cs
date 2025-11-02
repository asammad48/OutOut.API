using OutOut.ViewModels.Responses.Categories;
using OutOut.ViewModels.Responses.EventBookings;
using OutOut.ViewModels.Responses.Venues;
using System.Collections.Generic;

namespace OutOut.ViewModels.Responses.Events
{
    public class SingleEventOccurrenceResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public LocationResponse Location { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public List<CategoryResponse> Categories { get; set; }
        public string FacebookLink { get; set; }
        public string InstagramLink { get; set; }
        public string YoutubeLink { get; set; }
        public string WebpageLink { get; set; }
        public bool IsFeatured { get; set; }
        public EventOccurrenceResponse Occurrence { get; set; }
        public List<EventOccurrenceResponse> Occurrences { get; set; }
        public bool IsFavorite { get; set; }
        public VenueMiniSummaryResponse Venue { get; set; }
        public EventBookingMiniSummaryResponse Booking { get; set; }
    }
}
