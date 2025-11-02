using OutOut.ViewModels.Responses.Categories;
using System.Collections.Generic;

namespace OutOut.ViewModels.Responses.Venues
{
    public class VenueSummaryWithBookingResponse
    {
        public string Id { get; set; }
        public string Logo { get; set; }
        public string DetailsLogo { get; set; }
        public string TableLogo { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<AvailableTimeResponse> OpenTimes { get; set; }
        public LocationResponse Location { get; set; }
        public List<CategoryResponse> Categories { get; set; }
        public long Count { get; set; } //Booking Count
        public string PhoneNumber { get; set; }
        public string Status { get; set; }
    }
}
