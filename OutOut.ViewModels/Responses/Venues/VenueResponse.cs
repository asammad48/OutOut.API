using OutOut.ViewModels.Responses.Categories;
using OutOut.ViewModels.Responses.Events;
using OutOut.ViewModels.Responses.Offers;
using OutOut.ViewModels.Responses.VenueBooking;
using System.Collections.Generic;

namespace OutOut.ViewModels.Responses.Venues
{
    public class VenueResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Logo { get; set; }
        public string DetailsLogo { get; set; }
        public string TableLogo { get; set; }
        public string Background { get; set; }
        public LocationResponse Location { get; set; }
        public List<CategoryResponse> Categories { get; set; }
        public List<AvailableTimeResponse> OpenTimes { get; set; }
        public string PhoneNumber { get; set; }
        public string Menu { get; set; }
        public List<string> Gallery { get; set; }
        public string FacebookLink { get; set; }
        public string InstagramLink { get; set; }
        public string YoutubeLink { get; set; }
        public string WebpageLink { get; set; }
        public bool IsFavorite { get; set; }
        public List<OfferResponse> Offers { get; set; }
        public List<OfferResponse> UpcomingOffers { get; set; }

        public VenueLoyaltySummaryResponse Loyalty { get; set; }
        public VenueBookingSummaryResponse Booking { get; set; }
        public List<EventSummaryResponse> UpcomingEvents { get; set; }
    }
}
