using OutOut.ViewModels.Responses.Categories;
using System.Collections.Generic;

namespace OutOut.ViewModels.Responses.Venues
{
    public class VenueSummaryResponse
    {
        public string Id { get; set; }
        public string Logo { get; set; }
        public string DetailsLogo { get; set; }
        public string TableLogo { get; set; }
        public string Name { get; set; }
        public List<AvailableTimeResponse> OpenTimes { get; set; }
        public LocationResponse Location { get; set; }
        public List<CategoryResponse> Categories { get; set; }
        public bool IsFavorite { get; set; }
        public string PhoneNumber { get; set; }
    }
}